from __future__ import annotations

import argparse
import os
import posixpath
import secrets
import string
import sys
from pathlib import Path

import paramiko


EXCLUDE_DIRS = {
    ".git",
    ".idea",
    ".vscode",
    ".vs",
    ".dotnet",
    "__pycache__",
    "bin",
    "dist",
    "node_modules",
    "obj",
}

EXCLUDE_FILES = {
    ".env",
}


def parse_env_content(content: str) -> dict[str, str]:
    values: dict[str, str] = {}
    for raw_line in content.splitlines():
        line = raw_line.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        key, value = line.split("=", 1)
        values[key.strip()] = value.strip()
    return values


def generate_password(length: int = 24) -> str:
    alphabet = string.ascii_letters + string.digits
    while True:
        value = "".join(secrets.choice(alphabet) for _ in range(length - 3))
        password = "Aa1" + value
        if (
            any(ch.islower() for ch in password)
            and any(ch.isupper() for ch in password)
            and any(ch.isdigit() for ch in password)
        ):
            return password


def generate_secret(length: int = 48) -> str:
    return secrets.token_urlsafe(length)


def print_step(message: str) -> None:
    write_line(sys.stdout, f"[deploy] {message}")


def write_line(stream: object, message: str = "") -> None:
    text = f"{message}\n"
    buffer = getattr(stream, "buffer", None)
    if buffer is not None:
        buffer.write(text.encode("utf-8", errors="replace"))
        buffer.flush()
    else:
        print(text, end="", file=stream)


def run_command(
    client: paramiko.SSHClient,
    command: str,
    *,
    check: bool = True,
    timeout: int = 120,
) -> tuple[int, str, str]:
    print_step(f"remote> {command}")
    stdin, stdout, stderr = client.exec_command(command, timeout=timeout, get_pty=True)
    exit_code = stdout.channel.recv_exit_status()
    out = stdout.read().decode("utf-8", errors="replace")
    err = stderr.read().decode("utf-8", errors="replace")
    if out.strip():
        write_line(sys.stdout, out.strip())
    if err.strip():
        write_line(sys.stderr, err.strip())
    if check and exit_code != 0:
        raise RuntimeError(f"Remote command failed with exit code {exit_code}: {command}")
    return exit_code, out, err


def ensure_remote_dir(sftp: paramiko.SFTPClient, remote_dir: str) -> None:
    parts = [part for part in remote_dir.split("/") if part]
    current = "/"
    for part in parts:
        current = posixpath.join(current, part)
        try:
            sftp.stat(current)
        except FileNotFoundError:
            sftp.mkdir(current)


def upload_file(sftp: paramiko.SFTPClient, local_path: Path, remote_path: str) -> None:
    ensure_remote_dir(sftp, posixpath.dirname(remote_path))
    sftp.put(str(local_path), remote_path)


def upload_tree(sftp: paramiko.SFTPClient, local_root: Path, remote_root: str) -> None:
    ensure_remote_dir(sftp, remote_root)
    for root, dirs, files in os.walk(local_root):
        dirs[:] = [d for d in dirs if d not in EXCLUDE_DIRS]
        root_path = Path(root)
        relative_root = root_path.relative_to(local_root)
        remote_dir = remote_root
        if str(relative_root) != ".":
            remote_dir = posixpath.join(remote_root, relative_root.as_posix())
            ensure_remote_dir(sftp, remote_dir)

        for file_name in files:
            if file_name in EXCLUDE_FILES:
                continue
            local_path = root_path / file_name
            remote_path = posixpath.join(remote_dir, file_name)
            print_step(f"upload {local_path.relative_to(local_root).as_posix()}")
            upload_file(sftp, local_path, remote_path)


def write_remote_text(sftp: paramiko.SFTPClient, remote_path: str, content: str) -> None:
    ensure_remote_dir(sftp, posixpath.dirname(remote_path))
    with sftp.file(remote_path, "w") as remote_file:
        remote_file.write(content)


def read_remote_text(sftp: paramiko.SFTPClient, remote_path: str) -> str | None:
    try:
        with sftp.file(remote_path, "r") as remote_file:
            return remote_file.read().decode("utf-8", errors="replace")
    except FileNotFoundError:
        return None


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--host", required=True)
    parser.add_argument("--user", required=True)
    parser.add_argument("--password", required=True)
    parser.add_argument("--admin-password")
    parser.add_argument(
        "--local-root",
        default=str(Path(__file__).resolve().parents[1]),
    )
    parser.add_argument("--remote-root", default="/opt/pawnshop-v2")
    args = parser.parse_args()

    local_root = Path(args.local_root).resolve()

    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())

    print_step(f"connecting to {args.user}@{args.host}")
    client.connect(
        hostname=args.host,
        username=args.user,
        password=args.password,
        timeout=30,
        look_for_keys=False,
        allow_agent=False,
    )

    try:
        run_command(client, "uname -a")
        run_command(client, "free -h")
        run_command(
            client,
            "if [ -z \"$(swapon --show=NAME --noheadings)\" ]; then "
            "fallocate -l 2G /swapfile || dd if=/dev/zero of=/swapfile bs=1M count=2048; "
            "chmod 600 /swapfile; mkswap /swapfile; swapon /swapfile; "
            "grep -q '^/swapfile ' /etc/fstab || echo '/swapfile none swap sw 0 0' >> /etc/fstab; "
            "fi",
            timeout=180,
        )
        run_command(client, "free -h")
        run_command(
            client,
            "if ! command -v docker >/dev/null 2>&1; then "
            "export DEBIAN_FRONTEND=noninteractive; "
            "apt-get update && apt-get install -y docker.io docker-compose-v2; "
            "systemctl enable --now docker; "
            "fi",
            timeout=900,
        )
        run_command(client, "docker --version")
        run_command(client, "docker compose version")
        run_command(client, f"mkdir -p {args.remote_root}")

        sftp = client.open_sftp()
        try:
            existing_env_content = read_remote_text(sftp, f"{args.remote_root}/.env")
            existing_env = parse_env_content(existing_env_content or "")

            admin_password = (
                args.admin_password
                or existing_env.get("ADMIN_DEFAULT_PASSWORD")
                or generate_password(18)
            )
            jwt_secret = existing_env.get("JWT_SECRET_KEY") or generate_secret(48)

            env_content = "\n".join(
                [
                    f"DATABASE_PROVIDER={existing_env.get('DATABASE_PROVIDER', 'Sqlite')}",
                    f"DEFAULT_CONNECTION_STRING={existing_env.get('DEFAULT_CONNECTION_STRING', 'Data Source=/app/data/pawnshopv2.db')}",
                    f"JWT_SECRET_KEY={jwt_secret}",
                    f"JWT_ISSUER={existing_env.get('JWT_ISSUER', 'PawnShopV2')}",
                    f"JWT_AUDIENCE={existing_env.get('JWT_AUDIENCE', 'PawnShopV2Users')}",
                    f"JWT_ACCESS_TOKEN_EXPIRATION_MINUTES={existing_env.get('JWT_ACCESS_TOKEN_EXPIRATION_MINUTES', '60')}",
                    f"ADMIN_DEFAULT_PASSWORD={admin_password}",
                    "",
                ]
            )

            upload_tree(sftp, local_root, args.remote_root)
            write_remote_text(sftp, f"{args.remote_root}/.env", env_content)
        finally:
            sftp.close()

        compose_cmd = "docker compose"
        run_command(
            client,
            f"cd {args.remote_root} && {compose_cmd} --ansi never down --remove-orphans || true",
            check=False,
            timeout=240,
        )
        run_command(
            client,
            f"cd {args.remote_root} && {compose_cmd} --ansi never up --build -d",
            timeout=1800,
        )
        run_command(client, f"cd {args.remote_root} && {compose_cmd} ps", timeout=120)
        run_command(client, "docker ps --format 'table {{.Names}}\\t{{.Status}}\\t{{.Ports}}'")
        run_command(
            client,
            "for i in $(seq 1 20); do curl -fsS --max-time 20 http://127.0.0.1 >/dev/null && exit 0; sleep 3; done; exit 1",
            timeout=90,
        )
        run_command(
            client,
            "for i in $(seq 1 20); do curl -fsS --max-time 20 http://127.0.0.1:5000/swagger/index.html >/dev/null && exit 0; sleep 3; done; exit 1",
            timeout=90,
        )

        write_line(sys.stdout)
        write_line(sys.stdout, "DEPLOYMENT_RESULT")
        write_line(sys.stdout, f"SERVER_URL=http://{args.host}")
        write_line(sys.stdout, f"SWAGGER_URL=http://{args.host}:5000/swagger")
        write_line(sys.stdout, "ADMIN_USERNAME=admin")
        write_line(sys.stdout, f"ADMIN_PASSWORD={admin_password}")
        return 0
    finally:
        client.close()


if __name__ == "__main__":
    raise SystemExit(main())

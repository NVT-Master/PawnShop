@echo off
setlocal

if not exist .env (
    echo .env file not found. Create it from .env.example before deploying.
    exit /b 1
)

docker compose version >nul 2>&1
if %errorlevel%==0 (
    set "COMPOSE_CMD=docker compose"
) else (
    docker-compose version >nul 2>&1
    if %errorlevel%==0 (
        set "COMPOSE_CMD=docker-compose"
    ) else (
        echo Docker Compose is not installed.
        exit /b 1
    )
)

echo === PawnShop V2 Docker Deployment ===
echo Using compose command: %COMPOSE_CMD%

echo Stopping existing containers...
%COMPOSE_CMD% down

echo Building and starting containers...
%COMPOSE_CMD% up --build -d

echo Waiting for services to start...
timeout /t 10 /nobreak

echo.
echo === Container Status ===
%COMPOSE_CMD% ps

echo.
echo === Deployment Complete ===
echo Frontend: http://localhost
echo Backend API: http://localhost:5000
echo Swagger: http://localhost:5000/swagger
echo.
echo Default login: admin / Admin@123
pause

#!/bin/bash

set -euo pipefail

if docker compose version >/dev/null 2>&1; then
  COMPOSE_CMD="docker compose"
elif docker-compose version >/dev/null 2>&1; then
  COMPOSE_CMD="docker-compose"
else
  echo "Docker Compose is not installed."
  exit 1
fi

if [ ! -f .env ]; then
  echo ".env file not found. Create it from .env.example before deploying."
  exit 1
fi

echo "=== PawnShop V2 Docker Deployment ==="
echo "Using compose command: ${COMPOSE_CMD}"

echo "Stopping existing containers..."
${COMPOSE_CMD} down

echo "Building and starting containers..."
${COMPOSE_CMD} up --build -d

echo "Waiting for services to start..."
sleep 10

echo ""
echo "=== Container Status ==="
${COMPOSE_CMD} ps

echo ""
echo "=== Deployment Complete ==="
echo "Frontend: http://localhost"
echo "Backend API: http://localhost:5000"
echo "Swagger: http://localhost:5000/swagger"
echo ""
echo "Default login: admin / Admin@123"

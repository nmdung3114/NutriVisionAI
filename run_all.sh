#!/usr/bin/env bash
# NutriVision AI - chay SQL Server (Docker) + backend (http://localhost:5000) + frontend (http://localhost:3000).
# Ctrl+C de dung backend/frontend (SQL Server van chay nen; dung bang: docker compose stop).
# Khong co Docker: tu dong chay bang SQLite (profile http-sqlite).
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
[ -d "$ROOT/src/nutrivision-web/node_modules" ] || "$ROOT/setup.sh"

PROFILE=http
if command -v docker >/dev/null && docker info >/dev/null 2>&1; then
  echo "==> Khoi dong SQL Server (Docker)..."
  (cd "$ROOT" && docker compose up -d --wait)
else
  echo "==> Khong dung duoc Docker -> chay bang SQLite (App_Data/nutrivision-dev.db)"
  PROFILE=http-sqlite
fi

(cd "$ROOT/src/NutriVision.Api" && exec dotnet run --launch-profile "$PROFILE") &
API_PID=$!
(cd "$ROOT/src/nutrivision-web" && exec npm run dev) &
WEB_PID=$!
trap 'kill $API_PID $WEB_PID 2>/dev/null' EXIT INT TERM

echo "  - Web UI:  http://localhost:3000"
echo "  - Swagger: http://localhost:5000/swagger"
echo "  - Tai khoan demo: user@nutrivision.ai / User@123 ; admin@nutrivision.ai / Admin@123"
wait

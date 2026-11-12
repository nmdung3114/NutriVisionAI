#!/usr/bin/env bash
# NutriVision AI - chay backend (http://localhost:5000) + frontend (http://localhost:3000). Ctrl+C de dung ca hai.
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
[ -d "$ROOT/src/nutrivision-web/node_modules" ] || "$ROOT/setup.sh"

(cd "$ROOT/src/NutriVision.Api" && exec dotnet run --launch-profile http) &
API_PID=$!
(cd "$ROOT/src/nutrivision-web" && exec npm run dev) &
WEB_PID=$!
trap 'kill $API_PID $WEB_PID 2>/dev/null' EXIT INT TERM

echo "  - Web UI:  http://localhost:3000"
echo "  - Swagger: http://localhost:5000/swagger"
echo "  - Tai khoan demo: user@nutrivision.ai / User@123 ; admin@nutrivision.ai / Admin@123"
wait

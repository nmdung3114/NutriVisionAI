#!/usr/bin/env bash
# NutriVision AI - cai dat toan bo package can thiet (Linux / macOS)
#   ./setup.sh           cai dat package backend + frontend
#   ./setup.sh --verify  cai dat roi build + chay toan bo test
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
WEB="$ROOT/src/nutrivision-web"

step() { printf '\n==> %s\n' "$1"; }
fail() { printf '\n[LOI] %s\n' "$1" >&2; exit 1; }

step "Kiem tra cong cu"
command -v dotnet >/dev/null || fail "Chua cai .NET SDK 8: https://dotnet.microsoft.com/download/dotnet/8.0"
dotnet --list-sdks | grep -qE '^(8|9|10|11)\.' || fail "Can .NET SDK 8 tro len (hien co: $(dotnet --list-sdks | tr '\n' ' '))"
command -v node >/dev/null || fail "Chua cai Node.js >= 20.9: https://nodejs.org"
NODE_MAJOR="$(node -p 'process.versions.node.split(".")[0]')"
[ "$NODE_MAJOR" -ge 20 ] || fail "Can Node.js >= 20.9 (hien co: $(node -v))"
echo "  .NET SDK: $(dotnet --version)   Node.js: $(node -v)   npm: $(npm -v)"

step "Backend: cong cu dotnet-ef (.config/dotnet-tools.json)"
(cd "$ROOT" && dotnet tool restore)

step "Backend: NuGet packages"
(cd "$ROOT" && dotnet restore NutriVision.sln)

step "Frontend: npm packages (theo package-lock.json)"
(cd "$WEB" && npm ci --no-audit --no-fund)

if [ ! -f "$WEB/.env.local" ]; then
  cp "$WEB/.env.local.example" "$WEB/.env.local"
  echo "  Da tao src/nutrivision-web/.env.local tu .env.local.example"
fi

if [ "${1:-}" = "--verify" ]; then
  step "Kiem tra: build + test backend"
  (cd "$ROOT" && dotnet build NutriVision.sln --no-restore && dotnet test NutriVision.sln --no-build)
  step "Kiem tra: lint + typecheck + test + build frontend"
  (cd "$WEB" && npm run lint && npm run typecheck && npm test && npm run build)
fi

step "Xong. Chay he thong: ./run_all.sh  (hoac xem README.md muc 2)"

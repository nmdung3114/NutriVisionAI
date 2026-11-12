@echo off
setlocal
rem NutriVision AI - cai dat toan bo package can thiet (Windows)
rem   setup.bat           cai dat package backend + frontend
rem   setup.bat --verify  cai dat roi build + chay toan bo test
set "ROOT=%~dp0"
set "WEB=%ROOT%src\nutrivision-web"

echo ==^> Kiem tra cong cu
where dotnet >nul 2>nul || (echo [LOI] Chua cai .NET SDK 8: https://dotnet.microsoft.com/download/dotnet/8.0 & goto :fail)
dotnet --list-sdks | findstr /R "^8\. ^9\. ^10\. ^11\." >nul || (echo [LOI] Can .NET SDK 8 tro len & goto :fail)
where node >nul 2>nul || (echo [LOI] Chua cai Node.js ^>= 20.9: https://nodejs.org & goto :fail)
node -e "process.exit(+process.versions.node.split('.')[0] >= 20 ? 0 : 1)" || (echo [LOI] Can Node.js ^>= 20.9 & goto :fail)
for /f %%v in ('dotnet --version') do echo   .NET SDK: %%v
for /f %%v in ('node -v') do echo   Node.js: %%v

echo.
echo ==^> Backend: cong cu dotnet-ef
pushd "%ROOT%" && dotnet tool restore || (popd & goto :fail)
echo.
echo ==^> Backend: NuGet packages
dotnet restore NutriVision.sln || (popd & goto :fail)
popd

echo.
echo ==^> Frontend: npm packages (theo package-lock.json)
pushd "%WEB%" && call npm ci --no-audit --no-fund || (popd & goto :fail)
if not exist ".env.local" (copy ".env.local.example" ".env.local" >nul & echo   Da tao .env.local tu .env.local.example)
popd

if /I "%~1"=="--verify" (
  echo.
  echo ==^> Kiem tra: build + test backend
  pushd "%ROOT%" && dotnet build NutriVision.sln --no-restore && dotnet test NutriVision.sln --no-build || (popd & goto :fail)
  popd
  echo.
  echo ==^> Kiem tra: lint + typecheck + test + build frontend
  pushd "%WEB%" && call npm run lint && call npm run typecheck && call npm test && call npm run build || (popd & goto :fail)
  popd
)

echo.
echo ==^> Xong. Chay he thong: run_all.bat
exit /b 0

:fail
echo.
echo Cai dat THAT BAI - xem loi o tren.
exit /b 1

@echo off
echo ===================================================
echo     NutriVision AI - Khoi dong He thong Web
echo ===================================================
echo.

echo [1/2] Dang khoi dong Backend ASP.NET Core API tai http://localhost:5000 ...
start "NutriVision API (.NET 8)" cmd /k "cd /d %~dp0src\NutriVision.Api && dotnet run --urls=http://localhost:5000"

timeout /t 3 /nobreak > nul

echo [2/2] Dang khoi dong Frontend Web (Next.js 14) tai http://localhost:3000 ...
start "NutriVision Web (Next.js 14)" cmd /k "cd /d %~dp0src\nutrivision-web && npm run dev"

echo.
echo ===================================================
echo   He thong da duoc khoi dong thanh cong!
echo   - Web UI: http://localhost:3000
echo   - API Swagger: http://localhost:5000/swagger
echo ===================================================
echo.
pause

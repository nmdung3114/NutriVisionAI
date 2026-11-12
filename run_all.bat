@echo off
echo ===================================================
echo     NutriVision AI - Khoi dong He thong Web
echo ===================================================
if not exist "%~dp0src\nutrivision-web\node_modules" (
  echo Chua cai package - chay setup.bat truoc...
  call "%~dp0setup.bat" || (pause & exit /b 1)
)
echo.
echo [1/2] Backend ASP.NET Core API tai http://localhost:5000 (Swagger: /swagger) ...
start "NutriVision API (.NET 8)" cmd /k "cd /d %~dp0src\NutriVision.Api && dotnet run --launch-profile http"
timeout /t 5 /nobreak > nul
echo [2/2] Frontend Next.js tai http://localhost:3000 ...
start "NutriVision Web (Next.js)" cmd /k "cd /d %~dp0src\nutrivision-web && npm run dev"
echo.
echo   - Web UI:  http://localhost:3000
echo   - Swagger: http://localhost:5000/swagger
echo   - Tai khoan demo: user@nutrivision.ai / User@123 ; admin@nutrivision.ai / Admin@123
pause

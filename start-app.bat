@echo off
echo ============================================
echo   Starting DFC Restaurant Management System
echo ============================================
echo.

echo Starting backend (.NET API) on port 7122...
start "DFC Backend" cmd /k "cd /d "E:\RMS-DFC-PROJECT\Backend" && dotnet run --launch-profile https"

timeout /t 3 /nobreak >nul

echo Starting shop management frontend (Angular) on port 4200...
start "DFC Shop Frontend" cmd /k "cd /d "E:\RMS-DFC-PROJECT\Frontend\RMS" && npm start"

timeout /t 3 /nobreak >nul

echo Starting customer ordering website (Angular) on port 4300...
start "DFC Customer Site" cmd /k "cd /d "E:\RMS-DFC-PROJECT\Frontend\CustomerOrderingWeb" && npx ng serve --port 4300"

echo.
echo Waiting for all servers to finish starting...
timeout /t 20 /nobreak >nul

start "" "http://localhost:4200"

echo.
echo ============================================
echo   Done. Three windows are now running:
echo     - "DFC Backend"        (keep open)
echo     - "DFC Shop Frontend"  (keep open)
echo     - "DFC Customer Site"  (keep open)
echo.
echo   Shop Management app: http://localhost:4200
echo   Customer Ordering site: http://localhost:4300
echo.
echo   Login (shop management app):
echo     Username: HassanAli
echo     Password: lup@fg3WT3
echo.
echo   To stop the app, just close all three windows
echo   (or press Ctrl+C inside each one).
echo ============================================
pause

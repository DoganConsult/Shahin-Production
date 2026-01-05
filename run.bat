@echo off
REM GRC System - Build & Run Script (Windows)
REM This script builds and runs the GRC application

setlocal enabledelayedexpansion

echo.
echo ================================================
echo  GRC System - Build and Run
echo ================================================
echo.

REM Step 1: Clean
echo [1/5] Cleaning project...
cd /d C:\path\to\grc-system
dotnet clean src\GrcMvc\GrcMvc.csproj -q >nul 2>&1
echo [OK] Project cleaned
echo.

REM Step 2: Restore
echo [2/5] Restoring packages...
dotnet restore src\GrcMvc\GrcMvc.csproj -q
echo [OK] Packages restored
echo.

REM Step 3: Build
echo [3/5] Building project...
dotnet build src\GrcMvc\GrcMvc.csproj -c Release -q
if !errorlevel! neq 0 (
    echo [FAILED] Build failed
    exit /b 1
)
echo [OK] Build successful
echo.

REM Step 4: Database Migration
echo [4/5] Applying database migrations...
cd src\GrcMvc
dotnet ef database update --context GrcDbContext -q >nul 2>&1
echo [OK] Database ready
echo.

REM Step 5: Run
echo [5/5] Starting application...
echo.
echo ================================================
echo  Application is running!
echo ================================================
echo.
echo  Open your browser at:
echo  https://localhost:5001
echo.
echo  Health Check:
echo  https://localhost:5001/health
echo.
echo  Login with:
echo  Email: Info@doganconsult.com
echo  Password: AhmEma$123456
echo.
echo  Press Ctrl+C to stop
echo ================================================
echo.

dotnet run

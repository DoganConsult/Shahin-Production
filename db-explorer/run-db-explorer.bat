@echo off
echo ========================================
echo Database Explorer - C# Console App
echo ========================================
echo.

echo Compiling DatabaseExplorer.csproj...
dotnet build DatabaseExplorer.csproj -o bin --nologo --verbosity quiet
if %errorlevel% neq 0 (
    echo.
    echo ❌ Compilation failed!
    echo.
    echo Make sure you have .NET SDK installed.
    echo Download from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo.
echo Running database explorer...
echo.
bin\DatabaseExplorer.exe %*

echo.
echo ========================================
echo Done!
echo ========================================
pause

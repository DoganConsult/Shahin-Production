# Run this as Administrator to stop PostgreSQL
Write-Host "Stopping PostgreSQL service..." -ForegroundColor Yellow
Stop-Service -Name 'postgresql-x64-18' -Force
Write-Host "PostgreSQL stopped." -ForegroundColor Green
pause

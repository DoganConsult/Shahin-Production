# Check Railway Deployment Status and Logs

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Railway Deployment Status Check" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Link to the application service
Write-Host "Linking to shahin-ai-producion service..." -ForegroundColor Yellow
railway link

Write-Host ""
Write-Host "Checking deployment status..." -ForegroundColor Yellow
railway status

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Recent Deployment Logs" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
railway logs --limit 100

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "To view live logs, run:" -ForegroundColor Green
Write-Host "railway logs --follow" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan

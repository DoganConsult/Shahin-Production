# Set environment variables for Shahin GRC
# Run this script before starting the application

# SMTP Password for Microsoft 365
$env:SMTP_PASSWORD = "AhmEma`$123456789"

# Verify
Write-Host "Environment variables set:" -ForegroundColor Green
Write-Host "  SMTP_PASSWORD: ****configured****" -ForegroundColor Cyan

# To make permanent (User level):
# [Environment]::SetEnvironmentVariable("SMTP_PASSWORD", "AhmEma`$123456789", "User")

Write-Host ""
Write-Host "To test email, run the app and call:" -ForegroundColor Yellow
Write-Host "  POST /api/emailtest/test" -ForegroundColor White
Write-Host "  Body: { `"to`": `"your-email@example.com`" }" -ForegroundColor White

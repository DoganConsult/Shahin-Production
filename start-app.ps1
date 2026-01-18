Write-Host "Starting Shahin GRC Platform..." -ForegroundColor Green

$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:8888"

Set-Location "c:\Shahin-ai\Shahin-Jan-2026\src\GrcMvc\publish-fresh"
dotnet GrcMvc.dll

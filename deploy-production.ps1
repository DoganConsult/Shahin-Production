# Production Deployment Script for GRC Platform
# Deploy to server with Cloudflare Tunnel

Write-Host "========================================" -ForegroundColor Green
Write-Host "GRC Platform Production Deployment" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

# Build for production
Write-Host "`n1. Building for production..." -ForegroundColor Yellow
Set-Location "C:\Shahin-ai\Shahin-Jan-2026\src\GrcMvc"
dotnet clean
dotnet publish -c Release -o ./publish

# Create deployment package
Write-Host "`n2. Creating deployment package..." -ForegroundColor Yellow
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$packageName = "grc-deploy-$timestamp.zip"
Compress-Archive -Path "./publish/*" -DestinationPath "../../../$packageName" -Force

Write-Host "`n3. Deployment package created: $packageName" -ForegroundColor Green

# Server deployment instructions
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "DEPLOYMENT INSTRUCTIONS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host @"

1. COPY PACKAGE TO SERVER:
   scp $packageName your-server:/var/www/grc-platform/

2. ON SERVER - EXTRACT AND DEPLOY:
   ssh your-server
   cd /var/www/grc-platform
   unzip $packageName -d release-$timestamp
   ln -sfn release-$timestamp current

3. SET ENVIRONMENT VARIABLES:
   export ASPNETCORE_ENVIRONMENT=Production
   export ASPNETCORE_URLS=http://0.0.0.0:8080
   export ConnectionStrings__DefaultConnection="Host=caboose.proxy.rlwy.net;Port=11527;Database=GrcMvcDb;Username=postgres;Password=QNcTvViWopMfCunsyIkkXwuDpufzhkLs;SSL Mode=Require"
   export ConnectionStrings__GrcAuthDb="Host=caboose.proxy.rlwy.net;Port=11527;Database=GrcMvcDb;Username=postgres;Password=QNcTvViWopMfCunsyIkkXwuDpufzhkLs;SSL Mode=Require"

4. CREATE SYSTEMD SERVICE:
   sudo nano /etc/systemd/system/grc-platform.service

   [Unit]
   Description=GRC Platform ASP.NET Core App
   After=network.target

   [Service]
   WorkingDirectory=/var/www/grc-platform/current
   ExecStart=/usr/bin/dotnet /var/www/grc-platform/current/GrcMvc.dll
   Restart=always
   RestartSec=10
   KillSignal=SIGINT
   SyslogIdentifier=grc-platform
   User=www-data
   Environment=ASPNETCORE_ENVIRONMENT=Production
   Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

   [Install]
   WantedBy=multi-user.target

5. START SERVICE:
   sudo systemctl daemon-reload
   sudo systemctl enable grc-platform
   sudo systemctl start grc-platform

6. CONFIGURE CLOUDFLARE TUNNEL:
   cloudflared tunnel create grc-platform
   cloudflared tunnel route dns grc-platform yourdomain.com
   cloudflared tunnel run --url http://localhost:8080 grc-platform

"@ -ForegroundColor White

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "Deployment package ready!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

# SSH Tunnel to UpCloud PostgreSQL Database
param(
    [string]$Host = "95.111.222.132",
    [string]$RemoteHost = "localhost",
    [int]$RemotePort = 5432,
    [int]$LocalPort = 5433,
    [string]$SshKeyPath = "c:\Shahin-ai\shahin_grc_key",
    [string]$User = "root"
)

Write-Host "🌉 Creating SSH Tunnel to UpCloud PostgreSQL" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "Local:  127.0.0.1:$LocalPort" -ForegroundColor Yellow
Write-Host "Remote: $RemoteHost`:$RemotePort on $Host" -ForegroundColor Yellow
Write-Host ""

# Verify SSH key
if (-not (Test-Path $SshKeyPath)) {
    Write-Host "❌ SSH key not found: $SshKeyPath" -ForegroundColor Red
    exit 1
}

Write-Host "✅ SSH key found" -ForegroundColor Green
Write-Host "🔄 Starting tunnel..." -ForegroundColor Cyan
Write-Host ""

# Create tunnel
ssh -i $SshKeyPath `
    -L "${LocalPort}:${RemoteHost}:${RemotePort}" `
    -N `
    "${User}@${Host}"

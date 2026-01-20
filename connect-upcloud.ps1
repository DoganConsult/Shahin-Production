# Direct SSH connection to UpCloud server
param(
    [string]$Host = "95.111.222.132",
    [string]$SshKeyPath = "c:\Shahin-ai\shahin_grc_key",
    [string]$User = "root",
    [int]$Port = 22
)

Write-Host "🔐 Connecting to UpCloud Server" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "Host: $Host" -ForegroundColor Yellow
Write-Host "User: $User" -ForegroundColor Yellow
Write-Host "Key: $SshKeyPath" -ForegroundColor Yellow
Write-Host ""

# Verify SSH key exists
if (-not (Test-Path $SshKeyPath)) {
    Write-Host "❌ SSH key not found: $SshKeyPath" -ForegroundColor Red
    exit 1
}

Write-Host "✅ SSH key found" -ForegroundColor Green
Write-Host "⏳ Connecting..." -ForegroundColor Cyan
Write-Host ""

# Connect via SSH
ssh -i $SshKeyPath -p $Port "${User}@${Host}"

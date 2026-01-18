#!/usr/bin/env pwsh
# Database Explorer - Quick launcher
# Connects to Railway database and explores all tables

param(
    [switch]$Schema,
    [switch]$SampleData,
    [string]$Table = "",
    [int]$Rows = 5
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Shahin GRC - Database Explorer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Set Railway DATABASE_URL if not already set
if (-not $env:DATABASE_URL) {
    Write-Host "Setting Railway DATABASE_URL..." -ForegroundColor Yellow
    $env:DATABASE_URL = "postgresql://postgres:VUykzDaybssURQkSAfxUYOBKBkDQSuVW@centerbeam.proxy.rlwy.net:11539/railway"
    Write-Host "✅ DATABASE_URL configured" -ForegroundColor Green
} else {
    Write-Host "✅ Using existing DATABASE_URL" -ForegroundColor Green
}

Write-Host ""

# Change to project directory
$projectPath = "Shahin-Jan-2026/src/GrcMvc"
if (-not (Test-Path $projectPath)) {
    Write-Host "❌ Project directory not found: $projectPath" -ForegroundColor Red
    exit 1
}

Set-Location $projectPath
Write-Host "📁 Working directory: $(Get-Location)" -ForegroundColor Gray
Write-Host ""

# Build command arguments
$cmdArgs = @("explore-db")

if ($Schema) {
    $cmdArgs += "--schema"
}

if ($SampleData) {
    $cmdArgs += "--sample-data"
}

if ($Table) {
    $cmdArgs += "--table=$Table"
}

# Run the database explorer
Write-Host "🚀 Running database explorer..." -ForegroundColor Cyan
Write-Host ""

try {
    dotnet run -- @cmdArgs
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ Exploration completed successfully!" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "⚠️  Exploration completed with warnings" -ForegroundColor Yellow
    }
} catch {
    Write-Host ""
    Write-Host "❌ Error: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Usage Examples:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  .\explore-db.ps1                          # List all tables" -ForegroundColor Gray
Write-Host "  .\explore-db.ps1 -Schema                  # Show table schemas" -ForegroundColor Gray
Write-Host "  .\explore-db.ps1 -SampleData -Table Tenants  # Show sample data" -ForegroundColor Gray
Write-Host ""

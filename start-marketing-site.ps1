# Start Marketing Site Script
# This script starts the Next.js marketing site on port 3000

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Starting Shahin AI Marketing Site" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if we're in the correct directory
$landingPagePath = "C:\Shahin-ai\landing-page"

if (-not (Test-Path $landingPagePath)) {
    Write-Host "ERROR: landing-page directory not found at $landingPagePath" -ForegroundColor Red
    exit 1
}

# Navigate to landing-page directory
Set-Location $landingPagePath
Write-Host "✓ Changed directory to: $landingPagePath" -ForegroundColor Green

# Check if node_modules exists
if (-not (Test-Path "node_modules")) {
    Write-Host ""
    Write-Host "Installing dependencies (this may take a few minutes)..." -ForegroundColor Yellow
    npm install
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: npm install failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Dependencies installed successfully" -ForegroundColor Green
} else {
    Write-Host "✓ Dependencies already installed" -ForegroundColor Green
}

# Check if port 3000 is already in use
Write-Host ""
Write-Host "Checking if port 3000 is available..." -ForegroundColor Yellow
$portCheck = netstat -ano | findstr ":3000"

if ($portCheck) {
    Write-Host "WARNING: Port 3000 is already in use!" -ForegroundColor Red
    Write-Host "Current processes using port 3000:" -ForegroundColor Yellow
    Write-Host $portCheck
    Write-Host ""
    Write-Host "Options:" -ForegroundColor Cyan
    Write-Host "1. Kill the process and restart this script" -ForegroundColor White
    Write-Host "2. Use a different port (modify the script)" -ForegroundColor White
    Write-Host ""
    $continue = Read-Host "Do you want to continue anyway? (y/n)"
    if ($continue -ne "y") {
        exit 0
    }
} else {
    Write-Host "✓ Port 3000 is available" -ForegroundColor Green
}

# Start the development server
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Starting Next.js Development Server" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "The site will be available at: http://localhost:3000" -ForegroundColor Green
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow
Write-Host ""

# Start the dev server
npm run dev

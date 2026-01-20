# ===========================================
# Shahin GRC Platform - Production Setup Script
# ===========================================
# Run this script to configure production environment

param(
    [Parameter(Mandatory=$false)]
    [string]$Environment = "Production",

    [Parameter(Mandatory=$false)]
    [switch]$SkipSecrets
)

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host " Shahin GRC Platform - Production Setup" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as admin
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "[WARNING] Running without administrator privileges" -ForegroundColor Yellow
}

# ===========================================
# 1. Environment Variables Setup
# ===========================================
Write-Host "[1/5] Setting up environment variables..." -ForegroundColor Green

if (-not $SkipSecrets) {
    # Prompt for sensitive values
    Write-Host ""
    Write-Host "Enter the following secrets (or press Enter to skip):" -ForegroundColor Yellow

    # Platform Admin Password
    $platformAdminPassword = Read-Host -Prompt "Platform Admin Password" -AsSecureString
    if ($platformAdminPassword.Length -gt 0) {
        $plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($platformAdminPassword))
        [Environment]::SetEnvironmentVariable("GRC_PLATFORM_ADMIN_PASSWORD", $plainPassword, "User")
        Write-Host "  - GRC_PLATFORM_ADMIN_PASSWORD set" -ForegroundColor Gray
    }

    # Database Connection String
    $dbConnection = Read-Host -Prompt "PostgreSQL Connection String (or Enter to skip)"
    if ($dbConnection) {
        [Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection", $dbConnection, "User")
        [Environment]::SetEnvironmentVariable("ConnectionStrings__GrcAuthDb", $dbConnection, "User")
        [Environment]::SetEnvironmentVariable("ConnectionStrings__HangfireConnection", $dbConnection, "User")
        Write-Host "  - Database connection strings set" -ForegroundColor Gray
    }

    # Redis Connection
    $redisConnection = Read-Host -Prompt "Redis Connection String (or Enter to skip)"
    if ($redisConnection) {
        [Environment]::SetEnvironmentVariable("ConnectionStrings__Redis", $redisConnection, "User")
        Write-Host "  - Redis connection string set" -ForegroundColor Gray
    }

    # SMTP Password
    $smtpPassword = Read-Host -Prompt "SMTP Password" -AsSecureString
    if ($smtpPassword.Length -gt 0) {
        $plainSmtp = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($smtpPassword))
        [Environment]::SetEnvironmentVariable("SMTP_PASSWORD", $plainSmtp, "User")
        Write-Host "  - SMTP_PASSWORD set" -ForegroundColor Gray
    }

    # Claude API Key
    $claudeApiKey = Read-Host -Prompt "Claude API Key (or Enter to skip)"
    if ($claudeApiKey) {
        [Environment]::SetEnvironmentVariable("CLAUDE_API_KEY", $claudeApiKey, "User")
        Write-Host "  - CLAUDE_API_KEY set" -ForegroundColor Gray
    }

    # NextAuth Secret
    $nextAuthSecret = Read-Host -Prompt "NextAuth Secret (or Enter to generate)"
    if (-not $nextAuthSecret) {
        $nextAuthSecret = [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
        Write-Host "  - Generated NextAuth secret" -ForegroundColor Gray
    }
    [Environment]::SetEnvironmentVariable("NEXTAUTH_SECRET", $nextAuthSecret, "User")
    Write-Host "  - NEXTAUTH_SECRET set" -ForegroundColor Gray
}

Write-Host "[OK] Environment variables configured" -ForegroundColor Green

# ===========================================
# 2. Backend Setup
# ===========================================
Write-Host ""
Write-Host "[2/5] Setting up backend..." -ForegroundColor Green

$backendPath = Join-Path $PSScriptRoot "src\GrcMvc"
if (Test-Path $backendPath) {
    Push-Location $backendPath

    # Restore packages
    Write-Host "  - Restoring NuGet packages..." -ForegroundColor Gray
    dotnet restore --verbosity quiet

    # Build
    Write-Host "  - Building backend..." -ForegroundColor Gray
    dotnet build --configuration Release --verbosity quiet

    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Backend built successfully" -ForegroundColor Green
    } else {
        Write-Host "[ERROR] Backend build failed" -ForegroundColor Red
    }

    Pop-Location
} else {
    Write-Host "[SKIP] Backend path not found: $backendPath" -ForegroundColor Yellow
}

# ===========================================
# 3. Frontend Setup
# ===========================================
Write-Host ""
Write-Host "[3/5] Setting up frontend..." -ForegroundColor Green

$frontendPath = Join-Path $PSScriptRoot "grc-frontend"
if (Test-Path $frontendPath) {
    Push-Location $frontendPath

    # Check if node_modules exists
    if (-not (Test-Path "node_modules")) {
        Write-Host "  - Installing npm packages..." -ForegroundColor Gray
        npm install --silent
    }

    # Build
    Write-Host "  - Building frontend..." -ForegroundColor Gray
    npm run build 2>$null

    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Frontend built successfully" -ForegroundColor Green
    } else {
        Write-Host "[WARNING] Frontend build had issues" -ForegroundColor Yellow
    }

    Pop-Location
} else {
    Write-Host "[SKIP] Frontend path not found: $frontendPath" -ForegroundColor Yellow
}

# ===========================================
# 4. Database Migration
# ===========================================
Write-Host ""
Write-Host "[4/5] Database setup..." -ForegroundColor Green

$backendPath = Join-Path $PSScriptRoot "src\GrcMvc"
if (Test-Path $backendPath) {
    Push-Location $backendPath

    $runMigration = Read-Host "Run database migrations? (y/N)"
    if ($runMigration -eq "y" -or $runMigration -eq "Y") {
        Write-Host "  - Running migrations..." -ForegroundColor Gray
        dotnet ef database update --configuration Release

        if ($LASTEXITCODE -eq 0) {
            Write-Host "[OK] Database migrations completed" -ForegroundColor Green
        } else {
            Write-Host "[WARNING] Migration had issues - check manually" -ForegroundColor Yellow
        }
    } else {
        Write-Host "[SKIP] Database migration skipped" -ForegroundColor Yellow
    }

    Pop-Location
}

# ===========================================
# 5. Summary
# ===========================================
Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host " Setup Complete!" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Verify environment variables are set correctly"
Write-Host "  2. Start the backend:  cd src/GrcMvc && dotnet run"
Write-Host "  3. Start the frontend: cd grc-frontend && npm run dev"
Write-Host ""
Write-Host "URLs:" -ForegroundColor Yellow
Write-Host "  - Backend:        http://localhost:3006"
Write-Host "  - Frontend:       http://localhost:3003"
Write-Host "  - Platform Admin: http://localhost:3006/platform-admin"
Write-Host ""
Write-Host "Platform Admin Login:" -ForegroundColor Yellow
Write-Host "  - Email:    Dooganlap@gmail.com"
Write-Host "  - Password: (set via GRC_PLATFORM_ADMIN_PASSWORD)"
Write-Host ""
Write-Host "Production URLs:" -ForegroundColor Yellow
Write-Host "  - Backend:  https://portal.shahin-ai.com"
Write-Host "  - Frontend: https://app.shahin-ai.com"
Write-Host ""

# Database Connection Setup Script for Shahin AI GRC Platform
# This script helps configure database connections for Windows environments

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  Shahin AI GRC - Database Connection Setup" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# Check if PostgreSQL is installed
Write-Host "Checking PostgreSQL installation..." -ForegroundColor Yellow
$pgService = Get-Service -Name "postgresql*" -ErrorAction SilentlyContinue

if ($null -eq $pgService) {
    Write-Host "❌ PostgreSQL is not installed or not running" -ForegroundColor Red
    Write-Host "Please install PostgreSQL from: https://www.postgresql.org/download/windows/" -ForegroundColor Yellow
    Write-Host ""
    $continue = Read-Host "Do you want to continue anyway? (y/n)"
    if ($continue -ne "y") {
        exit
    }
} else {
    Write-Host "✅ PostgreSQL service found: $($pgService.Name)" -ForegroundColor Green
    Write-Host "   Status: $($pgService.Status)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  Database Configuration" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# Get database configuration
$dbHost = Read-Host "Database Host (default: localhost)"
if ([string]::IsNullOrWhiteSpace($dbHost)) { $dbHost = "localhost" }

$dbPort = Read-Host "Database Port (default: 5432)"
if ([string]::IsNullOrWhiteSpace($dbPort)) { $dbPort = "5432" }

$dbUser = Read-Host "Database Username (default: postgres)"
if ([string]::IsNullOrWhiteSpace($dbUser)) { $dbUser = "postgres" }

$dbPassword = Read-Host "Database Password" -AsSecureString
$dbPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($dbPassword))

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  JWT Configuration" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# Generate JWT Secret
Write-Host "Generating secure JWT secret..." -ForegroundColor Yellow
$jwtSecret = -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 64 | ForEach-Object {[char]$_})
Write-Host "✅ JWT Secret generated (64 characters)" -ForegroundColor Green

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  SMTP Configuration (Optional)" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

$configureSmtp = Read-Host "Do you want to configure SMTP for email? (y/n)"
$smtpFromEmail = ""
$smtpUsername = ""
$smtpPassword = ""

if ($configureSmtp -eq "y") {
    $smtpFromEmail = Read-Host "SMTP From Email (e.g., info@shahin-ai.com)"
    $smtpUsername = Read-Host "SMTP Username (usually same as From Email)"
    $smtpPasswordSecure = Read-Host "SMTP Password" -AsSecureString
    $smtpPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($smtpPasswordSecure))
}

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  Creating Configuration File" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# Build connection strings
$defaultConnection = "Host=$dbHost;Port=$dbPort;Database=GrcMvcDb;Username=$dbUser;Password=$dbPasswordPlain;Include Error Detail=true"
$authConnection = "Host=$dbHost;Port=$dbPort;Database=GrcAuthDb;Username=$dbUser;Password=$dbPasswordPlain;Include Error Detail=true"
$hangfireConnection = "Host=$dbHost;Port=$dbPort;Database=HangfireDb;Username=$dbUser;Password=$dbPasswordPlain;Include Error Detail=true"
$redisConnection = "localhost:6379,abortConnect=false"

# Create appsettings.Local.json
$configPath = "src/GrcMvc/appsettings.Local.json"

$config = @{
    ConnectionStrings = @{
        DefaultConnection = $defaultConnection
        GrcAuthDb = $authConnection
        Redis = $redisConnection
        HangfireConnection = $hangfireConnection
    }
    JwtSettings = @{
        Secret = $jwtSecret
        Issuer = "ShahinAI"
        Audience = "ShahinAIUsers"
        ExpiryMinutes = 60
    }
}

if ($configureSmtp -eq "y") {
    $config.SmtpSettings = @{
        Host = "smtp.office365.com"
        Port = 587
        EnableSsl = $true
        FromEmail = $smtpFromEmail
        FromName = "Shahin AI"
        Username = $smtpUsername
        Password = $smtpPassword
    }
}

# Convert to JSON and save
$configJson = $config | ConvertTo-Json -Depth 10
Set-Content -Path $configPath -Value $configJson -Encoding UTF8

Write-Host "✅ Configuration file created: $configPath" -ForegroundColor Green

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  Setting Environment Variables" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# Set environment variables for current session
$env:ConnectionStrings__DefaultConnection = $defaultConnection
$env:ConnectionStrings__GrcAuthDb = $authConnection
$env:ConnectionStrings__Redis = $redisConnection
$env:ConnectionStrings__HangfireConnection = $hangfireConnection
$env:JWT_SECRET = $jwtSecret

if ($configureSmtp -eq "y") {
    $env:SMTP_FROM_EMAIL = $smtpFromEmail
    $env:SMTP_USERNAME = $smtpUsername
    $env:SMTP_PASSWORD = $smtpPassword
}

Write-Host "✅ Environment variables set for current session" -ForegroundColor Green

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  Database Setup" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

$createDatabases = Read-Host "Do you want to create the databases now? (y/n)"

if ($createDatabases -eq "y") {
    Write-Host "Creating databases..." -ForegroundColor Yellow
    
    # Create SQL script
    $sqlScript = @"
-- Create databases if they don't exist
SELECT 'CREATE DATABASE "GrcMvcDb"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'GrcMvcDb')\gexec

SELECT 'CREATE DATABASE "GrcAuthDb"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'GrcAuthDb')\gexec

SELECT 'CREATE DATABASE "HangfireDb"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'HangfireDb')\gexec
"@

    $sqlScript | Out-File -FilePath "create-databases.sql" -Encoding UTF8
    
    Write-Host "SQL script created: create-databases.sql" -ForegroundColor Gray
    Write-Host "Run this command to create databases:" -ForegroundColor Yellow
    Write-Host "  psql -h $dbHost -U $dbUser -f create-databases.sql" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  Next Steps" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "1. Create databases (if not done):" -ForegroundColor Yellow
Write-Host "   psql -h $dbHost -U $dbUser -f create-databases.sql" -ForegroundColor Gray
Write-Host ""

Write-Host "2. Run database migrations:" -ForegroundColor Yellow
Write-Host "   cd src/GrcMvc" -ForegroundColor Gray
Write-Host "   dotnet ef database update --context GrcDbContext" -ForegroundColor Gray
Write-Host "   dotnet ef database update --context GrcAuthDbContext" -ForegroundColor Gray
Write-Host ""

Write-Host "3. Start the application:" -ForegroundColor Yellow
Write-Host "   cd src/GrcMvc" -ForegroundColor Gray
Write-Host "   dotnet run" -ForegroundColor Gray
Write-Host ""

Write-Host "4. Test the application:" -ForegroundColor Yellow
Write-Host "   Navigate to: http://localhost:5000/account/register" -ForegroundColor Gray
Write-Host ""

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  Configuration Summary" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Database Host:     $dbHost" -ForegroundColor Gray
Write-Host "Database Port:     $dbPort" -ForegroundColor Gray
Write-Host "Database User:     $dbUser" -ForegroundColor Gray
Write-Host "Config File:       $configPath" -ForegroundColor Gray
Write-Host "JWT Secret:        Generated (64 chars)" -ForegroundColor Gray
if ($configureSmtp -eq "y") {
    Write-Host "SMTP Configured:   Yes" -ForegroundColor Gray
    Write-Host "SMTP From:         $smtpFromEmail" -ForegroundColor Gray
}
Write-Host ""

Write-Host "✅ Setup complete!" -ForegroundColor Green
Write-Host ""

# Offer to run migrations
$runMigrations = Read-Host "Do you want to run database migrations now? (y/n)"

if ($runMigrations -eq "y") {
    Write-Host ""
    Write-Host "Running migrations..." -ForegroundColor Yellow
    
    Set-Location -Path "src/GrcMvc"
    
    Write-Host "Running GrcDbContext migrations..." -ForegroundColor Yellow
    dotnet ef database update --context GrcDbContext
    
    Write-Host "Running GrcAuthDbContext migrations..." -ForegroundColor Yellow
    dotnet ef database update --context GrcAuthDbContext
    
    Write-Host ""
    Write-Host "✅ Migrations complete!" -ForegroundColor Green
    
    Set-Location -Path "../.."
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

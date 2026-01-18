# Add All Configuration Variables to Railway
# This script helps you add all required environment variables to Railway

param(
    [string]$ProjectId = "402d90cb-9706-4b98-ae24-0f2e992c624c",
    [string]$EnvironmentId = "03604398-8431-4c35-8fce-e230c4c8d585",
    [string]$ServiceId = "0cb7da15-a249-4cba-a197-677e800c306a"
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Railway Environment Variables Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "This script will help you add all required variables to Railway." -ForegroundColor Yellow
Write-Host ""
Write-Host "Project ID: $ProjectId" -ForegroundColor Gray
Write-Host "Environment ID: $EnvironmentId" -ForegroundColor Gray
Write-Host "Service ID: $ServiceId" -ForegroundColor Gray
Write-Host ""

# Check if railway CLI is available
if (-not (Get-Command railway -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Railway CLI not found. Please install it first:" -ForegroundColor Red
    Write-Host "   npm install -g @railway/cli" -ForegroundColor Gray
    exit 1
}

Write-Host "✅ Railway CLI found" -ForegroundColor Green
Write-Host ""

# List of required variables
$requiredVars = @{
    # Database (already set via template)
    "DATABASE_URL" = @{
        Value = "`${{ Postgres.DATABASE_URL }}"
        Required = $true
        Description = "Database connection URL (Railway template)"
        Note = "✅ Already configured via Railway template"
    }
    
    # Critical Application Settings
    "ASPNETCORE_ENVIRONMENT" = @{
        Value = "Production"
        Required = $true
        Description = "Application environment"
    }
    
    "ASPNETCORE_URLS" = @{
        Value = "http://0.0.0.0:5000"
        Required = $true
        Description = "Kestrel listening URLs"
    }
    
    # JWT Authentication (CRITICAL)
    "JWT_SECRET" = @{
        Value = ""
        Required = $true
        Description = "JWT token signing key (min 32 characters)"
        Generate = $true
        GenerateCommand = "openssl rand -base64 64"
    }
    
    "JwtSettings__Issuer" = @{
        Value = "https://portal.shahin-ai.com"
        Required = $false
        Description = "JWT token issuer"
    }
    
    "JwtSettings__Audience" = @{
        Value = "https://portal.shahin-ai.com"
        Required = $false
        Description = "JWT token audience"
    }
}

# Optional variables
$optionalVars = @{
    # Claude AI
    "CLAUDE_API_KEY" = @{
        Value = ""
        Required = $false
        Description = "Claude AI API key (if using AI features)"
    }
    
    "CLAUDE_ENABLED" = @{
        Value = "false"
        Required = $false
        Description = "Enable/disable Claude AI agents"
    }
    
    "CLAUDE_MODEL" = @{
        Value = "claude-sonnet-4-20250514"
        Required = $false
        Description = "Claude AI model name"
    }
    
    # SMTP Email
    "SMTP_FROM_EMAIL" = @{
        Value = ""
        Required = $false
        Description = "SMTP from email address"
    }
    
    "SMTP_CLIENT_ID" = @{
        Value = ""
        Required = $false
        Description = "SMTP OAuth2 client ID"
    }
    
    "SMTP_CLIENT_SECRET" = @{
        Value = ""
        Required = $false
        Description = "SMTP OAuth2 client secret"
    }
    
    # Azure/Microsoft Graph
    "AZURE_TENANT_ID" = @{
        Value = ""
        Required = $false
        Description = "Azure AD tenant ID"
    }
    
    "MSGRAPH_CLIENT_ID" = @{
        Value = ""
        Required = $false
        Description = "Microsoft Graph client ID"
    }
    
    "MSGRAPH_CLIENT_SECRET" = @{
        Value = ""
        Required = $false
        Description = "Microsoft Graph client secret"
    }
}

Write-Host "=== Required Variables ===" -ForegroundColor Yellow
Write-Host ""

foreach ($var in $requiredVars.GetEnumerator()) {
    $name = $var.Key
    $config = $var.Value
    
    Write-Host "Variable: $name" -ForegroundColor Cyan
    Write-Host "  Description: $($config.Description)" -ForegroundColor Gray
    
    if ($config.Note) {
        Write-Host "  $($config.Note)" -ForegroundColor Green
    } elseif ($config.Generate -and [string]::IsNullOrWhiteSpace($config.Value)) {
        Write-Host "  ⚠️  Needs to be generated" -ForegroundColor Yellow
        Write-Host "  Generate with: $($config.GenerateCommand)" -ForegroundColor Gray
    } else {
        Write-Host "  Value: $($config.Value)" -ForegroundColor White
    }
    Write-Host ""
}

Write-Host "=== Optional Variables ===" -ForegroundColor Yellow
Write-Host ""
Write-Host "These can be added later if needed." -ForegroundColor Gray
Write-Host ""

# Generate Railway CLI commands
Write-Host "=== Railway CLI Commands ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Run these commands to add variables to Railway:" -ForegroundColor Yellow
Write-Host ""

# DATABASE_URL (already set)
Write-Host "# Database (already configured)" -ForegroundColor Gray
Write-Host "railway variables set DATABASE_URL=`"`${{ Postgres.DATABASE_URL }}`" --project=$ProjectId --environment=$EnvironmentId --service=$ServiceId" -ForegroundColor White
Write-Host ""

# Required variables
Write-Host "# Required Application Settings" -ForegroundColor Gray
Write-Host "railway variables set ASPNETCORE_ENVIRONMENT=`"Production`" --project=$ProjectId --environment=$EnvironmentId --service=$ServiceId" -ForegroundColor White
Write-Host "railway variables set ASPNETCORE_URLS=`"http://0.0.0.0:5000`" --project=$ProjectId --environment=$EnvironmentId --service=$ServiceId" -ForegroundColor White
Write-Host ""

# JWT_SECRET (needs to be generated)
Write-Host "# JWT Secret (GENERATE FIRST - min 32 characters)" -ForegroundColor Yellow
Write-Host "# Generate with: openssl rand -base64 64" -ForegroundColor Gray
Write-Host "railway variables set JWT_SECRET=`"YOUR_GENERATED_SECRET_HERE`" --project=$ProjectId --environment=$EnvironmentId --service=$ServiceId" -ForegroundColor White
Write-Host ""

Write-Host "# JWT Settings (optional)" -ForegroundColor Gray
Write-Host "railway variables set JwtSettings__Issuer=`"https://portal.shahin-ai.com`" --project=$ProjectId --environment=$EnvironmentId --service=$ServiceId" -ForegroundColor White
Write-Host "railway variables set JwtSettings__Audience=`"https://portal.shahin-ai.com`" --project=$ProjectId --environment=$EnvironmentId --service=$ServiceId" -ForegroundColor White
Write-Host ""

Write-Host "=== To Add Variables via Railway Dashboard ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Go to Railway Dashboard" -ForegroundColor Yellow
Write-Host "2. Select your service" -ForegroundColor Yellow
Write-Host "3. Go to Variables tab" -ForegroundColor Yellow
Write-Host "4. Add each variable:" -ForegroundColor Yellow
Write-Host ""

foreach ($var in $requiredVars.GetEnumerator()) {
    $name = $var.Key
    $config = $var.Value
    
    if ($config.Note) {
        continue  # Skip DATABASE_URL as it's already configured
    }
    
    Write-Host "   $name" -ForegroundColor Cyan
    if ($config.Generate -and [string]::IsNullOrWhiteSpace($config.Value)) {
        Write-Host "      Value: [GENERATE - min 32 chars]" -ForegroundColor Yellow
    } else {
        Write-Host "      Value: $($config.Value)" -ForegroundColor White
    }
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

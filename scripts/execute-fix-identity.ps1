# Execute Fix Identity Tables SQL Script
# This will drop and recreate AspNetUsers with all ApplicationUser columns

param(
    [string]$ConnectionString = ""
)

# Get connection string from .env file
if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    $envFile = Join-Path $PSScriptRoot "..\.env"
    if (Test-Path $envFile) {
        $envContent = Get-Content $envFile -Raw
        if ($envContent -match 'ConnectionStrings__GrcAuthDb=(.+)') {
            $ConnectionString = $matches[1].Trim()
        }
    }
}

# Default if still empty
if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    $ConnectionString = "Host=localhost;Database=GrcAuthDb;Username=postgres;Password=postgres;Port=5432"
}

Write-Host "`n=== Fixing Identity Tables ===" -ForegroundColor Cyan
Write-Host "Connection: $($ConnectionString -replace 'Password=[^;]+', 'Password=***')" -ForegroundColor Gray

# Parse connection string
$connParams = @{}
$ConnectionString -split ';' | ForEach-Object {
    if ($_ -match '^([^=]+)=(.*)$') {
        $key = $matches[1].Trim()
        $value = $matches[2].Trim()
        $connParams[$key] = $value
    }
}

$dbHost = $connParams['Host'] ?? 'localhost'
$dbPort = $connParams['Port'] ?? '5432'
$dbName = $connParams['Database'] ?? 'GrcAuthDb'
$dbUser = $connParams['Username'] ?? 'postgres'
$dbPassword = $connParams['Password'] ?? 'postgres'

$sqlFile = Join-Path $PSScriptRoot "fix-identity-tables.sql"

if (-not (Test-Path $sqlFile)) {
    Write-Host "ERROR: SQL file not found: $sqlFile" -ForegroundColor Red
    exit 1
}

Write-Host "`nExecuting SQL script: $sqlFile" -ForegroundColor Cyan
Write-Host "Database: $dbHost`:$dbPort/$dbName" -ForegroundColor Gray

# Check if psql is available
try {
    $null = Get-Command psql -ErrorAction Stop
    $psqlAvailable = $true
} catch {
    $psqlAvailable = $false
    Write-Host "`nWARNING: psql not found. You can:" -ForegroundColor Yellow
    Write-Host "1. Install PostgreSQL client tools" -ForegroundColor White
    Write-Host "2. Run the SQL file manually in pgAdmin or another PostgreSQL client" -ForegroundColor White
    Write-Host "3. SQL file location: $sqlFile" -ForegroundColor Cyan
}

if ($psqlAvailable) {
    $env:PGPASSWORD = $dbPassword
    
    try {
        Write-Host "`nExecuting SQL script..." -ForegroundColor Cyan
        $result = Get-Content $sqlFile -Raw | & psql -h $dbHost -p $dbPort -U $dbUser -d $dbName 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "`n✅ SUCCESS: Identity tables fixed!" -ForegroundColor Green
            Write-Host $result
            
            # Verify columns
            Write-Host "`nVerifying AspNetUsers columns..." -ForegroundColor Cyan
            $verifyQuery = "SELECT column_name FROM information_schema.columns WHERE table_name = 'AspNetUsers' ORDER BY ordinal_position;"
            $verifyResult = $verifyQuery | & psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -c $verifyQuery 2>&1
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host $verifyResult
                
                # Check for required columns
                $requiredColumns = @("Abilities", "AssignedScope", "JobTitle", "FirstName", "LastName", "Department", "RoleProfileId", "KsaCompetencyLevel", "KnowledgeAreas", "Skills", "IsActive", "CreatedDate", "MustChangePassword", "LastPasswordChangedAt")
                $missingColumns = @()
                
                foreach ($col in $requiredColumns) {
                    if ($verifyResult -notmatch $col) {
                        $missingColumns += $col
                    }
                }
                
                if ($missingColumns.Count -eq 0) {
                    Write-Host "`n✅ All required ApplicationUser columns are present!" -ForegroundColor Green
                } else {
                    Write-Host "`n❌ Missing columns: $($missingColumns -join ', ')" -ForegroundColor Red
                }
            }
        } else {
            Write-Host "`n❌ ERROR: Failed to execute SQL script" -ForegroundColor Red
            Write-Host $result -ForegroundColor Red
            exit 1
        }
    } catch {
        Write-Host "`n❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    } finally {
        Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
    }
} else {
    Write-Host "`nPlease run the SQL script manually:" -ForegroundColor Yellow
    Write-Host "File: $sqlFile" -ForegroundColor Cyan
    Write-Host "Database: $dbName on $dbHost`:$dbPort" -ForegroundColor Cyan
}

Write-Host "`n=== Next Steps ===" -ForegroundColor Green
Write-Host "1. Restart your application" -ForegroundColor White
Write-Host "2. Test user registration/login forms" -ForegroundColor White
Write-Host "3. Verify all ApplicationUser properties are accessible" -ForegroundColor White

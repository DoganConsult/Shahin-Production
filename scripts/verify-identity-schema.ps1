# Verify Identity Schema - Check AspNetUsers has all ApplicationUser columns
# This script verifies that the migration was applied correctly

Write-Host "🔍 Verifying Identity Schema..." -ForegroundColor Cyan
Write-Host ""

# Get connection string from environment or appsettings
$envFile = Join-Path $PSScriptRoot "..\.env.local"
$connectionString = $null

if (Test-Path $envFile) {
    $envContent = Get-Content $envFile
    $authDbLine = $envContent | Select-String -Pattern "ConnectionStrings__GrcAuthDb|GrcAuthDb"
    if ($authDbLine) {
        $connectionString = ($authDbLine.Line -split '=')[1].Trim()
    }
}

if (-not $connectionString) {
    Write-Host "⚠️  Could not find GrcAuthDb connection string in .env.local" -ForegroundColor Yellow
    Write-Host "   Checking appsettings files..." -ForegroundColor Yellow
    
    $appsettingsPath = Join-Path $PSScriptRoot "..\src\GrcMvc\appsettings.Local.json"
    if (Test-Path $appsettingsPath) {
        $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
        $connectionString = $appsettings.ConnectionStrings.GrcAuthDb
    }
}

if (-not $connectionString) {
    Write-Host "❌ Could not find connection string. Please verify configuration." -ForegroundColor Red
    exit 1
}

Write-Host "✅ Found connection string" -ForegroundColor Green
Write-Host ""

# Extract database connection details
if ($connectionString -match 'Database=([^;]+)') { $dbName = $matches[1] } else { $dbName = "GrcAuthDb" }
if ($connectionString -match 'Host=([^;]+)') { $host = $matches[1] } else { $host = "localhost" }
if ($connectionString -match 'Port=([^;]+)') { $port = $matches[1] } else { $port = "5432" }
if ($connectionString -match 'Username=([^;]+)') { $user = $matches[1] } else { $user = "postgres" }
if ($connectionString -match 'Password=([^;]+)') { $password = $matches[1] } else { $password = "postgres" }

Write-Host "📊 Checking AspNetUsers table schema..." -ForegroundColor Cyan
Write-Host "   Database: $dbName" -ForegroundColor Gray
Write-Host "   Host: $host:$port" -ForegroundColor Gray
Write-Host ""

# Required ApplicationUser columns
$requiredColumns = @(
    "FirstName",
    "LastName", 
    "Department",
    "JobTitle",
    "RoleProfileId",
    "KsaCompetencyLevel",
    "KnowledgeAreas",
    "Skills",
    "Abilities",
    "AssignedScope",
    "IsActive",
    "CreatedDate",
    "LastLoginDate",
    "RefreshToken",
    "RefreshTokenExpiry",
    "MustChangePassword",
    "LastPasswordChangedAt"
)

# Check if psql is available
$psqlPath = Get-Command psql -ErrorAction SilentlyContinue
if (-not $psqlPath) {
    Write-Host "⚠️  psql not found. Using EF Core to verify..." -ForegroundColor Yellow
    Write-Host ""
    
    # Use dotnet ef to check
    Push-Location (Join-Path $PSScriptRoot "..\src\GrcMvc")
    
    Write-Host "📋 Checking migration status..." -ForegroundColor Cyan
    $migrationStatus = dotnet ef migrations list --context GrcAuthDbContext 2>&1
    if ($migrationStatus -match "AddApplicationUserCustomColumns") {
        Write-Host "✅ Migration found: AddApplicationUserCustomColumns" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Migration status unclear" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "💡 To verify columns, connect to database and run:" -ForegroundColor Cyan
    Write-Host "   SELECT column_name FROM information_schema.columns" -ForegroundColor Gray
    Write-Host "   WHERE table_name = 'AspNetUsers'" -ForegroundColor Gray
    Write-Host "   AND column_name IN ('FirstName', 'LastName', 'Abilities', 'AssignedScope', 'JobTitle');" -ForegroundColor Gray
    
    Pop-Location
    exit 0
}

# Use psql to query database
$env:PGPASSWORD = $password
$columnList = $requiredColumns -join "', '"
$query = "SELECT column_name, data_type, is_nullable FROM information_schema.columns WHERE table_name = 'AspNetUsers' AND column_name IN ('$columnList') ORDER BY column_name;"

Write-Host "🔍 Querying database..." -ForegroundColor Cyan
$result = & psql -h $host -p $port -U $user -d $dbName -t -A -c $query 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Database query failed!" -ForegroundColor Red
    Write-Host $result -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "📋 Found columns in AspNetUsers:" -ForegroundColor Cyan
Write-Host ""

$foundColumns = @()
if ($result) {
    $result | ForEach-Object {
        if ($_ -match "^\|(.+?)\|(.+?)\|(.+?)\|$") {
            $colName = $matches[1].Trim()
            $dataType = $matches[2].Trim()
            $isNullable = $matches[3].Trim()
            $foundColumns += $colName
            Write-Host "   ✅ $colName ($dataType, nullable: $isNullable)" -ForegroundColor Green
        }
    }
}

Write-Host ""
Write-Host "📊 Verification Summary:" -ForegroundColor Cyan
Write-Host ""

$missingColumns = $requiredColumns | Where-Object { $foundColumns -notcontains $_ }

if ($missingColumns.Count -eq 0) {
    Write-Host "✅ SUCCESS: All required ApplicationUser columns are present!" -ForegroundColor Green
    Write-Host "   Found $($foundColumns.Count) of $($requiredColumns.Count) required columns" -ForegroundColor Green
} else {
    Write-Host "❌ WARNING: Missing columns detected!" -ForegroundColor Red
    Write-Host "   Missing: $($missingColumns -join ', ')" -ForegroundColor Red
    Write-Host ""
    Write-Host "   Action required: Apply migration" -ForegroundColor Yellow
    Write-Host "   Run: dotnet ef database update --context GrcAuthDbContext" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "✅ Identity schema verification complete!" -ForegroundColor Green

# Production Deployment Verification Script
# Verifies database schema and application readiness

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Production Deployment Verification" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$errors = @()
$success = @()

# 1. Check Migration Status
Write-Host "1. Checking Migration Status..." -ForegroundColor Yellow
Push-Location "src\GrcMvc"
try {
    $migrationStatus = dotnet ef migrations list --context GrcAuthDbContext 2>&1
    if ($migrationStatus -match "20260115064458_AddApplicationUserCustomColumns") {
        if ($migrationStatus -notmatch "Pending") {
            $success += "Migration applied successfully"
            Write-Host "   [OK] Migration applied" -ForegroundColor Green
        } else {
            $errors += "Migration is still pending - run: dotnet ef database update --context GrcAuthDbContext"
            Write-Host "   [ERROR] Migration pending" -ForegroundColor Red
        }
    } else {
        $errors += "Migration not found"
        Write-Host "   [ERROR] Migration not found" -ForegroundColor Red
    }
} catch {
    $errors += "Could not check migration status: $_"
    Write-Host "   [ERROR] Could not verify migration" -ForegroundColor Red
}
Pop-Location

# 2. Check Release Build
Write-Host "2. Checking Release Build..." -ForegroundColor Yellow
$releaseDll = "src\GrcMvc\bin\Release\net8.0\GrcMvc.dll"
if (Test-Path $releaseDll) {
    $success += "Release build exists"
    Write-Host "   [OK] Release build found" -ForegroundColor Green
    $buildDate = (Get-Item $releaseDll).LastWriteTime
    Write-Host "   Build date: $buildDate" -ForegroundColor Gray
} else {
    $errors += "Release build not found. Run: dotnet build -c Release"
    Write-Host "   [ERROR] Release build missing" -ForegroundColor Red
}

# 3. Check Environment Variables
Write-Host "3. Checking Environment Variables..." -ForegroundColor Yellow
if ($env:JWT_SECRET) {
    $success += "JWT_SECRET is set"
    Write-Host "   [OK] JWT_SECRET configured" -ForegroundColor Green
} else {
    $errors += "JWT_SECRET not set. Required for production."
    Write-Host "   [ERROR] JWT_SECRET missing" -ForegroundColor Red
}

if ($env:ConnectionStrings__GrcAuthDb) {
    $success += "GrcAuthDb connection string is set"
    Write-Host "   [OK] GrcAuthDb connection string configured" -ForegroundColor Green
} else {
    $errors += "ConnectionStrings__GrcAuthDb not set"
    Write-Host "   [WARNING] GrcAuthDb connection string not set (may use appsettings)" -ForegroundColor Yellow
}

if ($env:ASPNETCORE_ENVIRONMENT -eq "Production") {
    $success += "Environment set to Production"
    Write-Host "   [OK] ASPNETCORE_ENVIRONMENT = Production" -ForegroundColor Green
} else {
    Write-Host "   [INFO] ASPNETCORE_ENVIRONMENT = $($env:ASPNETCORE_ENVIRONMENT)" -ForegroundColor Gray
}

# 4. Check Program.cs Configuration
Write-Host "4. Checking Program.cs Configuration..." -ForegroundColor Yellow
$programPath = "src\GrcMvc\Program.cs"
if (Test-Path $programPath) {
    $programContent = Get-Content $programPath -Raw
    if ($programContent -match "authContext\.Database\.Migrate\(\)") {
        $success += "Program.cs uses Migrate()"
        Write-Host "   [OK] Program.cs uses Migrate() (correct)" -ForegroundColor Green
    } elseif ($programContent -match "authContext\.Database\.EnsureCreated\(\)") {
        $errors += "Program.cs uses EnsureCreated() instead of Migrate()"
        Write-Host "   [ERROR] Program.cs uses EnsureCreated() (WRONG!)" -ForegroundColor Red
    } else {
        Write-Host "   [WARNING] Could not verify migration method" -ForegroundColor Yellow
    }
} else {
    $errors += "Program.cs not found"
    Write-Host "   [ERROR] Program.cs missing" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Verification Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($errors.Count -eq 0) {
    Write-Host "[SUCCESS] All checks passed!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Cyan
    Write-Host "1. Start application:" -ForegroundColor White
    Write-Host "   cd src\GrcMvc\bin\Release\net8.0" -ForegroundColor Gray
    Write-Host "   dotnet GrcMvc.dll" -ForegroundColor Gray
    Write-Host ""
    Write-Host "2. Monitor logs for:" -ForegroundColor White
    Write-Host "   'Auth database migrations applied'" -ForegroundColor Gray
    Write-Host ""
    Write-Host "3. Test user forms:" -ForegroundColor White
    Write-Host "   - Create new user with all fields" -ForegroundColor Gray
    Write-Host "   - Edit existing user" -ForegroundColor Gray
    Write-Host "   - Verify all ApplicationUser properties work" -ForegroundColor Gray
    Write-Host ""
    Write-Host "4. Verify database schema:" -ForegroundColor White
    Write-Host "   Run: scripts\verify-database-schema.sql" -ForegroundColor Gray
    exit 0
} else {
    Write-Host "[ERROR] Issues found that must be fixed:" -ForegroundColor Red
    foreach ($error in $errors) {
        Write-Host "   - $error" -ForegroundColor Red
    }
    Write-Host ""
    if ($success.Count -gt 0) {
        Write-Host "Completed checks:" -ForegroundColor Green
        foreach ($item in $success) {
            Write-Host "   + $item" -ForegroundColor Green
        }
    }
    exit 1
}

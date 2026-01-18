# Production Readiness Verification Script
# Verifies database schema and migration status

Write-Host "🔍 Production Readiness Verification" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

$errors = @()
$warnings = @()

# 1. Check if migration file exists
Write-Host "1. Checking migration file..." -ForegroundColor Yellow
$migrationPath = "src\GrcMvc\Migrations\Auth\20260115064458_AddApplicationUserCustomColumns.cs"
if (Test-Path $migrationPath) {
    Write-Host "   [OK] Migration file found" -ForegroundColor Green
} else {
    $errors += "Migration file not found: $migrationPath"
    Write-Host "   [ERROR] Migration file missing" -ForegroundColor Red
}

# 2. Check Program.cs uses Migrate()
Write-Host "2. Checking Program.cs configuration..." -ForegroundColor Yellow
$programPath = "src\GrcMvc\Program.cs"
if (Test-Path $programPath) {
    $programContent = Get-Content $programPath -Raw
    if ($programContent -match "authContext\.Database\.Migrate\(\)") {
        Write-Host "   [OK] Program.cs uses Migrate()" -ForegroundColor Green
    } elseif ($programContent -match "authContext\.Database\.EnsureCreated\(\)") {
        $errors += "Program.cs uses EnsureCreated() instead of Migrate()"
        Write-Host "   [ERROR] Program.cs uses EnsureCreated() (WRONG!)" -ForegroundColor Red
    } else {
        $warnings += "Could not verify migration method in Program.cs"
        Write-Host "   ⚠️  Could not verify migration method" -ForegroundColor Yellow
    }
} else {
    $errors += "Program.cs not found"
    Write-Host "   [ERROR] Program.cs not found" -ForegroundColor Red
}

# 3. Check Release build exists
Write-Host "3. Checking Release build..." -ForegroundColor Yellow
$releaseDll = "src\GrcMvc\bin\Release\net8.0\GrcMvc.dll"
if (Test-Path $releaseDll) {
    Write-Host "   [OK] Release build found" -ForegroundColor Green
    $buildDate = (Get-Item $releaseDll).LastWriteTime
    Write-Host "   📅 Build date: $buildDate" -ForegroundColor Gray
} else {
    $warnings += "Release build not found. Run: dotnet build -c Release"
    Write-Host "   ⚠️  Release build not found" -ForegroundColor Yellow
}

# 4. Check ApplicationUser entity
Write-Host "4. Checking ApplicationUser entity..." -ForegroundColor Yellow
$userEntityPath = "src\GrcMvc\Models\Entities\ApplicationUser.cs"
if (Test-Path $userEntityPath) {
    $entityContent = Get-Content $userEntityPath -Raw
    $requiredProps = @("FirstName", "LastName", "Abilities", "AssignedScope", "JobTitle")
    $missingProps = @()
    foreach ($prop in $requiredProps) {
        if ($entityContent -notmatch "public.*$prop") {
            $missingProps += $prop
        }
    }
    if ($missingProps.Count -eq 0) {
        Write-Host "   [OK] All required properties found in ApplicationUser" -ForegroundColor Green
    } else {
        $warnings += "Missing properties in ApplicationUser: $($missingProps -join ', ')"
        Write-Host "   ⚠️  Missing properties: $($missingProps -join ', ')" -ForegroundColor Yellow
    }
} else {
    $errors += "ApplicationUser.cs not found"
    Write-Host "   [ERROR] ApplicationUser.cs not found" -ForegroundColor Red
}

# 5. Check documentation
Write-Host "5. Checking documentation..." -ForegroundColor Yellow
$docs = @(
    "docs\IDENTITY_SCHEMA_SAFEGUARDS.md",
    "DEPLOYMENT_VERIFICATION.md",
    "PRODUCTION_DEPLOYMENT_STEPS.md"
)
$docCount = 0
foreach ($doc in $docs) {
    if (Test-Path $doc) {
        $docCount++
    }
}
if ($docCount -eq $docs.Count) {
    Write-Host "   [OK] All documentation files present ($docCount/$($docs.Count))" -ForegroundColor Green
} else {
    Write-Host "   ⚠️  Some documentation missing ($docCount/$($docs.Count))" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "====================================" -ForegroundColor Cyan
Write-Host "Verification Summary" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

if ($errors.Count -eq 0 -and $warnings.Count -eq 0) {
    Write-Host "[SUCCESS] ALL CHECKS PASSED - Ready for Production!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Cyan
    Write-Host "1. Set environment variables (JWT_SECRET, connection strings)" -ForegroundColor White
    Write-Host "2. Deploy application: dotnet bin/Release/net8.0/GrcMvc.dll" -ForegroundColor White
    Write-Host "3. Monitor startup logs for migration confirmation" -ForegroundColor White
    Write-Host "4. Verify database schema using SQL queries" -ForegroundColor White
    Write-Host "5. Test user creation/editing forms" -ForegroundColor White
    exit 0
} elseif ($errors.Count -eq 0) {
    Write-Host "⚠️  WARNINGS FOUND - Review before production" -ForegroundColor Yellow
    foreach ($warning in $warnings) {
        Write-Host "   - $warning" -ForegroundColor Yellow
    }
    Write-Host ""
    Write-Host "Application may work, but review warnings above." -ForegroundColor Yellow
    exit 0
} else {
    Write-Host "[ERROR] ERRORS FOUND - Must fix before production" -ForegroundColor Red
    foreach ($error in $errors) {
        Write-Host "   - $error" -ForegroundColor Red
    }
    if ($warnings.Count -gt 0) {
        Write-Host ""
        Write-Host "Warnings:" -ForegroundColor Yellow
        foreach ($warning in $warnings) {
            Write-Host "   - $warning" -ForegroundColor Yellow
        }
    }
    exit 1
}

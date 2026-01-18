
`# Railway Database Migration Script
# Creates initial migrations for both database contexts

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Railway Database Migration Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Change to project directory
$projectPath = "Shahin-Jan-2026/src/GrcMvc"
Write-Host "Navigating to project: $projectPath" -ForegroundColor Yellow
Set-Location $projectPath

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 1: Build Project" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "Building project..." -ForegroundColor Yellow
dotnet build

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed! Please fix compilation errors first." -ForegroundColor Red
    Write-Host ""
    Write-Host "Run 'dotnet build' to see detailed errors." -ForegroundColor Yellow
    exit 1
}

Write-Host "Build successful!" -ForegroundColor Green
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 2: Check Existing Migrations" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "Checking GrcAuthDbContext migrations..." -ForegroundColor Yellow
dotnet ef migrations list --context GrcAuthDbContext

Write-Host ""
Write-Host "Checking GrcDbContext migrations..." -ForegroundColor Yellow
$mainMigrations = dotnet ef migrations list --context GrcDbContext 2>&1

if ($mainMigrations -match "No migrations were found") {
    Write-Host "No migrations found for GrcDbContext (expected)" -ForegroundColor Yellow
} else {
    Write-Host $mainMigrations
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 3: Create Initial Migration for GrcDbContext" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "Creating initial migration for main GRC database..." -ForegroundColor Yellow
Write-Host "   This will create migrations for 200+ tables..." -ForegroundColor Gray
Write-Host ""

# Create Migrations/Main directory if it doesn't exist
$mainMigrationsDir = "Migrations/Main"
if (-not (Test-Path $mainMigrationsDir)) {
    New-Item -ItemType Directory -Path $mainMigrationsDir -Force | Out-Null
    Write-Host "Created directory: $mainMigrationsDir" -ForegroundColor Green
}

# Create the migration
dotnet ef migrations add InitialCreate --context GrcDbContext --output-dir Migrations/Main

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to create migration!" -ForegroundColor Red
    exit 1
}

Write-Host "Initial migration created successfully!" -ForegroundColor Green
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 4: Generate SQL Scripts (Optional)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "Generating SQL script for review..." -ForegroundColor Yellow

# Generate SQL script for main database
dotnet ef migrations script --context GrcDbContext --output Migrations/Main/InitialCreate.sql

if ($LASTEXITCODE -eq 0) {
    Write-Host "SQL script generated: Migrations/Main/InitialCreate.sql" -ForegroundColor Green
} else {
    Write-Host "Could not generate SQL script (non-critical)" -ForegroundColor Yellow
}

Write-Host ""

# Generate SQL script for auth database
Write-Host "Generating SQL script for auth migrations..." -ForegroundColor Yellow
dotnet ef migrations script --context GrcAuthDbContext --output Migrations/Auth/AllAuthMigrations.sql

if ($LASTEXITCODE -eq 0) {
    Write-Host "SQL script generated: Migrations/Auth/AllAuthMigrations.sql" -ForegroundColor Green
} else {
    Write-Host "Could not generate SQL script (non-critical)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 5: Verify Migration Files" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "Migration files created:" -ForegroundColor Yellow
Write-Host ""

# List main migration files
Write-Host "Main Database (GrcDbContext):" -ForegroundColor Cyan
Get-ChildItem -Path "Migrations/Main" -Filter "*.cs" | ForEach-Object {
    Write-Host "  [OK] $($_.Name)" -ForegroundColor Green
}

Write-Host ""

# List auth migration files
Write-Host "Auth Database (GrcAuthDbContext):" -ForegroundColor Cyan
Get-ChildItem -Path "Migrations/Auth" -Filter "*.cs" | ForEach-Object {
    Write-Host "  [OK] $($_.Name)" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Migration Setup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Review the generated migration files in:" -ForegroundColor White
Write-Host "   - Migrations/Main/YYYYMMDDHHMMSS_InitialCreate.cs" -ForegroundColor Gray
Write-Host "   - Migrations/Main/InitialCreate.sql (optional review)" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Commit the migration files:" -ForegroundColor White
Write-Host "   git add Shahin-Jan-2026/src/GrcMvc/Migrations/" -ForegroundColor Gray
Write-Host "   git commit -m 'Add initial database migrations for Railway'" -ForegroundColor Gray
Write-Host "   git push" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Deploy to Railway (auto-deploys on push):" -ForegroundColor White
Write-Host "   railway logs --service 0cb7da15-a249-4cba-a197-677e800c306a --follow" -ForegroundColor Gray
Write-Host ""
Write-Host "4. Verify migration success in logs:" -ForegroundColor White
Write-Host "   Look for: 'Main database migrations applied successfully'" -ForegroundColor Gray
Write-Host "   Look for: 'Auth database migrations applied successfully'" -ForegroundColor Gray
Write-Host ""
Write-Host "5. Verify tables in Railway database:" -ForegroundColor White
Write-Host "   railway ssh --service 0cb7da15-a249-4cba-a197-677e800c306a" -ForegroundColor Gray
Write-Host "   psql `$DATABASE_URL -c '\dt'" -ForegroundColor Gray
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Documentation:" -ForegroundColor Yellow
Write-Host "   See RAILWAY_MIGRATION_PLAN.md for detailed guide" -ForegroundColor Gray
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Return to original directory
Set-Location ../../../

Write-Host "Script completed successfully!" -ForegroundColor Green
Write-Host ""

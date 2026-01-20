# Test Golden Path Login Flow
# Tests login flow and checks for [GOLDEN_PATH] log entries

Write-Host "🧪 Testing Golden Path Login Flow" -ForegroundColor Cyan
Write-Host ""

# Configuration
$BackendUrl = "http://localhost:5010"
$FrontendUrl = "http://localhost:3003"  # Update if different
$LogFile = "logs/grcmvc-*.log"

# Test credentials (update with actual test user)
$TestEmail = "admin@test.com"
$TestPassword = "Test123!"

Write-Host "📋 Configuration:" -ForegroundColor Yellow
Write-Host "   Backend:  $BackendUrl" -ForegroundColor Gray
Write-Host "   Frontend: $FrontendUrl" -ForegroundColor Gray
Write-Host ""

# Check if backend is running
Write-Host "🔍 Checking backend status..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$BackendUrl/health" -TimeoutSec 2 -ErrorAction Stop
    Write-Host "✅ Backend is running" -ForegroundColor Green
} catch {
    Write-Host "❌ Backend not running on $BackendUrl" -ForegroundColor Red
    Write-Host "   Start with: cd src/GrcMvc; dotnet run --urls '$BackendUrl'" -ForegroundColor Yellow
    exit 1
}

# Check if frontend is running
Write-Host "🔍 Checking frontend status..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$FrontendUrl" -TimeoutSec 2 -ErrorAction Stop
    Write-Host "✅ Frontend is running" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Frontend not running on $FrontendUrl" -ForegroundColor Yellow
    Write-Host "   Start with: cd grc-frontend; npm run dev" -ForegroundColor Yellow
    Write-Host "   Continuing with backend-only test..." -ForegroundColor Gray
}

Write-Host ""
Write-Host "📝 Testing Login Flow:" -ForegroundColor Cyan
Write-Host "   1. Navigate to login page" -ForegroundColor Gray
Write-Host "   2. Submit login form" -ForegroundColor Gray
Write-Host "   3. Check for redirects" -ForegroundColor Gray
Write-Host "   4. Verify [GOLDEN_PATH] logs" -ForegroundColor Gray
Write-Host ""

# Check for log files
$logPath = "src/GrcMvc/logs"
if (Test-Path $logPath) {
    $logFiles = Get-ChildItem -Path $logPath -Filter "*.log" -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1

    if ($logFiles) {
        Write-Host "📄 Latest log file: $($logFiles.FullName)" -ForegroundColor Cyan
        Write-Host ""
        
        # Search for GOLDEN_PATH entries
        Write-Host "🔍 Searching for [GOLDEN_PATH] log entries..." -ForegroundColor Yellow
        $goldenPathLogs = Select-String -Path $logFiles.FullName -Pattern "\[GOLDEN_PATH\]" | Select-Object -Last 20
        
        if ($goldenPathLogs) {
            Write-Host "✅ Found $($goldenPathLogs.Count) [GOLDEN_PATH] log entries:" -ForegroundColor Green
            Write-Host ""
            foreach ($log in $goldenPathLogs) {
                $line = $log.Line.Trim()
                if ($line -match "✅") {
                    Write-Host "   ✅ $line" -ForegroundColor Green
                } elseif ($line -match "ERROR") {
                    Write-Host "   ❌ $line" -ForegroundColor Red
                } else {
                    Write-Host "   📝 $line" -ForegroundColor Gray
                }
            }
        } else {
            Write-Host "⚠️  No [GOLDEN_PATH] log entries found" -ForegroundColor Yellow
            Write-Host "   This could mean:" -ForegroundColor Gray
            Write-Host "   - Login flow hasn't been triggered yet" -ForegroundColor Gray
            Write-Host "   - Logs are in a different location" -ForegroundColor Gray
            Write-Host "   - Logging level is too high" -ForegroundColor Gray
        }
    } else {
        Write-Host "⚠️  No log files found in $logPath" -ForegroundColor Yellow
    }
} else {
    Write-Host "⚠️  Log directory not found: $logPath" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🌐 Open browser to test login flow:" -ForegroundColor Cyan
Write-Host "   Backend Login:  $BackendUrl/Account/Login" -ForegroundColor Gray
Write-Host "   Frontend Login: $FrontendUrl/login" -ForegroundColor Gray
Write-Host ""
Write-Host "💡 After testing login, run this script again to check logs" -ForegroundColor Yellow

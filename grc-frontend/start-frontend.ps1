# Start GRC Frontend on port 3003
# Connects to backend on port 3006

Write-Host "🚀 Starting GRC Frontend..." -ForegroundColor Green
Write-Host "   Frontend: http://localhost:3003" -ForegroundColor Cyan
Write-Host "   Backend:  http://localhost:3006" -ForegroundColor Cyan
Write-Host ""

# Check if backend is running
try {
    Invoke-WebRequest -Uri "http://localhost:3006/health" -TimeoutSec 2 -ErrorAction Stop | Out-Null
    Write-Host "✅ Backend is running on port 3006" -ForegroundColor Green
} catch {
    Write-Host "❌ Backend not found on port 3006" -ForegroundColor Red
    Write-Host "   Start backend first: dotnet run --urls 'http://localhost:3006'" -ForegroundColor Yellow
    exit 1
}

# Start frontend
npm run dev

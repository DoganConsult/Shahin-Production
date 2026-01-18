# ══════════════════════════════════════════════════════════════════════════════
# SHAHIN AI GRC - PRODUCTION DEPLOYMENT SCRIPT (PowerShell)
# ══════════════════════════════════════════════════════════════════════════════
# Usage: .\scripts\deploy-production.ps1 [command]
# Commands: setup | build | deploy | status | logs | stop | restart | rebuild-full
# ══════════════════════════════════════════════════════════════════════════════

param(
    [Parameter(Position=0)]
    [ValidateSet("setup", "build", "deploy", "status", "logs", "stop", "restart", "rebuild-full", "help")]
    [string]$Command = "help"
)

$ErrorActionPreference = "Stop"

# Get project root (script is in scripts/ folder, so go up one level)
if ($PSScriptRoot) {
    $PROJECT_ROOT = Split-Path -Parent $PSScriptRoot
} else {
    # Fallback: assume we're in project root
    $PROJECT_ROOT = $PWD
}

$COMPOSE_FILE = Join-Path $PROJECT_ROOT "docker-compose.production.yml"
$ENV_FILE = Join-Path $PROJECT_ROOT ".env.production"

Write-Host "Project root: $PROJECT_ROOT" -ForegroundColor Cyan
Set-Location $PROJECT_ROOT

function Write-Info { Write-Host "[INFO] $args" -ForegroundColor Green }
function Write-Warn { Write-Host "[WARN] $args" -ForegroundColor Yellow }
function Write-Error { Write-Host "[ERROR] $args" -ForegroundColor Red }

# ════════════════════════════════════════════════════════════════════════════
# SETUP: Create required directories and files
# ════════════════════════════════════════════════════════════════════════════
function Setup-Environment {
    Write-Info "Setting up production environment..."
    
    # Create host directories for volumes
    $dirs = @(
        "C:\var\lib\shahin-ai\postgres",
        "C:\var\lib\shahin-ai\redis",
        "C:\var\www\shahin-ai\storage",
        "C:\var\log\shahin-ai"
    )
    
    foreach ($dir in $dirs) {
        if (-not (Test-Path $dir)) {
            New-Item -ItemType Directory -Path $dir -Force | Out-Null
            Write-Info "Created directory: $dir"
        }
    }
    
    # Create SSL directories
    $sslDirs = @(
        "nginx\ssl",
        "src\GrcMvc\certificates"
    )
    
    foreach ($dir in $sslDirs) {
        $fullPath = Join-Path $PROJECT_ROOT $dir
        if (-not (Test-Path $fullPath)) {
            New-Item -ItemType Directory -Path $fullPath -Force | Out-Null
        }
    }
    
    # Check for env file
    if (-not (Test-Path $ENV_FILE)) {
        $templateFile = Join-Path $PROJECT_ROOT ".env.production.template"
        if (Test-Path $templateFile) {
            Copy-Item $templateFile $ENV_FILE
            Write-Warn "Created $ENV_FILE from template - EDIT THIS FILE!"
        } else {
            Write-Error ".env.production not found! Create it from template first."
            exit 1
        }
    }
    
    Write-Info "Setup complete!"
}

# ════════════════════════════════════════════════════════════════════════════
# BUILD: Build production binaries and Docker images
# ════════════════════════════════════════════════════════════════════════════
function Build-Production {
    Write-Info "Building production binaries..."
    
    $grcMvcPath = Join-Path $PROJECT_ROOT "src\GrcMvc\GrcMvc.csproj"
    
    if (-not (Test-Path $grcMvcPath)) {
        Write-Error "Project file not found: $grcMvcPath"
        exit 1
    }
    
    # Clean previous builds
    Write-Info "Cleaning previous builds..."
    dotnet clean $grcMvcPath -c Release --verbosity quiet
    
    # Restore dependencies
    Write-Info "Restoring NuGet packages..."
    dotnet restore $grcMvcPath --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Package restore failed!"
        exit 1
    }
    
    # Build Release configuration
    Write-Info "Building Release configuration..."
    dotnet build $grcMvcPath -c Release --no-incremental
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed! Check errors above."
        exit 1
    }
    
    Write-Info "✅ Build succeeded!"
    
    # Build Docker images if Docker is available
    if (Get-Command docker -ErrorAction SilentlyContinue) {
        Write-Info "Building Docker images..."
        docker-compose -f $COMPOSE_FILE build --no-cache
        if ($LASTEXITCODE -ne 0) {
            Write-Warn "Docker build failed, but .NET build succeeded"
        } else {
            Write-Info "✅ Docker images built successfully!"
        }
    } else {
        Write-Warn "Docker not found - skipping Docker image build"
    }
    
    Write-Info "Build complete!"
}

# ════════════════════════════════════════════════════════════════════════════
# REBUILD-FULL: Complete rebuild with all checks
# ════════════════════════════════════════════════════════════════════════════
function Rebuild-Full {
    Write-Info "═══════════════════════════════════════════════════════════════"
    Write-Info "FULL PRODUCTION REBUILD"
    Write-Info "═══════════════════════════════════════════════════════════════"
    
    # Step 1: Verify prerequisites
    Write-Info "Step 1: Verifying prerequisites..."
    
    $grcMvcPath = Join-Path $PROJECT_ROOT "src\GrcMvc\GrcMvc.csproj"
    if (-not (Test-Path $grcMvcPath)) {
        Write-Error "Project file not found: $grcMvcPath"
        exit 1
    }
    
    # Check .NET SDK
    $dotnetVersion = dotnet --version
    Write-Info "Using .NET SDK: $dotnetVersion"
    
    # Step 2: Clean everything
    Write-Info "Step 2: Cleaning build artifacts..."
    dotnet clean $grcMvcPath -c Release --verbosity quiet
    Remove-Item -Path "src\GrcMvc\bin" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "src\GrcMvc\obj" -Recurse -Force -ErrorAction SilentlyContinue
    
    # Step 3: Restore dependencies
    Write-Info "Step 3: Restoring dependencies..."
    dotnet restore $grcMvcPath --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Package restore failed!"
        exit 1
    }
    
    # Step 4: Build Release
    Write-Info "Step 4: Building Release configuration..."
    dotnet build $grcMvcPath -c Release --no-incremental 2>&1 | Tee-Object -Variable buildOutput
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed!"
        $buildOutput | Select-String -Pattern "error CS" | Select-Object -First 10
        exit 1
    }
    
    # Check for warnings
    $warnings = $buildOutput | Select-String -Pattern "warning"
    if ($warnings) {
        $warningCount = ($warnings | Measure-Object).Count
        Write-Warn "Build completed with $warningCount warnings"
    } else {
        Write-Info "✅ Build succeeded with 0 warnings!"
    }
    
    # Step 5: Publish for production
    Write-Info "Step 5: Publishing for production..."
    $publishPath = Join-Path $PROJECT_ROOT "src\GrcMvc\bin\Release\net8.0\publish"
    dotnet publish $grcMvcPath -c Release -o $publishPath --no-build
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Publish failed!"
        exit 1
    }
    
    Write-Info "✅ Published to: $publishPath"
    
    # Step 6: Verify binaries
    Write-Info "Step 6: Verifying production binaries..."
    $dllPath = Join-Path $publishPath "GrcMvc.dll"
    if (Test-Path $dllPath) {
        $fileInfo = Get-Item $dllPath
        Write-Info "✅ Production DLL: $($fileInfo.FullName) ($([math]::Round($fileInfo.Length / 1MB, 2)) MB)"
    } else {
        Write-Error "Production DLL not found!"
        exit 1
    }
    
    # Step 7: Build Docker images (if Docker available)
    if (Get-Command docker -ErrorAction SilentlyContinue) {
        Write-Info "Step 7: Building Docker images..."
        docker-compose -f $COMPOSE_FILE build --no-cache
        if ($LASTEXITCODE -eq 0) {
            Write-Info "✅ Docker images built successfully!"
        } else {
            Write-Warn "Docker build had issues, but .NET build succeeded"
        }
    } else {
        Write-Warn "Docker not found - skipping Docker image build"
    }
    
    Write-Info "═══════════════════════════════════════════════════════════════"
    Write-Info "✅ FULL REBUILD COMPLETE"
    Write-Info "═══════════════════════════════════════════════════════════════"
    Write-Info "Production binaries ready at: $publishPath"
    Write-Info "Next step: Run 'deploy' command to start containers"
}

# ════════════════════════════════════════════════════════════════════════════
# DEPLOY: Start production containers
# ════════════════════════════════════════════════════════════════════════════
function Deploy-Production {
    Write-Info "Deploying to production..."
    
    # Verify prerequisites
    if (-not (Test-Path $ENV_FILE)) {
        Write-Error "Environment file not found: $ENV_FILE"
        Write-Info "Run: .\scripts\deploy-production.ps1 setup"
        exit 1
    }
    
    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
        Write-Error "Docker not found! Install Docker Desktop first."
        exit 1
    }
    
    # Start containers
    Write-Info "Starting production containers..."
    docker-compose -f $COMPOSE_FILE --env-file $ENV_FILE up -d
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Deployment failed!"
        exit 1
    }
    
    # Wait for health checks
    Write-Info "Waiting for services to be healthy..."
    Start-Sleep -Seconds 10
    
    # Show status
    Show-Status
    
    Write-Info "Deployment complete!"
    Write-Info "Access application at: http://localhost:5000"
}

# ════════════════════════════════════════════════════════════════════════════
# STATUS: Show container status
# ════════════════════════════════════════════════════════════════════════════
function Show-Status {
    Write-Info "Container status:"
    docker-compose -f $COMPOSE_FILE ps
    
    Write-Info ""
    Write-Info "Health check:"
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
        Write-Info "✅ Health endpoint responding: $($response.StatusCode)"
    } catch {
        Write-Warn "Health endpoint not responding yet"
    }
}

# ════════════════════════════════════════════════════════════════════════════
# LOGS: View container logs
# ════════════════════════════════════════════════════════════════════════════
function Show-Logs {
    param([string]$Service = "")
    
    if ($Service) {
        docker-compose -f $COMPOSE_FILE logs -f --tail=100 $Service
    } else {
        docker-compose -f $COMPOSE_FILE logs -f --tail=100
    }
}

# ════════════════════════════════════════════════════════════════════════════
# STOP: Stop all containers
# ════════════════════════════════════════════════════════════════════════════
function Stop-Production {
    Write-Info "Stopping production containers..."
    docker-compose -f $COMPOSE_FILE down
    Write-Info "Containers stopped."
}

# ════════════════════════════════════════════════════════════════════════════
# RESTART: Restart containers
# ════════════════════════════════════════════════════════════════════════════
function Restart-Production {
    Write-Info "Restarting production containers..."
    docker-compose -f $COMPOSE_FILE restart
    Show-Status
}

# ════════════════════════════════════════════════════════════════════════════
# MAIN
# ════════════════════════════════════════════════════════════════════════════
switch ($Command) {
    "setup" { Setup-Environment }
    "build" { Build-Production }
    "rebuild-full" { Rebuild-Full }
    "deploy" { Deploy-Production }
    "status" { Show-Status }
    "logs" { Show-Logs }
    "stop" { Stop-Production }
    "restart" { Restart-Production }
    "help" {
        Write-Host "Usage: .\scripts\deploy-production.ps1 [command]"
        Write-Host ""
        Write-Host "Commands:"
        Write-Host "  setup       - Create directories and prepare environment"
        Write-Host "  build       - Build production binaries"
        Write-Host "  rebuild-full - Complete rebuild with all checks"
        Write-Host "  deploy      - Start production containers"
        Write-Host "  status      - Show container status"
        Write-Host "  logs        - View container logs"
        Write-Host "  stop        - Stop all containers"
        Write-Host "  restart     - Restart all containers"
        Write-Host ""
        Write-Host "Example: .\scripts\deploy-production.ps1 rebuild-full"
    }
    default {
        Write-Error "Unknown command: $Command"
        Write-Host "Run '.\scripts\deploy-production.ps1 help' for usage"
        exit 1
    }
}

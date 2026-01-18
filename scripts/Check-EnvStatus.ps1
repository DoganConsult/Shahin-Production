# Shahin AI - Environment Status Checker (PowerShell)
# Checks status of all environments: Development, Staging, Production
#
# Usage: .\Check-EnvStatus.ps1 [environment]
#   - No argument: Check all environments
#   - dev: Check development only
#   - staging: Check staging only
#   - production: Check production only

param(
    [Parameter(Position=0)]
    [ValidateSet("all", "dev", "development", "staging", "production", "prod")]
    [string]$Environment = "all"
)

# Colors
function Write-Header { Write-Host "════════════════════════════════════════════════════════" -ForegroundColor Blue; Write-Host $args[0] -ForegroundColor Blue; Write-Host "════════════════════════════════════════════════════════" -ForegroundColor Blue; Write-Host "" }
function Write-Success { Write-Host "✓ " -NoNewline -ForegroundColor Green; Write-Host $args[0] }
function Write-Error { Write-Host "✗ " -NoNewline -ForegroundColor Red; Write-Host $args[0] }
function Write-Warning { Write-Host "⚠ " -NoNewline -ForegroundColor Yellow; Write-Host $args[0] }
function Write-Info { Write-Host "ℹ " -NoNewline -ForegroundColor Cyan; Write-Host $args[0] }

# Check Docker
function Test-Docker {
    try {
        $dockerVersion = docker --version 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Docker installed: $dockerVersion"
            
            $dockerPs = docker ps 2>&1
            if ($LASTEXITCODE -eq 0) {
                Write-Success "Docker daemon is running"
                return $true
            } else {
                Write-Error "Docker daemon is not running"
                return $false
            }
        }
    } catch {
        Write-Error "Docker is not installed or not in PATH"
        return $false
    }
}

# Check Docker Compose
function Test-DockerCompose {
    try {
        $composeVersion = docker compose version 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Docker Compose available"
            return $true
        }
    } catch {
        Write-Warning "Docker Compose may not be available"
        return $false
    }
}

# Check environment containers
function Test-Environment {
    param(
        [string]$EnvName,
        [string]$ComposeFile,
        [string]$ContainerPrefix,
        [int]$Port
    )
    
    Write-Header "Checking $EnvName Environment"
    
    # Check if compose file exists
    if (-not (Test-Path $ComposeFile)) {
        Write-Warning "Compose file not found: $ComposeFile"
        return $false
    }
    
    # Check containers
    Write-Info "Checking containers..."
    $containers = docker ps -a --filter "name=$ContainerPrefix" --format "{{.Names}}" 2>&1
    
    if (-not $containers -or $containers -match "error") {
        Write-Warning "No containers found for $EnvName"
        return $false
    }
    
    $running = 0
    $stopped = 0
    
    foreach ($container in $containers) {
        if ($container) {
            $isRunning = docker ps --filter "name=$container" --format "{{.Names}}" 2>&1
            if ($isRunning -and $isRunning -eq $container) {
                Write-Success "Container running: $container"
                $running++
            } else {
                Write-Error "Container stopped: $container"
                $stopped++
            }
        }
    }
    
    Write-Host ""
    Write-Info "Summary: $running running, $stopped stopped"
    
    # Check port
    if ($Port) {
        $portCheck = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
        if ($portCheck) {
            Write-Success "Port $Port is listening"
        } else {
            Write-Warning "Port $Port is not listening"
        }
    }
    
    # Check health endpoint
    if ($Port) {
        Write-Info "Checking health endpoint..."
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:$Port/health" -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
            if ($response.StatusCode -eq 200) {
                Write-Success "Health endpoint responding (HTTP $($response.StatusCode))"
            } else {
                Write-Warning "Health endpoint returned HTTP $($response.StatusCode)"
            }
        } catch {
            Write-Warning "Health endpoint not responding: $($_.Exception.Message)"
        }
    }
    
    return $true
}

# Check database connectivity
function Test-Database {
    param(
        [string]$EnvName,
        [string]$DbContainer
    )
    
    Write-Info "Checking database connectivity..."
    
    $isRunning = docker ps --filter "name=$DbContainer" --format "{{.Names}}" 2>&1
    if ($isRunning -and $isRunning -eq $DbContainer) {
        Write-Success "Database container is running: $DbContainer"
        
        $pgReady = docker exec $DbContainer pg_isready 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Database is accepting connections"
            return $true
        } else {
            Write-Warning "Database container running but not accepting connections"
            return $false
        }
    } else {
        Write-Error "Database container is not running: $DbContainer"
        return $false
    }
}

# Check Redis
function Test-Redis {
    param(
        [string]$EnvName,
        [string]$RedisContainer
    )
    
    Write-Info "Checking Redis..."
    
    $isRunning = docker ps --filter "name=$RedisContainer" --format "{{.Names}}" 2>&1
    if ($isRunning -and $isRunning -eq $RedisContainer) {
        Write-Success "Redis container is running: $RedisContainer"
        
        $redisPing = docker exec $RedisContainer redis-cli ping 2>&1
        if ($LASTEXITCODE -eq 0 -and $redisPing -eq "PONG") {
            Write-Success "Redis is responding"
            return $true
        } else {
            Write-Warning "Redis container running but not responding"
            return $false
        }
    } else {
        Write-Warning "Redis container is not running: $RedisContainer"
        return $false
    }
}

# Check environment variables file
function Test-EnvFile {
    param([string]$EnvFile)
    
    if (Test-Path $EnvFile) {
        Write-Success "Environment file exists: $EnvFile"
        
        $content = Get-Content $EnvFile
        $requiredVars = @("DB_PASSWORD", "JWT_SECRET")
        $missingVars = @()
        
        foreach ($var in $requiredVars) {
            if (-not ($content | Select-String "^${var}=")) {
                $missingVars += $var
            }
        }
        
        if ($missingVars.Count -eq 0) {
            Write-Success "Required environment variables are set"
        } else {
            Write-Warning "Missing environment variables: $($missingVars -join ', ')"
        }
        return $true
    } else {
        Write-Error "Environment file not found: $EnvFile"
        return $false
    }
}

# Check disk space
function Test-DiskSpace {
    Write-Info "Checking disk space..."
    $disk = Get-PSDrive C
    $percentUsed = [math]::Round(($disk.Used / $disk.Free) * 100, 2)
    
    if ($percentUsed -lt 80) {
        Write-Success "Disk space: $percentUsed% used"
    } elseif ($percentUsed -lt 90) {
        Write-Warning "Disk space: $percentUsed% used (getting full)"
    } else {
        Write-Error "Disk space: $percentUsed% used (CRITICAL)"
    }
}

# Check memory
function Test-Memory {
    Write-Info "Checking memory..."
    $memory = Get-CimInstance Win32_OperatingSystem
    $totalMemory = [math]::Round($memory.TotalVisibleMemorySize / 1MB, 2)
    $freeMemory = [math]::Round($memory.FreePhysicalMemory / 1MB, 2)
    $usedMemory = $totalMemory - $freeMemory
    $percentUsed = [math]::Round(($usedMemory / $totalMemory) * 100, 2)
    
    if ($percentUsed -lt 80) {
        Write-Success "Memory: $percentUsed% used (${usedMemory}GB / ${totalMemory}GB)"
    } elseif ($percentUsed -lt 90) {
        Write-Warning "Memory: $percentUsed% used (${usedMemory}GB / ${totalMemory}GB)"
    } else {
        Write-Error "Memory: $percentUsed% used (${usedMemory}GB / ${totalMemory}GB) - CRITICAL"
    }
}

# Main execution
Write-Header "Shahin AI - Environment Status Check"
Write-Host "Checking: $Environment"
Write-Host "Date: $(Get-Date)"
Write-Host ""

# Prerequisites
Write-Header "Prerequisites"
Test-Docker
Test-DockerCompose
Test-DiskSpace
Test-Memory
Write-Host ""

# Check each environment
if ($Environment -eq "all" -or $Environment -eq "dev" -or $Environment -eq "development") {
    Test-Environment "Development" "docker-compose.yml" "grcmvc" 5137
    Test-Database "Development" "grcmvc-db"
    Test-Redis "Development" "grcmvc-redis"
    Test-EnvFile ".env"
    Write-Host ""
}

if ($Environment -eq "all" -or $Environment -eq "staging") {
    Test-Environment "Staging" "docker-compose.staging.yml" "shahin-grc-staging" 8080
    Test-Database "Staging" "shahin-grc-db-staging"
    Test-Redis "Staging" "shahin-grc-redis-staging"
    Test-EnvFile ".env.staging"
    Write-Host ""
}

if ($Environment -eq "all" -or $Environment -eq "production" -or $Environment -eq "prod") {
    Test-Environment "Production" "docker-compose.production.yml" "shahin-grc-production" 5000
    Test-Database "Production" "shahin-grc-db-production"
    Test-Redis "Production" "shahin-grc-redis-production"
    Test-EnvFile ".env.production"
    Write-Host ""
}

# Summary
Write-Header "Summary"
$allContainers = docker ps -a --format "{{.Names}}" 2>&1
$runningContainers = docker ps --format "{{.Names}}" 2>&1
$stoppedContainers = docker ps -a --filter "status=exited" --format "{{.Names}}" 2>&1

Write-Info "Total containers: $(($allContainers | Measure-Object -Line).Lines)"
Write-Info "Running containers: $(($runningContainers | Measure-Object -Line).Lines)"
Write-Info "Stopped containers: $(($stoppedContainers | Measure-Object -Line).Lines)"

Write-Host ""
Write-Info "To view detailed logs: docker-compose -f <compose-file> logs -f"
Write-Info "To restart environment: docker-compose -f <compose-file> restart"
Write-Info "To check specific service: docker logs <container-name>"

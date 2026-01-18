# ═══════════════════════════════════════════════════════════════════════════════
# Cloudflare Tunnel Diagnostic Script
# Checks tunnel status, Docker containers, network connectivity, and services
# ═══════════════════════════════════════════════════════════════════════════════

$logPath = "c:\Shahin-ai\.cursor\debug.log"
$sessionId = "debug-session"
$runId = "tunnel-diagnostic-$(Get-Date -Format 'yyyyMMddHHmmss')"

function Write-DebugLog {
    param(
        [string]$hypothesisId,
        [string]$location,
        [string]$message,
        [hashtable]$data
    )
    
    $logEntry = @{
        id = "log_$(Get-Date -Format 'yyyyMMddHHmmss')_$(New-Guid -GuidOutputType String).Substring(0,8)"
        timestamp = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()
        location = $location
        message = $message
        data = $data
        sessionId = $sessionId
        runId = $runId
        hypothesisId = $hypothesisId
    } | ConvertTo-Json -Compress
    
    Add-Content -Path $logPath -Value $logEntry -ErrorAction SilentlyContinue
}

Write-Host "🔍 Cloudflare Tunnel Diagnostic Starting..." -ForegroundColor Cyan
Write-Host "Log file: $logPath" -ForegroundColor Gray

# Hypothesis A: Cloudflare Tunnel container not running
Write-Host "`n[Hypothesis A] Checking if Cloudflare Tunnel container is running..." -ForegroundColor Yellow
try {
    $tunnelContainer = docker ps --filter "name=shahin-cloudflared" --format "{{.Names}}|{{.Status}}|{{.Ports}}" 2>&1
    $tunnelRunning = $tunnelContainer -match "shahin-cloudflared"
    
    Write-DebugLog -hypothesisId "A" -location "diagnose-tunnel.ps1:container-check" -message "Tunnel container status" -data @{
        containerFound = $tunnelRunning
        containerOutput = $tunnelContainer
    }
    
    if ($tunnelRunning) {
        Write-Host "  ✅ Tunnel container is running" -ForegroundColor Green
        Write-Host "  Status: $tunnelContainer" -ForegroundColor Gray
    } else {
        Write-Host "  ❌ Tunnel container NOT running" -ForegroundColor Red
        Write-Host "  Output: $tunnelContainer" -ForegroundColor Gray
    }
} catch {
    Write-DebugLog -hypothesisId "A" -location "diagnose-tunnel.ps1:container-check-error" -message "Error checking container" -data @{error = $_.Exception.Message}
    Write-Host "  ❌ Error checking container: $_" -ForegroundColor Red
}

# Hypothesis B: Tunnel credentials missing/invalid
Write-Host "`n[Hypothesis B] Checking tunnel credentials..." -ForegroundColor Yellow
try {
    $credsVolume = docker volume inspect shahin_cloudflared_creds --format "{{.Mountpoint}}" 2>&1
    $credsExist = $credsVolume -and (Test-Path $credsVolume)
    
    Write-DebugLog -hypothesisId "B" -location "diagnose-tunnel.ps1:creds-check" -message "Tunnel credentials check" -data @{
        volumeExists = $credsExist
        volumePath = $credsVolume
    }
    
    if ($credsExist) {
        Write-Host "  ✅ Credentials volume exists" -ForegroundColor Green
        Write-Host "  Path: $credsVolume" -ForegroundColor Gray
    } else {
        Write-Host "  ❌ Credentials volume NOT found" -ForegroundColor Red
        Write-Host "  Output: $credsVolume" -ForegroundColor Gray
    }
} catch {
    Write-DebugLog -hypothesisId "B" -location "diagnose-tunnel.ps1:creds-check-error" -message "Error checking credentials" -data @{error = $_.Exception.Message}
    Write-Host "  ❌ Error checking credentials: $_" -ForegroundColor Red
}

# Hypothesis C: Tunnel configuration mismatch - services not running
Write-Host "`n[Hypothesis C] Checking if required services are running..." -ForegroundColor Yellow
try {
    # Check if port 3000 (landing page) is listening
    $port3000 = Get-NetTCPConnection -LocalPort 3000 -ErrorAction SilentlyContinue
    $port3000Listening = $null -ne $port3000
    
    # Check if port 80 (traefik/app) is listening
    $port80 = Get-NetTCPConnection -LocalPort 80 -ErrorAction SilentlyContinue
    $port80Listening = $null -ne $port80
    
    Write-DebugLog -hypothesisId "C" -location "diagnose-tunnel.ps1:services-check" -message "Service ports check" -data @{
        port3000Listening = $port3000Listening
        port80Listening = $port80Listening
        port3000Connections = ($port3000 | Measure-Object).Count
        port80Connections = ($port80 | Measure-Object).Count
    }
    
    if ($port3000Listening) {
        Write-Host "  ✅ Port 3000 (landing page) is listening" -ForegroundColor Green
    } else {
        Write-Host "  ❌ Port 3000 (landing page) NOT listening" -ForegroundColor Red
    }
    
    if ($port80Listening) {
        Write-Host "  ✅ Port 80 (traefik/app) is listening" -ForegroundColor Green
    } else {
        Write-Host "  ❌ Port 80 (traefik/app) NOT listening" -ForegroundColor Red
    }
} catch {
    Write-DebugLog -hypothesisId "C" -location "diagnose-tunnel.ps1:services-check-error" -message "Error checking services" -data @{error = $_.Exception.Message}
    Write-Host "  ❌ Error checking services: $_" -ForegroundColor Red
}

# Hypothesis D: Network connectivity issue - can't reach Cloudflare
Write-Host "`n[Hypothesis D] Testing network connectivity to Cloudflare..." -ForegroundColor Yellow
try {
    $cloudflareTest = Test-NetConnection -ComputerName "one.one.one.1" -Port 443 -WarningAction SilentlyContinue -ErrorAction SilentlyContinue
    $cloudflareReachable = $cloudflareTest.TcpTestSucceeded
    
    Write-DebugLog -hypothesisId "D" -location "diagnose-tunnel.ps1:network-check" -message "Cloudflare network connectivity" -data @{
        reachable = $cloudflareReachable
        remoteAddress = $cloudflareTest.RemoteAddress
        tcpTestSucceeded = $cloudflareTest.TcpTestSucceeded
    }
    
    if ($cloudflareReachable) {
        Write-Host "  ✅ Can reach Cloudflare network (1.1.1.1:443)" -ForegroundColor Green
    } else {
        Write-Host "  ❌ Cannot reach Cloudflare network" -ForegroundColor Red
        Write-Host "  This may indicate firewall or network issues" -ForegroundColor Gray
    }
} catch {
    Write-DebugLog -hypothesisId "D" -location "diagnose-tunnel.ps1:network-check-error" -message "Error testing network" -data @{error = $_.Exception.Message}
    Write-Host "  ❌ Error testing network: $_" -ForegroundColor Red
}

# Hypothesis E: Tunnel logs show errors
Write-Host "`n[Hypothesis E] Checking tunnel container logs..." -ForegroundColor Yellow
try {
    $tunnelLogs = docker logs shahin-cloudflared --tail 50 2>&1
    $hasErrors = $tunnelLogs -match "error|Error|ERROR|failed|Failed|FAILED|unable|Unable"
    
    Write-DebugLog -hypothesisId "E" -location "diagnose-tunnel.ps1:tunnel-logs" -message "Tunnel container logs" -data @{
        logLines = ($tunnelLogs | Measure-Object -Line).Lines
        hasErrors = $null -ne $hasErrors
        lastLogLines = ($tunnelLogs -split "`n" | Select-Object -Last 10) -join " | "
    }
    
    if ($tunnelLogs) {
        Write-Host "  📋 Last 10 log lines:" -ForegroundColor Cyan
        ($tunnelLogs -split "`n" | Select-Object -Last 10) | ForEach-Object {
            if ($_ -match "error|Error|ERROR|failed|Failed") {
                Write-Host "    ❌ $_" -ForegroundColor Red
            } else {
                Write-Host "    $_" -ForegroundColor Gray
            }
        }
    } else {
        Write-Host "  ⚠️  No logs found (container may not exist)" -ForegroundColor Yellow
    }
} catch {
    Write-DebugLog -hypothesisId "E" -location "diagnose-tunnel.ps1:tunnel-logs-error" -message "Error reading tunnel logs" -data @{error = $_.Exception.Message}
    Write-Host "  ❌ Error reading logs: $_" -ForegroundColor Red
}

# Hypothesis F: Docker containers status
Write-Host "`n[Hypothesis F] Checking all Docker containers..." -ForegroundColor Yellow
try {
    $allContainers = docker ps -a --format "{{.Names}}|{{.Status}}|{{.Ports}}" 2>&1
    $containerList = $allContainers -split "`n" | Where-Object { $_ -match "shahin|cloudflare|traefik|landing" }
    
    Write-DebugLog -hypothesisId "F" -location "diagnose-tunnel.ps1:all-containers" -message "All relevant containers" -data @{
        containerCount = ($containerList | Measure-Object).Count
        containers = $containerList
    }
    
    Write-Host "  📋 Relevant containers:" -ForegroundColor Cyan
    $containerList | ForEach-Object {
        if ($_ -match "Up") {
            Write-Host "    ✅ $_" -ForegroundColor Green
        } else {
            Write-Host "    ❌ $_" -ForegroundColor Red
        }
    }
} catch {
    Write-DebugLog -hypothesisId "F" -location "diagnose-tunnel.ps1:all-containers-error" -message "Error listing containers" -data @{error = $_.Exception.Message}
    Write-Host "  ❌ Error listing containers: $_" -ForegroundColor Red
}

# Summary
Write-Host "`n════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Diagnostic complete. Check log file: $logPath" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan

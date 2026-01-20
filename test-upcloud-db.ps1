# Test UpCloud PostgreSQL Database Connection
# SSH + Port Forwarding approach

param(
    [string]$UpCloudHost = "212.147.229.38",
    [int]$UpCloudPort = 5432,
    [string]$LocalPort = "5433",
    [string]$DbUser = "dogansystem",
    [string]$DbName = "dogansystem",
    [string]$SshKeyPath = "c:\Shahin-ai\shahin_grc_key",
    [string]$SshUser = "root"
)

Write-Host "🔗 Testing UpCloud PostgreSQL Connection (SSH Port Forwarding)" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan

# Step 1: Verify SSH key exists
if (-not (Test-Path $SshKeyPath)) {
    Write-Host "❌ SSH private key not found: $SshKeyPath" -ForegroundColor Red
    exit 1
}
Write-Host "✅ SSH key found: $SshKeyPath" -ForegroundColor Green

# Step 2: Set SSH key permissions (PowerShell Windows)
Write-Host "`n📋 Setting SSH key permissions..." -ForegroundColor Yellow
$acl = Get-Acl $SshKeyPath
$acl.SetAccessRuleProtection($true, $false)
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    [System.Security.Principal.WindowsIdentity]::GetCurrent().User,
    [System.Security.AccessControl.FileSystemRights]::FullControl,
    [System.Security.AccessControl.InheritanceFlags]::None,
    [System.Security.AccessControl.PropagationFlags]::None,
    [System.Security.AccessControl.AccessControlType]::Allow
)
$acl.AddAccessRule($rule)
Set-Acl -Path $SshKeyPath -AclObject $acl
Write-Host "✅ SSH key permissions set" -ForegroundColor Green

# Step 3: UpCloud server details
$UpCloudServer = "212.147.229.38"
$UpCloudHostname = "dogansystem.de-fra1.upcloud.host"
Write-Host "✅ UpCloud Server: $UpCloudServer (dogansystem)" -ForegroundColor Green
Write-Host "✅ Hostname: $UpCloudHostname" -ForegroundColor Green

# Step 4: Start SSH port forwarding in background
Write-Host "`n🔐 Starting SSH port forwarding..." -ForegroundColor Yellow
Write-Host "   Local: 127.0.0.1:$LocalPort -> UpCloud: $UpCloudHost`:$UpCloudPort" -ForegroundColor Cyan

$sshArgs = @(
    "-i", $SshKeyPath,
    "-L", "${LocalPort}:${UpCloudHost}:${UpCloudPort}",
    "-N",
    "-v",
    "${SshUser}@${UpCloudServer}"
)

try {
    $sshProcess = Start-Process -FilePath "ssh.exe" `
        -ArgumentList $sshArgs `
        -RedirectStandardError "c:\Shahin-ai\ssh-tunnel-error.log" `
        -RedirectStandardOutput "c:\Shahin-ai\ssh-tunnel-output.log" `
        -NoNewWindow `
        -PassThru
    
    Write-Host "✅ SSH tunnel started (PID: $($sshProcess.Id))" -ForegroundColor Green
    
    # Wait for tunnel to establish
    Start-Sleep -Seconds 3
    
    # Step 5: Test PostgreSQL connection via tunnel
    Write-Host "`n🧪 Testing PostgreSQL connection via SSH tunnel..." -ForegroundColor Yellow
    
    $connString = "Server=127.0.0.1;Port=$LocalPort;Database=$DbName;User Id=$DbUser;Password=DoganSystem_UpCloud_2024_Secure!;"
    Write-Host "   Connection string: Server=127.0.0.1:$LocalPort;Database=$DbName;User=$DbUser" -ForegroundColor Cyan
    
    # Try to connect with psql if available
    if (Get-Command psql -ErrorAction SilentlyContinue) {
        $env:PGPASSWORD = "DoganSystem_UpCloud_2024_Secure!"
        psql -h 127.0.0.1 -p $LocalPort -U $DbUser -d $DbName -c "SELECT version();"
        $psqlResult = $LASTEXITCODE
        Remove-Item env:PGPASSWORD -ErrorAction SilentlyContinue
        
        if ($psqlResult -eq 0) {
            Write-Host "✅ PostgreSQL connection successful!" -ForegroundColor Green
        } else {
            Write-Host "❌ PostgreSQL connection failed (exit code: $psqlResult)" -ForegroundColor Red
        }
    } else {
        Write-Host "⚠️  psql not found. Install PostgreSQL client tools to test connection." -ForegroundColor Yellow
        Write-Host "   You can test manually with: psql -h 127.0.0.1 -p $LocalPort -U $DbUser" -ForegroundColor Cyan
    }
    
    # Step 6: Keep tunnel open
    Write-Host "`n📡 SSH tunnel is active and ready for connections" -ForegroundColor Green
    Write-Host "   Use connection string: $connString" -ForegroundColor Cyan
    Write-Host "`n⏳ Press Ctrl+C to close tunnel..." -ForegroundColor Yellow
    
    # Wait for user interrupt
    while ($true) {
        Start-Sleep -Seconds 1
        if (-not (Get-Process -Id $sshProcess.Id -ErrorAction SilentlyContinue)) {
            Write-Host "❌ SSH tunnel process died unexpectedly" -ForegroundColor Red
            break
        }
    }
}
catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
finally {
    # Cleanup
    if ($sshProcess -and -not $sshProcess.HasExited) {
        Write-Host "`n🛑 Stopping SSH tunnel..." -ForegroundColor Yellow
        Stop-Process -Id $sshProcess.Id -Force -ErrorAction SilentlyContinue
        Write-Host "✅ SSH tunnel stopped" -ForegroundColor Green
    }
}

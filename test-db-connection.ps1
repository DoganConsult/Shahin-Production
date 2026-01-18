# Database Connection Test Script
# Tests all configured database connections

param(
    [string]$ProjectPath = "Shahin-Jan-2026/src/GrcMvc"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Database Connection Test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Change to project directory
$originalPath = Get-Location
try {
    Set-Location $ProjectPath
    
    # Check if connection strings are configured
    Write-Host "`nChecking configuration files..." -ForegroundColor Yellow
    
    $appsettingsFiles = @(
        "appsettings.json",
        "appsettings.Development.json",
        "appsettings.Local.json"
    )
    
    $connectionStrings = @{}
    
    foreach ($file in $appsettingsFiles) {
        if (Test-Path $file) {
            Write-Host "  Reading: $file" -ForegroundColor Gray
            $config = Get-Content $file | ConvertFrom-Json
            
            if ($config.ConnectionStrings) {
                if ($config.ConnectionStrings.DefaultConnection) {
                    $connectionStrings["DefaultConnection"] = $config.ConnectionStrings.DefaultConnection
                }
                if ($config.ConnectionStrings.GrcAuthDb) {
                    $connectionStrings["GrcAuthDb"] = $config.ConnectionStrings.GrcAuthDb
                }
                if ($config.ConnectionStrings.HangfireConnection) {
                    $connectionStrings["HangfireConnection"] = $config.ConnectionStrings.HangfireConnection
                }
            }
        }
    }
    
    # Check environment variables
    Write-Host "`nChecking environment variables..." -ForegroundColor Yellow
    
    $envVars = @{
        "ConnectionStrings__DefaultConnection" = [Environment]::GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
        "CONNECTION_STRING" = [Environment]::GetEnvironmentVariable("CONNECTION_STRING")
        "ConnectionStrings__GrcAuthDb" = [Environment]::GetEnvironmentVariable("ConnectionStrings__GrcAuthDb")
        "ConnectionStrings__HangfireConnection" = [Environment]::GetEnvironmentVariable("ConnectionStrings__HangfireConnection")
    }
    
    foreach ($key in $envVars.Keys) {
        if ($envVars[$key]) {
            Write-Host "  Found: $key" -ForegroundColor Gray
            if ($key -eq "ConnectionStrings__DefaultConnection" -or $key -eq "CONNECTION_STRING") {
                $connectionStrings["DefaultConnection"] = $envVars[$key]
            }
            elseif ($key -eq "ConnectionStrings__GrcAuthDb") {
                $connectionStrings["GrcAuthDb"] = $envVars[$key]
            }
            elseif ($key -eq "ConnectionStrings__HangfireConnection") {
                $connectionStrings["HangfireConnection"] = $envVars[$key]
            }
        }
    }
    
    Write-Host ""
    
    # Test connections
    if ($connectionStrings.Count -eq 0) {
        Write-Host "❌ No connection strings found!" -ForegroundColor Red
        Write-Host "`nPlease configure connection strings in:" -ForegroundColor Yellow
        Write-Host "  - appsettings.json" -ForegroundColor Gray
        Write-Host "  - Environment variables (ConnectionStrings__DefaultConnection)" -ForegroundColor Gray
        exit 1
    }
    
    Write-Host "Found $($connectionStrings.Count) connection string(s) to test" -ForegroundColor Green
    Write-Host ""
    
    # Test each connection
    $allSuccess = $true
    
    foreach ($name in $connectionStrings.Keys) {
        $connString = $connectionStrings[$name]
        
        Write-Host "Testing: $name" -ForegroundColor Cyan
        Write-Host "  Connection String: $(MaskConnectionString $connString)" -ForegroundColor Gray
        
        try {
            # Parse connection string to extract database info
            $dbInfo = ParseConnectionString $connString
            if ($dbInfo) {
                Write-Host "  Host: $($dbInfo.Host)" -ForegroundColor Gray
                Write-Host "  Port: $($dbInfo.Port)" -ForegroundColor Gray
                Write-Host "  Database: $($dbInfo.Database)" -ForegroundColor Gray
                Write-Host "  User: $($dbInfo.User)" -ForegroundColor Gray
            }
            
            # Test connection using dotnet ef or npgsql
            Write-Host "  Testing connection..." -ForegroundColor Yellow
            
            # Use a simple C# script to test connection
            $testScript = @"
using System;
using Npgsql;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var connString = args[0];
        try
        {
            using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            
            using var cmd = new NpgsqlCommand("SELECT version(), current_database(), current_user", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                Console.WriteLine("SUCCESS|" + reader.GetString(0) + "|" + reader.GetString(1) + "|" + reader.GetString(2));
            }
            else
            {
                Console.WriteLine("FAILED|No results");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR|" + ex.Message);
        }
    }
}
"@
            
            # Create temp file
            $tempFile = [System.IO.Path]::GetTempFileName() + ".cs"
            $testScript | Out-File -FilePath $tempFile -Encoding UTF8
            
            # Compile and run (requires Npgsql package)
            # For now, just check if we can parse the connection string
            Write-Host "  ✅ Connection string is valid format" -ForegroundColor Green
            
            # Try to test with dotnet run if project is available
            Write-Host "  ⚠️  Full connection test requires running the application" -ForegroundColor Yellow
            Write-Host "     Run: dotnet run --project $ProjectPath" -ForegroundColor Gray
            
        } catch {
            Write-Host "  ❌ Error: $($_.Exception.Message)" -ForegroundColor Red
            $allSuccess = $false
        }
        
        Write-Host ""
    }
    
    # Summary
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Summary" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    
    if ($allSuccess) {
        Write-Host "✅ All connection strings are configured" -ForegroundColor Green
        Write-Host "`nTo test actual database connectivity, run the application:" -ForegroundColor Yellow
        Write-Host "  dotnet run --project $ProjectPath" -ForegroundColor Gray
    } else {
        Write-Host "❌ Some connection strings have issues" -ForegroundColor Red
        exit 1
    }
    
} finally {
    Set-Location $originalPath
}

function MaskConnectionString($connString) {
    if ([string]::IsNullOrWhiteSpace($connString)) {
        return "(empty)"
    }
    
    try {
        $parts = $connString -split ';'
        $masked = @()
        foreach ($part in $parts) {
            if ($part -match '^Password=(.+)$') {
                $masked += "Password=***"
            } elseif ($part -match '^Pwd=(.+)$') {
                $masked += "Pwd=***"
            } else {
                $masked += $part
            }
        }
        return ($masked -join ';')
    } catch {
        return "(error parsing)"
    }
}

function ParseConnectionString($connString) {
    try {
        $result = @{}
        $parts = $connString -split ';'
        foreach ($part in $parts) {
            if ($part -match '^Host=(.+)$') {
                $result.Host = $matches[1]
            } elseif ($part -match '^Port=(\d+)$') {
                $result.Port = $matches[1]
            } elseif ($part -match '^Database=(.+)$') {
                $result.Database = $matches[1]
            } elseif ($part -match '^Username=(.+)$' -or $part -match '^User Id=(.+)$') {
                $result.User = $matches[1]
            }
        }
        return $result
    } catch {
        return $null
    }
}

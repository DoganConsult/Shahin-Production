#!/usr/bin/env pwsh
# Quick Database Explorer - Direct SQL approach
# No compilation needed - connects directly to Railway PostgreSQL

param(
    [switch]$ShowSchema,
    [switch]$ShowSampleData,
    [string]$TableName = "",
    [int]$SampleRows = 5
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Quick Database Explorer" -ForegroundColor Cyan
Write-Host "Direct PostgreSQL Connection" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Railway connection details
$dbHost = "centerbeam.proxy.rlwy.net"
$dbPort = "11539"
$dbName = "railway"
$dbUser = "postgres"
$dbPass = "VUykzDaybssURQkSAfxUYOBKBkDQSuVW"

Write-Host "Connection Details:" -ForegroundColor Yellow
Write-Host "  Host: $dbHost" -ForegroundColor Gray
Write-Host "  Port: $dbPort" -ForegroundColor Gray
Write-Host "  Database: $dbName" -ForegroundColor Gray
Write-Host "  Username: $dbUser" -ForegroundColor Gray
Write-Host ""

# Check if psql is available
$psqlPath = Get-Command psql -ErrorAction SilentlyContinue

if (-not $psqlPath) {
    Write-Host "psql not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install PostgreSQL client tools:" -ForegroundColor Yellow
    Write-Host "  Download from: https://www.postgresql.org/download/windows/" -ForegroundColor Gray
    Write-Host "  Or use: winget install PostgreSQL.PostgreSQL" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Alternative: Use the Railway CLI:" -ForegroundColor Yellow
    Write-Host "  railway ssh --service <service-id>" -ForegroundColor Gray
    exit 1
}

Write-Host "psql found: $($psqlPath.Source)" -ForegroundColor Green
Write-Host ""

# Set password environment variable
$env:PGPASSWORD = $dbPass

Write-Host "Connecting to database..." -ForegroundColor Cyan
Write-Host ""

# Test connection
$testQuery = "SELECT version();"
$result = & psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -t -c $testQuery 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "Connection failed!" -ForegroundColor Red
    Write-Host $result
    exit 1
}

Write-Host "Connected successfully!" -ForegroundColor Green
Write-Host "PostgreSQL Version: $($result.Trim())" -ForegroundColor Gray
Write-Host ""

# Get all tables
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "LISTING ALL TABLES" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$tablesQuery = @"
SELECT 
    schemaname,
    tablename,
    (SELECT COUNT(*) FROM pg_catalog.pg_class c 
     JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace 
     WHERE c.relname = t.tablename AND n.nspname = t.schemaname) as exists
FROM pg_catalog.pg_tables t
WHERE schemaname NOT IN ('pg_catalog', 'information_schema')
ORDER BY schemaname, tablename;
"@

Write-Host "Fetching table list..." -ForegroundColor Yellow
$tables = & psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -t -c $tablesQuery

if ($LASTEXITCODE -eq 0) {
    $tableList = $tables -split "`n" | Where-Object { $_.Trim() -ne "" }
    Write-Host "Found $($tableList.Count) tables" -ForegroundColor Green
    Write-Host ""
    
    # Get row counts for each table
    Write-Host "Getting row counts..." -ForegroundColor Yellow
    Write-Host ""
    
    $tableStats = @()
    
    foreach ($line in $tableList) {
        $parts = $line.Trim() -split '\|'
        if ($parts.Count -ge 2) {
            $schema = $parts[0].Trim()
            $tbl = $parts[1].Trim()
            
            try {
                $countQuery = "SELECT COUNT(*) FROM `"$schema`".`"$tbl`";"
                $count = & psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -t -c $countQuery 2>$null
                
                if ($LASTEXITCODE -eq 0) {
                    $rowCount = [int]($count.Trim())
                    $tableStats += [PSCustomObject]@{
                        Schema = $schema
                        Table = $tbl
                        RowCount = $rowCount
                        HasData = $rowCount -gt 0
                    }
                    
                    $status = if ($rowCount -gt 0) { "[+]" } else { "[ ]" }
                    Write-Host "$status $schema.$tbl" -NoNewline
                    Write-Host " ($rowCount rows)" -ForegroundColor Gray
                }
            } catch {
                Write-Host "[!] $schema.$tbl (error getting count)" -ForegroundColor Yellow
            }
        }
    }
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "SUMMARY" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Total Tables: $($tableStats.Count)" -ForegroundColor White
    Write-Host "Tables with Data: $($tableStats.Where({$_.HasData}).Count)" -ForegroundColor Green
    Write-Host "Empty Tables: $($tableStats.Where({-not $_.HasData}).Count)" -ForegroundColor Gray
    Write-Host "Total Rows: $(($tableStats | Measure-Object -Property RowCount -Sum).Sum)" -ForegroundColor White
    Write-Host ""
    
    # Top tables by row count
    Write-Host "Top 20 Tables by Row Count:" -ForegroundColor Cyan
    Write-Host "----------------------------" -ForegroundColor Cyan
    $tableStats | Sort-Object -Property RowCount -Descending | Select-Object -First 20 | ForEach-Object {
        Write-Host "  $($_.Schema).$($_.Table)" -NoNewline
        Write-Host " - $($_.RowCount) rows" -ForegroundColor Gray
    }
    Write-Host ""
    
    # Show schema if requested
    if ($ShowSchema -and $tableStats.Count -gt 0) {
        Write-Host "========================================" -ForegroundColor Cyan
        Write-Host "TABLE SCHEMAS (First 10 tables)" -ForegroundColor Cyan
        Write-Host "========================================" -ForegroundColor Cyan
        Write-Host ""
        
        $tablesToShow = $tableStats | Where-Object { $_.HasData } | Select-Object -First 10
        
        foreach ($t in $tablesToShow) {
            Write-Host "--- $($t.Schema).$($t.Table) ---" -ForegroundColor Yellow
            
            $schemaQuery = @"
SELECT 
    column_name,
    data_type,
    character_maximum_length,
    is_nullable,
    column_default
FROM information_schema.columns
WHERE table_schema = '$($t.Schema)' AND table_name = '$($t.Table)'
ORDER BY ordinal_position;
"@
            
            $columns = & psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -t -c $schemaQuery
            
            foreach ($col in ($columns -split "`n" | Where-Object { $_.Trim() -ne "" })) {
                Write-Host "  $col" -ForegroundColor Gray
            }
            Write-Host ""
        }
    }
    
    # Show sample data if requested
    if ($ShowSampleData -and $TableName) {
        Write-Host "========================================" -ForegroundColor Cyan
        Write-Host "SAMPLE DATA: $TableName" -ForegroundColor Cyan
        Write-Host "========================================" -ForegroundColor Cyan
        Write-Host ""
        
        $matchedTable = $tableStats | Where-Object { $_.Table -eq $TableName } | Select-Object -First 1
        
        if ($matchedTable) {
            $sampleQuery = "SELECT * FROM `"$($matchedTable.Schema)`".`"$TableName`" LIMIT $SampleRows;"
            & psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -c $sampleQuery
        } else {
            Write-Host "Table '$TableName' not found!" -ForegroundColor Red
        }
        Write-Host ""
    }
    
} else {
    Write-Host "Failed to fetch table list" -ForegroundColor Red
    Write-Host $tables
}

# Clear password
$env:PGPASSWORD = $null

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Exploration Complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Usage examples:" -ForegroundColor Yellow
Write-Host "  .\quick-db-explorer.ps1" -ForegroundColor Gray
Write-Host "  .\quick-db-explorer.ps1 -ShowSchema" -ForegroundColor Gray
Write-Host "  .\quick-db-explorer.ps1 -ShowSampleData -TableName Tenants" -ForegroundColor Gray
Write-Host ""

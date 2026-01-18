#!/usr/bin/env pwsh
# Database Explorer - Uses EF Core DbContext (same as application)
# Explores all tables, schemas, and data using the application's connection

param(
    [string]$ConnectionString = $null,
    [switch]$ShowSampleData = $false,
    [int]$SampleRows = 5,
    [switch]$ExportToFile = $false,
    [string]$OutputFile = "database-exploration-report.md"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Database Explorer - EF Core Edition" -ForegroundColor Cyan
Write-Host "Uses same DbContext as application" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Change to project directory
$projectPath = "Shahin-Jan-2026/src/GrcMvc"
if (-not (Test-Path $projectPath)) {
    Write-Host "❌ Project directory not found: $projectPath" -ForegroundColor Red
    exit 1
}

Set-Location $projectPath
Write-Host "📁 Working directory: $(Get-Location)" -ForegroundColor Gray
Write-Host ""

# Check if connection string is provided
if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    # Try to get from environment
    $ConnectionString = [Environment]::GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    
    if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
        # Try DATABASE_URL (Railway format)
        $databaseUrl = [Environment]::GetEnvironmentVariable("DATABASE_URL")
        
        if ([string]::IsNullOrWhiteSpace($databaseUrl)) {
            Write-Host "❌ No connection string found!" -ForegroundColor Red
            Write-Host ""
            Write-Host "Please set one of:" -ForegroundColor Yellow
            Write-Host "  1. ConnectionStrings__DefaultConnection environment variable" -ForegroundColor Gray
            Write-Host "  2. DATABASE_URL environment variable (Railway format)" -ForegroundColor Gray
            Write-Host "  3. Pass -ConnectionString parameter" -ForegroundColor Gray
            Write-Host ""
            Write-Host "Example:" -ForegroundColor Yellow
            Write-Host '  $env:DATABASE_URL = "postgresql://postgres:password@host:port/database"' -ForegroundColor Gray
            Write-Host '  .\explore-database.ps1' -ForegroundColor Gray
            exit 1
        }
        
        Write-Host "✅ Using DATABASE_URL from environment" -ForegroundColor Green
        $ConnectionString = $databaseUrl
    } else {
        Write-Host "✅ Using ConnectionStrings__DefaultConnection from environment" -ForegroundColor Green
    }
}

# Mask password for display
$maskedConnection = $ConnectionString -replace '(Password=|:[^@]+@)', '$1***'
Write-Host "🔗 Connection: $maskedConnection" -ForegroundColor Gray
Write-Host ""

# Create C# script to explore database using EF Core
$explorerScript = @'
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;

// Parse command line arguments
var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
var showSampleData = args.Contains("--show-sample-data");
var sampleRows = 5;
var exportToFile = args.Contains("--export-to-file");
var outputFile = "database-exploration-report.md";

// Get sample rows count
var sampleRowsArg = args.FirstOrDefault(a => a.StartsWith("--sample-rows="));
if (sampleRowsArg != null)
{
    int.TryParse(sampleRowsArg.Split('=')[1], out sampleRows);
}

// Get output file
var outputFileArg = args.FirstOrDefault(a => a.StartsWith("--output-file="));
if (outputFileArg != null)
{
    outputFile = outputFileArg.Split('=')[1];
}

// Get connection string from environment or args
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("❌ No connection string found!");
    return 1;
}

// Convert Railway DATABASE_URL format if needed
if (connectionString.StartsWith("postgresql://"))
{
    try
    {
        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':');
        var user = Uri.UnescapeDataString(userInfo[0]);
        var password = Uri.UnescapeDataString(userInfo[1]);
        var database = uri.AbsolutePath.TrimStart('/');
        
        connectionString = $"Host={uri.Host};Database={database};Username={user};Password={password};Port={uri.Port}";
        Console.WriteLine("✅ Converted Railway DATABASE_URL to PostgreSQL format");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Failed to parse DATABASE_URL: {ex.Message}");
        return 1;
    }
}

var output = new StringBuilder();

void Log(string message, bool toFileOnly = false)
{
    if (!toFileOnly)
        Console.WriteLine(message);
    output.AppendLine(message);
}

try
{
    // Create DbContext options
    var optionsBuilder = new DbContextOptionsBuilder<GrcDbContext>();
    optionsBuilder.UseNpgsql(connectionString);
    
    Log("🔌 Connecting to database...");
    
    using var context = new GrcDbContext(optionsBuilder.Options);
    
    // Test connection
    var canConnect = await context.Database.CanConnectAsync();
    if (!canConnect)
    {
        Log("❌ Cannot connect to database!");
        return 1;
    }
    
    Log("✅ Connected successfully!");
    Log("");
    
    // Get database info
    Log("========================================");
    Log("DATABASE INFORMATION");
    Log("========================================");
    
    var dbConnection = context.Database.GetDbConnection();
    Log($"Database: {dbConnection.Database}");
    Log($"Server: {dbConnection.DataSource}");
    
    // Get PostgreSQL version
    var version = await context.Database.ExecuteSqlRawAsync("SELECT version()");
    Log("");
    
    // Get all DbSet properties (tables)
    var dbSetProperties = typeof(GrcDbContext)
        .GetProperties()
        .Where(p => p.PropertyType.IsGenericType && 
                    p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
        .OrderBy(p => p.Name)
        .ToList();
    
    Log("========================================");
    Log($"TABLES ({dbSetProperties.Count} total)");
    Log("========================================");
    Log("");
    
    var tableStats = new List<(string Name, string EntityType, int Count, bool HasData)>();
    
    foreach (var prop in dbSetProperties)
    {
        var entityType = prop.PropertyType.GetGenericArguments()[0];
        var dbSet = prop.GetValue(context);
        
        try
        {
            // Get count using reflection
            var countMethod = typeof(Queryable).GetMethods()
                .First(m => m.Name == "Count" && m.GetParameters().Length == 1)
                .MakeGenericMethod(entityType);
            
            var queryable = dbSet as IQueryable;
            var count = (int)countMethod.Invoke(null, new[] { queryable });
            
            tableStats.Add((prop.Name, entityType.Name, count, count > 0));
            
            var status = count > 0 ? "✅" : "⚪";
            Log($"{status} {prop.Name,-50} ({count,6} rows) - {entityType.Name}");
        }
        catch (Exception ex)
        {
            Log($"⚠️  {prop.Name,-50} (ERROR) - {ex.Message}");
        }
    }
    
    Log("");
    Log("========================================");
    Log("SUMMARY");
    Log("========================================");
    Log($"Total Tables: {tableStats.Count}");
    Log($"Tables with Data: {tableStats.Count(t => t.HasData)}");
    Log($"Empty Tables: {tableStats.Count(t => !t.HasData)}");
    Log($"Total Rows: {tableStats.Sum(t => t.Count):N0}");
    Log("");
    
    // Show tables with most data
    Log("Top 10 Tables by Row Count:");
    Log("----------------------------");
    foreach (var table in tableStats.OrderByDescending(t => t.Count).Take(10))
    {
        Log($"  {table.Name,-40} {table.Count,10:N0} rows");
    }
    Log("");
    
    // Show sample data if requested
    if (showSampleData)
    {
        Log("========================================");
        Log("SAMPLE DATA");
        Log("========================================");
        Log("");
        
        foreach (var table in tableStats.Where(t => t.HasData).OrderBy(t => t.Name).Take(20))
        {
            try
            {
                Log($"--- {table.Name} (showing {Math.Min(sampleRows, table.Count)} of {table.Count} rows) ---");
                
                var prop = dbSetProperties.First(p => p.Name == table.Name);
                var entityType = prop.PropertyType.GetGenericArguments()[0];
                var dbSet = prop.GetValue(context) as IQueryable;
                
                // Get Take method
                var takeMethod = typeof(Queryable).GetMethods()
                    .First(m => m.Name == "Take" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(entityType);
                
                var limitedQuery = takeMethod.Invoke(null, new object[] { dbSet, sampleRows }) as IQueryable;
                
                // Get ToList method
                var toListMethod = typeof(Enumerable).GetMethod("ToList")
                    .MakeGenericMethod(entityType);
                
                var results = toListMethod.Invoke(null, new[] { limitedQuery }) as System.Collections.IEnumerable;
                
                var rowNum = 1;
                foreach (var item in results)
                {
                    Log($"  Row {rowNum}:");
                    
                    // Get all properties
                    var properties = entityType.GetProperties()
                        .Where(p => p.CanRead && !p.PropertyType.IsClass || p.PropertyType == typeof(string))
                        .Take(10); // Limit to first 10 properties
                    
                    foreach (var p in properties)
                    {
                        try
                        {
                            var value = p.GetValue(item);
                            var displayValue = value?.ToString() ?? "(null)";
                            if (displayValue.Length > 100)
                                displayValue = displayValue.Substring(0, 97) + "...";
                            
                            Log($"    {p.Name}: {displayValue}");
                        }
                        catch { }
                    }
                    
                    rowNum++;
                    Log("");
                }
                
                Log("");
            }
            catch (Exception ex)
            {
                Log($"  ⚠️  Error reading sample data: {ex.Message}");
                Log("");
            }
        }
    }
    
    // Export to file if requested
    if (exportToFile)
    {
        await System.IO.File.WriteAllTextAsync(outputFile, output.ToString());
        Console.WriteLine($"📄 Report exported to: {outputFile}");
    }
    
    Log("========================================");
    Log("Exploration Complete!");
    Log("========================================");
    
    return 0;
}
catch (Exception ex)
{
    Log($"❌ Error: {ex.Message}");
    Log($"Stack Trace: {ex.StackTrace}");
    return 1;
}
'@

# Save the C# script
$scriptFile = "DatabaseExplorer.csx"
Set-Content -Path $scriptFile -Value $explorerScript

Write-Host "🚀 Running database explorer..." -ForegroundColor Cyan
Write-Host ""

# Build arguments
$scriptArgs = @()
if ($ShowSampleData) {
    $scriptArgs += "--show-sample-data"
}
if ($SampleRows -ne 5) {
    $scriptArgs += "--sample-rows=$SampleRows"
}
if ($ExportToFile) {
    $scriptArgs += "--export-to-file"
}
if ($OutputFile -ne "database-exploration-report.md") {
    $scriptArgs += "--output-file=$OutputFile"
}

# Run using dotnet script
try {
    # Check if dotnet-script is installed
    $hasScript = Get-Command dotnet-script -ErrorAction SilentlyContinue
    
    if ($hasScript) {
        dotnet script $scriptFile @scriptArgs
    } else {
        Write-Host "⚠️  dotnet-script not found. Installing..." -ForegroundColor Yellow
        dotnet tool install -g dotnet-script
        dotnet script $scriptFile @scriptArgs
    }
} catch {
    Write-Host "❌ Error running script: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Trying alternative method with dotnet run..." -ForegroundColor Yellow
    
    # Alternative: Create a simple console app
    $programCs = @"
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;

namespace DatabaseExplorer
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            $explorerScript
        }
    }
}
"@
    
    # This would require creating a full project, so let's use a simpler approach
    Write-Host ""
    Write-Host "Please run the following command manually:" -ForegroundColor Yellow
    Write-Host "  dotnet run --project . -- explore-db" -ForegroundColor Gray
}

# Cleanup
if (Test-Path $scriptFile) {
    Remove-Item $scriptFile -Force
}

Write-Host ""
Write-Host "✅ Done!" -ForegroundColor Green

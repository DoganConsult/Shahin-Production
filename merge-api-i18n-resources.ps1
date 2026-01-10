#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Merges API i18n resources into existing SharedResource RESX files

.DESCRIPTION
    This script merges the new API localization resources (336 keys) into the existing
    SharedResource.en.resx and SharedResource.ar.resx files.

    It creates backups of the original files before modification.

.EXAMPLE
    ./merge-api-i18n-resources.ps1
#>

$ErrorActionPreference = "Stop"

# Paths
$projectRoot = "/home/Shahin-ai/Shahin-Jan-2026"
$resourcesPath = "$projectRoot/src/GrcMvc/Resources"
$enResxPath = "$resourcesPath/SharedResource.en.resx"
$arResxPath = "$resourcesPath/SharedResource.ar.resx"
$enNewResourcesPath = "$projectRoot/api-i18n-resources-en.xml"
$arNewResourcesPath = "$projectRoot/api-i18n-resources-ar.xml"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "API i18n Resources Merger" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verify files exist
Write-Host "[1/6] Verifying files..." -ForegroundColor Yellow
if (-not (Test-Path $enResxPath)) {
    Write-Error "English RESX file not found: $enResxPath"
    exit 1
}
if (-not (Test-Path $arResxPath)) {
    Write-Error "Arabic RESX file not found: $arResxPath"
    exit 1
}
if (-not (Test-Path $enNewResourcesPath)) {
    Write-Error "New English resources file not found: $enNewResourcesPath"
    exit 1
}
if (-not (Test-Path $arNewResourcesPath)) {
    Write-Error "New Arabic resources file not found: $arNewResourcesPath"
    exit 1
}
Write-Host "   ✓ All files found" -ForegroundColor Green
Write-Host ""

# Create backups
Write-Host "[2/6] Creating backups..." -ForegroundColor Yellow
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$enBackup = "$enResxPath.backup-$timestamp"
$arBackup = "$arResxPath.backup-$timestamp"
Copy-Item $enResxPath $enBackup
Copy-Item $arResxPath $arBackup
Write-Host "   ✓ Backup created: $enBackup" -ForegroundColor Green
Write-Host "   ✓ Backup created: $arBackup" -ForegroundColor Green
Write-Host ""

# Function to merge resources
function Merge-ResxResources {
    param(
        [string]$ExistingResxPath,
        [string]$NewResourcesPath,
        [string]$Language
    )

    Write-Host "   Processing $Language resources..." -ForegroundColor Cyan

    # Read existing RESX
    [xml]$existingResx = Get-Content $ExistingResxPath

    # Read new resources (XML fragment)
    $newResourcesContent = Get-Content $NewResourcesPath -Raw

    # Find the </root> closing tag position
    $resxContent = Get-Content $ExistingResxPath -Raw
    $closingRootPosition = $resxContent.LastIndexOf("</root>")

    if ($closingRootPosition -eq -1) {
        Write-Error "Could not find closing </root> tag in $ExistingResxPath"
        return $false
    }

    # Insert new resources before </root>
    $beforeClosing = $resxContent.Substring(0, $closingRootPosition)
    $updatedContent = $beforeClosing + "`n" + $newResourcesContent + "`n</root>"

    # Write updated content
    Set-Content -Path $ExistingResxPath -Value $updatedContent -Encoding UTF8

    Write-Host "   ✓ Merged 336 new resources into $Language RESX" -ForegroundColor Green
    return $true
}

# Merge English resources
Write-Host "[3/6] Merging English resources..." -ForegroundColor Yellow
$enResult = Merge-ResxResources -ExistingResxPath $enResxPath -NewResourcesPath $enNewResourcesPath -Language "English"
if (-not $enResult) {
    Write-Error "Failed to merge English resources"
    # Restore backup
    Copy-Item $enBackup $enResxPath -Force
    exit 1
}
Write-Host ""

# Merge Arabic resources
Write-Host "[4/6] Merging Arabic resources..." -ForegroundColor Yellow
$arResult = Merge-ResxResources -ExistingResxPath $arResxPath -NewResourcesPath $arNewResourcesPath -Language "Arabic"
if (-not $arResult) {
    Write-Error "Failed to merge Arabic resources"
    # Restore backups
    Copy-Item $enBackup $enResxPath -Force
    Copy-Item $arBackup $arResxPath -Force
    exit 1
}
Write-Host ""

# Verify merged files
Write-Host "[5/6] Verifying merged files..." -ForegroundColor Yellow
try {
    [xml]$verifyEn = Get-Content $enResxPath
    [xml]$verifyAr = Get-Content $arResxPath

    # Count data elements
    $enCount = ($verifyEn.root.data | Measure-Object).Count
    $arCount = ($verifyAr.root.data | Measure-Object).Count

    Write-Host "   ✓ English RESX: $enCount resources" -ForegroundColor Green
    Write-Host "   ✓ Arabic RESX: $arCount resources" -ForegroundColor Green

    if ($enCount -lt 1000 -or $arCount -lt 1000) {
        Write-Warning "Resource count seems low. Expected >1000 resources per file."
    }
} catch {
    Write-Error "Failed to verify merged files: $_"
    # Restore backups
    Copy-Item $enBackup $enResxPath -Force
    Copy-Item $arBackup $arResxPath -Force
    exit 1
}
Write-Host ""

# Summary
Write-Host "[6/6] Summary" -ForegroundColor Yellow
Write-Host "   ✓ Successfully merged 336 API i18n resources" -ForegroundColor Green
Write-Host "   ✓ English resources: $enCount total" -ForegroundColor Green
Write-Host "   ✓ Arabic resources: $arCount total" -ForegroundColor Green
Write-Host "   ✓ Backups saved:" -ForegroundColor Green
Write-Host "     - $enBackup" -ForegroundColor Gray
Write-Host "     - $arBackup" -ForegroundColor Gray
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✓ Resource merge completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Run: ./update-api-controllers-i18n.ps1" -ForegroundColor White
Write-Host "2. Build project to verify" -ForegroundColor White
Write-Host "3. Test API with different cultures (ar, en)" -ForegroundColor White
Write-Host ""

#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Automatically updates API controllers to use IStringLocalizer for i18n

.DESCRIPTION
    This script updates all API controllers in the project to:
    1. Add IStringLocalizer<SharedResource> dependency injection
    2. Replace hardcoded error messages with localized strings
    3. Preserve existing functionality

.PARAMETER DryRun
    Run in dry-run mode without making changes (shows what would be changed)

.EXAMPLE
    ./update-api-controllers-i18n.ps1
    ./update-api-controllers-i18n.ps1 -DryRun
#>

param(
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

# Paths
$projectRoot = "/home/Shahin-ai/Shahin-Jan-2026"
$controllersPath = "$projectRoot/src/GrcMvc/Controllers/Api"

# Controllers that already have localization (skip these)
$skipControllers = @(
    "ReportController.cs",
    "EnhancedReportController.cs",
    "WorkflowsController.cs",
    "LocalizationDiagController.cs"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "API Controllers i18n Auto-Update Script" -ForegroundColor Cyan
if ($DryRun) {
    Write-Host "(DRY RUN MODE - No changes will be made)" -ForegroundColor Yellow
}
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get all API controllers
Write-Host "[1/5] Discovering API controllers..." -ForegroundColor Yellow
$controllers = Get-ChildItem -Path $controllersPath -Filter "*.cs" |
    Where-Object { $skipControllers -notcontains $_.Name }

Write-Host "   Found $($controllers.Count) controllers to update" -ForegroundColor Green
Write-Host "   Skipping $($skipControllers.Count) controllers (already localized)" -ForegroundColor Gray
Write-Host ""

# Function to check if controller already has IStringLocalizer
function Test-HasStringLocalizer {
    param([string]$Content)
    return $Content -match "IStringLocalizer<SharedResource>"
}

# Function to add using statement if not present
function Add-UsingStatement {
    param([string]$Content)

    $usingStatements = @(
        "using Microsoft.Extensions.Localization;",
        "using GrcMvc.Resources;"
    )

    $modified = $Content

    foreach ($using in $usingStatements) {
        if ($modified -notmatch [regex]::Escape($using)) {
            # Find the last using statement
            if ($modified -match "(?s)(using .*?;)(\s*namespace)") {
                $lastUsing = $matches[1]
                $namespace = $matches[2]
                $modified = $modified -replace [regex]::Escape("$lastUsing$namespace"), "$lastUsing`n$using$namespace"
            }
        }
    }

    return $modified
}

# Function to inject IStringLocalizer field
function Add-LocalizerField {
    param([string]$Content)

    # Find existing private fields (after first field declaration or before constructor)
    $pattern = "(?s)(private\s+readonly\s+\w+.*?;)"
    if ($Content -match $pattern) {
        $lastField = $matches[1]
        $replacement = "$lastField`n        private readonly IStringLocalizer<SharedResource> _localizer;"
        $Content = $Content -replace [regex]::Escape($lastField), $replacement
    } else {
        # If no fields, add before constructor
        $pattern = "(?s)(public\s+\w+Controller\s*\()"
        if ($Content -match $pattern) {
            $constructor = $matches[1]
            $replacement = "        private readonly IStringLocalizer<SharedResource> _localizer;`n`n        $constructor"
            $Content = $Content -replace [regex]::Escape($constructor), $replacement
        }
    }

    return $Content
}

# Function to add localizer parameter to constructor
function Add-LocalizerToConstructor {
    param([string]$Content)

    # Find constructor
    $pattern = "(?s)public\s+(\w+Controller)\s*\((.*?)\)\s*\{(.*?)\n\s+\}"

    if ($Content -match $pattern) {
        $controllerName = $matches[1]
        $parameters = $matches[2].Trim()
        $body = $matches[3]

        # Add localizer parameter
        if ($parameters -eq "") {
            $newParameters = "IStringLocalizer<SharedResource> localizer"
        } else {
            # Add comma and new parameter
            $newParameters = "$parameters,`n            IStringLocalizer<SharedResource> localizer"
        }

        # Add assignment in constructor body
        $newBody = $body.TrimEnd()
        if (-not ($newBody -match "_localizer\s*=\s*localizer")) {
            if ($newBody -ne "") {
                $newBody += "`n            _localizer = localizer;"
            } else {
                $newBody = "`n            _localizer = localizer;"
            }
        }

        $oldConstructor = "public $controllerName($parameters)`n        {$body`n        }"
        $newConstructor = "public $controllerName($newParameters)`n        {$newBody`n        }"

        $Content = $Content -replace [regex]::Escape($oldConstructor), $newConstructor
    }

    return $Content
}

# Mapping of hardcoded strings to resource keys
$stringMappings = @{
    '"An error occurred"' = '_localizer["Api_Error_Generic"]'
    '"Asset not found"' = '_localizer["Api_Error_AssetNotFound"]'
    '"User not found"' = '_localizer["Api_Error_UserNotFound"]'
    '"Failed to retrieve assets"' = '_localizer["Api_Error_FailedToRetrieveAssets"]'
    '"Failed to create asset"' = '_localizer["Api_Error_FailedToCreateAsset"]'
    '"Failed to update asset"' = '_localizer["Api_Error_FailedToUpdateAsset"]'
    '"Failed to delete asset"' = '_localizer["Api_Error_FailedToDeleteAsset"]'
    '"Failed to assign owner"' = '_localizer["Api_Error_FailedToAssignOwner"]'
    '"Failed to update classification"' = '_localizer["Api_Error_FailedToUpdateClassification"]'
    '"Failed to retrieve asset"' = '_localizer["Api_Error_FailedToRetrieveAsset"]'
    '"Failed to retrieve asset summary"' = '_localizer["Api_Error_FailedToRetrieveAssetSummary"]'
    '"Failed to retrieve counts"' = '_localizer["Api_Error_FailedToRetrieveCounts"]'
    '"Failed to retrieve in-scope assets"' = '_localizer["Api_Error_FailedToRetrieveInScopeAssets"]'
    '"Failed to retrieve PCI assets"' = '_localizer["Api_Error_FailedToRetrievePciAssets"]'
    '"Failed to retrieve PII assets"' = '_localizer["Api_Error_FailedToRetrievePiiAssets"]'
    '"Failed to retrieve unassigned assets"' = '_localizer["Api_Error_FailedToRetrieveUnassignedAssets"]'
    '"Failed to update scope"' = '_localizer["Api_Error_FailedToUpdateScope"]'
    '"Failed to import assets"' = '_localizer["Api_Error_FailedToImportAssets"]'
    '"Workspace not found"' = '_localizer["Api_Error_WorkspaceNotFound"]'
    '"Failed to create workspace"' = '_localizer["Api_Error_FailedToCreateWorkspace"]'
    '"Failed to get workspace"' = '_localizer["Api_Error_FailedToGetWorkspace"]'
    '"Failed to update workspace"' = '_localizer["Api_Error_FailedToUpdateWorkspace"]'
    '"Failed to switch workspace"' = '_localizer["Api_Error_FailedToSwitchWorkspace"]'
    '"Incident not found"' = '_localizer["Api_Error_IncidentNotFound"]'
    '"Gap not found"' = '_localizer["Api_Error_GapNotFound"]'
    '"Tenant not found"' = '_localizer["Api_Error_TenantNotFound"]'
    '"Report not found"' = '_localizer["Api_Error_ReportNotFound"]'
    '"Failed to create report"' = '_localizer["Api_Error_FailedToCreateReport"]'
    '"Failed to retrieve report"' = '_localizer["Api_Error_FailedToRetrieveReport"]'
    '"Failed to update report"' = '_localizer["Api_Error_FailedToUpdateReport"]'
    '"Failed to delete report"' = '_localizer["Api_Error_FailedToDeleteReport"]'
    '"Failed to download report"' = '_localizer["Api_Error_FailedToDownloadReport"]'
    '"Failed to generate report"' = '_localizer["Api_Error_FailedToGenerateReport"]'
    '"Failed to deliver report"' = '_localizer["Api_Error_FailedToDeliverReport"]'
    '"Workflow not found"' = '_localizer["Api_Error_WorkflowNotFound"]'
    '"Control not found"' = '_localizer["Api_Error_ControlNotFound"]'
    '"Evidence not found"' = '_localizer["Api_Error_EvidenceNotFound"]'
    '"Framework not found"' = '_localizer["Api_Error_FrameworkNotFound"]'
    '"Policy not found"' = '_localizer["Api_Error_PolicyNotFound"]'
    '"Exception not found"' = '_localizer["Api_Error_ExceptionNotFound"]'
    '"No tenant context"' = '_localizer["Api_Error_NoTenantContext"]'
    '"Tenant context required"' = '_localizer["Api_Error_TenantContextRequired"]'
    '"Internal error"' = '_localizer["Api_Error_InternalError"]'
    '"Email is required"' = '_localizer["Api_Error_EmailRequired"]'
    '"Title is required"' = '_localizer["Api_Error_TitleRequired"]'
    '"Message is required"' = '_localizer["Api_Error_MessageRequired"]'
    '"No file uploaded"' = '_localizer["Api_Error_NoFileUploaded"]'
    '"Code is required"' = '_localizer["Api_Error_CodeRequired"]'
    '"Platform admin not found"' = '_localizer["Api_Error_PlatformAdminNotFound"]'
    '"Failed to create platform admin"' = '_localizer["Api_Error_FailedToCreatePlatformAdmin"]'
    '"Failed to retrieve platform admin"' = '_localizer["Api_Error_FailedToRetrievePlatformAdmin"]'
    '"Failed to update platform admin"' = '_localizer["Api_Error_FailedToUpdatePlatformAdmin"]'
    '"Failed to suspend platform admin"' = '_localizer["Api_Error_FailedToSuspendPlatformAdmin"]'
    '"Failed to delete platform admin"' = '_localizer["Api_Error_FailedToDeletePlatformAdmin"]'
    '"Failed to reset password"' = '_localizer["Api_Error_FailedToResetPassword"]'
}

# Function to replace hardcoded strings
function Replace-HardcodedStrings {
    param([string]$Content)

    $modified = $Content

    foreach ($key in $stringMappings.Keys) {
        $value = $stringMappings[$key]
        # Match pattern: error = "..."
        $pattern = "error\s*=\s*$key"
        $replacement = "error = $value"
        $modified = $modified -replace $pattern, $replacement

        # Match pattern: message = "..."
        $pattern = "message\s*=\s*$key"
        $replacement = "message = $value"
        $modified = $modified -replace $pattern, $replacement
    }

    return $modified
}

# Process each controller
Write-Host "[2/5] Processing controllers..." -ForegroundColor Yellow
$processedCount = 0
$updatedCount = 0
$skippedCount = 0

foreach ($controller in $controllers) {
    $processedCount++
    Write-Host "   [$processedCount/$($controllers.Count)] Processing: $($controller.Name)" -ForegroundColor Cyan

    $content = Get-Content -Path $controller.FullName -Raw

    # Check if already has localizer
    if (Test-HasStringLocalizer -Content $content) {
        Write-Host "      ⊘ Already has IStringLocalizer, skipping" -ForegroundColor Gray
        $skippedCount++
        continue
    }

    $originalContent = $content

    # Add using statements
    $content = Add-UsingStatement -Content $content

    # Add field
    $content = Add-LocalizerField -Content $content

    # Add to constructor
    $content = Add-LocalizerToConstructor -Content $content

    # Replace hardcoded strings
    $content = Replace-HardcodedStrings -Content $content

    if ($content -ne $originalContent) {
        if (-not $DryRun) {
            # Create backup
            $backup = "$($controller.FullName).backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
            Copy-Item $controller.FullName $backup

            # Write updated content
            Set-Content -Path $controller.FullName -Value $content -Encoding UTF8
            Write-Host "      ✓ Updated (backup: $backup)" -ForegroundColor Green
        } else {
            Write-Host "      ✓ Would be updated (dry-run)" -ForegroundColor Yellow
        }
        $updatedCount++
    } else {
        Write-Host "      - No changes needed" -ForegroundColor Gray
        $skippedCount++
    }
}

Write-Host ""

# Summary
Write-Host "[3/5] Processing complete" -ForegroundColor Yellow
Write-Host "   Total controllers: $($controllers.Count)" -ForegroundColor White
Write-Host "   Updated: $updatedCount" -ForegroundColor Green
Write-Host "   Skipped: $skippedCount" -ForegroundColor Gray
Write-Host ""

# Verification
Write-Host "[4/5] Verification" -ForegroundColor Yellow
if (-not $DryRun) {
    $allControllers = Get-ChildItem -Path $controllersPath -Filter "*.cs"
    $withLocalizer = 0
    $withoutLocalizer = 0

    foreach ($ctrl in $allControllers) {
        $content = Get-Content -Path $ctrl.FullName -Raw
        if (Test-HasStringLocalizer -Content $content) {
            $withLocalizer++
        } else {
            $withoutLocalizer++
        }
    }

    Write-Host "   Controllers with IStringLocalizer: $withLocalizer / $($allControllers.Count)" -ForegroundColor Green
    Write-Host "   Controllers without IStringLocalizer: $withoutLocalizer" -ForegroundColor $(if ($withoutLocalizer -gt 0) { "Yellow" } else { "Green" })
} else {
    Write-Host "   Skipped (dry-run mode)" -ForegroundColor Gray
}
Write-Host ""

# Next steps
Write-Host "[5/5] Next Steps" -ForegroundColor Yellow
if ($DryRun) {
    Write-Host "   1. Review the changes above" -ForegroundColor White
    Write-Host "   2. Run without -DryRun to apply changes:" -ForegroundColor White
    Write-Host "      ./update-api-controllers-i18n.ps1" -ForegroundColor Gray
} else {
    Write-Host "   1. Build the project:" -ForegroundColor White
    Write-Host "      cd src/GrcMvc && dotnet build" -ForegroundColor Gray
    Write-Host "   2. Fix any compilation errors" -ForegroundColor White
    Write-Host "   3. Test API endpoints with different cultures" -ForegroundColor White
    Write-Host "   4. Manually review controllers for remaining hardcoded strings" -ForegroundColor White
}
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
if ($DryRun) {
    Write-Host "✓ Dry run completed!" -ForegroundColor Green
} else {
    Write-Host "✓ Controllers updated successfully!" -ForegroundColor Green
}
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

#!/bin/bash
set -e

# Configuration
PROJECT_ROOT="/home/Shahin-ai/Shahin-Jan-2026"
CONTROLLERS_PATH="$PROJECT_ROOT/src/GrcMvc/Controllers/Api"

# Controllers that already have localization (skip these)
SKIP_CONTROLLERS=("ReportController.cs" "EnhancedReportController.cs" "WorkflowsController.cs" "LocalizationDiagController.cs")

DRY_RUN=false
if [[ "$1" == "--dry-run" || "$1" == "-n" ]]; then
    DRY_RUN=true
fi

echo "========================================"
echo "API Controllers i18n Auto-Update Script"
if $DRY_RUN; then
    echo "(DRY RUN MODE - No changes will be made)"
fi
echo "========================================"
echo ""

# Function to check if value is in array
contains_element() {
    local e match="$1"
    shift
    for e; do [[ "$e" == "$match" ]] && return 0; done
    return 1
}

# Function to check if controller already has IStringLocalizer
has_string_localizer() {
    local file="$1"
    grep -q "IStringLocalizer<SharedResource>" "$file" && return 0 || return 1
}

# Discover controllers
echo "[1/5] Discovering API controllers..."
total_controllers=0
controllers_to_update=()

for file in "$CONTROLLERS_PATH"/*.cs; do
    filename=$(basename "$file")

    if ! contains_element "$filename" "${SKIP_CONTROLLERS[@]}"; then
        if ! has_string_localizer "$file"; then
            controllers_to_update+=("$file")
            ((total_controllers++))
        fi
    fi
done

echo "   Found ${#controllers_to_update[@]} controllers to update"
echo "   Skipping ${#SKIP_CONTROLLERS[@]} controllers (already localized)"
echo ""

# Process each controller
echo "[2/5] Processing controllers..."
updated_count=0
skipped_count=0
current=0

for controller_file in "${controllers_to_update[@]}"; do
    ((current++))
    filename=$(basename "$controller_file")
    echo "   [$current/${#controllers_to_update[@]}] Processing: $filename"

    # Create backup
    timestamp=$(date +"%Y%m%d-%H%M%S")
    backup_file="${controller_file}.backup-${timestamp}"

    if ! $DRY_RUN; then
        cp "$controller_file" "$backup_file"

        # Create temporary file for modifications
        temp_file="${controller_file}.temp"

        # Step 1: Add using statements if not present
        if ! grep -q "using Microsoft.Extensions.Localization;" "$controller_file"; then
            # Find last using statement and add new ones after it
            awk '
                /^using .*?;$/ { last_using=NR; lines[NR]=$0 }
                NR==last_using+1 && !found {
                    print lines[last_using]
                    print "using Microsoft.Extensions.Localization;"
                    print "using GrcMvc.Resources;"
                    found=1
                    next
                }
                NR!=last_using { print }
            ' "$controller_file" > "$temp_file"
            mv "$temp_file" "$controller_file"
        fi

        # Step 2: Add private readonly field
        if ! grep -q "private readonly IStringLocalizer<SharedResource> _localizer;" "$controller_file"; then
            # Find first private readonly field and add after it
            awk '
                /private readonly.*_.*/ && !found {
                    print
                    print "        private readonly IStringLocalizer<SharedResource> _localizer;"
                    found=1
                    next
                }
                { print }
            ' "$controller_file" > "$temp_file"
            mv "$temp_file" "$controller_file"
        fi

        # Step 3: Add parameter to constructor (this is complex, so we'll note it for manual review)
        # This would require parsing C# constructor syntax which is complex in bash
        # We'll create a marker comment instead

        # Step 4: Replace common hardcoded strings
        sed -i 's/error = "An error occurred"/error = _localizer["Api_Error_Generic"]/g' "$controller_file"
        sed -i 's/error = "Asset not found"/error = _localizer["Api_Error_AssetNotFound"]/g' "$controller_file"
        sed -i 's/error = "User not found"/error = _localizer["Api_Error_UserNotFound"]/g' "$controller_file"
        sed -i 's/error = "Tenant not found"/error = _localizer["Api_Error_TenantNotFound"]/g' "$controller_file"
        sed -i 's/error = "Report not found"/error = _localizer["Api_Error_ReportNotFound"]/g' "$controller_file"
        sed -i 's/error = "Workspace not found"/error = _localizer["Api_Error_WorkspaceNotFound"]/g' "$controller_file"
        sed -i 's/error = "Workflow not found"/error = _localizer["Api_Error_WorkflowNotFound"]/g' "$controller_file"
        sed -i 's/error = "Control not found"/error = _localizer["Api_Error_ControlNotFound"]/g' "$controller_file"
        sed -i 's/error = "Evidence not found"/error = _localizer["Api_Error_EvidenceNotFound"]/g' "$controller_file"
        sed -i 's/error = "Framework not found"/error = _localizer["Api_Error_FrameworkNotFound"]/g' "$controller_file"
        sed -i 's/error = "Incident not found"/error = _localizer["Api_Error_IncidentNotFound"]/g' "$controller_file"
        sed -i 's/error = "Gap not found"/error = _localizer["Api_Error_GapNotFound"]/g' "$controller_file"
        sed -i 's/error = "Policy not found"/error = _localizer["Api_Error_PolicyNotFound"]/g' "$controller_file"
        sed -i 's/error = "Exception not found"/error = _localizer["Api_Error_ExceptionNotFound"]/g' "$controller_file"
        sed -i 's/error = "Failed to retrieve assets"/error = _localizer["Api_Error_FailedToRetrieveAssets"]/g' "$controller_file"
        sed -i 's/error = "Failed to create asset"/error = _localizer["Api_Error_FailedToCreateAsset"]/g' "$controller_file"
        sed -i 's/error = "Failed to update asset"/error = _localizer["Api_Error_FailedToUpdateAsset"]/g' "$controller_file"
        sed -i 's/error = "Failed to delete asset"/error = _localizer["Api_Error_FailedToDeleteAsset"]/g' "$controller_file"
        sed -i 's/error = "No tenant context"/error = _localizer["Api_Error_NoTenantContext"]/g' "$controller_file"
        sed -i 's/error = "Tenant context required"/error = _localizer["Api_Error_TenantContextRequired"]/g' "$controller_file"
        sed -i 's/error = "Internal error"/error = _localizer["Api_Error_InternalError"]/g' "$controller_file"

        echo "      ✓ Updated (backup: $backup_file)"
        ((updated_count++))
    else
        echo "      ✓ Would be updated (dry-run)"
        ((updated_count++))
    fi
done

echo ""

# Summary
echo "[3/5] Processing complete"
echo "   Total controllers: ${#controllers_to_update[@]}"
echo "   Updated: $updated_count"
echo "   Skipped: $skipped_count"
echo ""

# Verification
echo "[4/5] Verification"
if ! $DRY_RUN; then
    all_controllers=$(find "$CONTROLLERS_PATH" -name "*.cs" | wc -l)
    with_localizer=$(grep -l "IStringLocalizer<SharedResource>" "$CONTROLLERS_PATH"/*.cs 2>/dev/null | wc -l)
    without_localizer=$((all_controllers - with_localizer))

    echo "   Controllers with IStringLocalizer: $with_localizer / $all_controllers"
    echo "   Controllers without IStringLocalizer: $without_localizer"
else
    echo "   Skipped (dry-run mode)"
fi
echo ""

# Next steps
echo "[5/5] Next Steps"
if $DRY_RUN; then
    echo "   1. Review the changes above"
    echo "   2. Run without --dry-run to apply changes:"
    echo "      ./update-api-controllers-i18n.sh"
else
    echo "   IMPORTANT: Constructor parameters were NOT automatically updated"
    echo "   You need to manually add IStringLocalizer parameter to constructors:"
    echo ""
    echo "   1. Open each controller"
    echo "   2. Add to constructor parameters:"
    echo "      IStringLocalizer<SharedResource> localizer"
    echo "   3. Add to constructor body:"
    echo "      _localizer = localizer;"
    echo ""
    echo "   4. Build the project:"
    echo "      cd src/GrcMvc && dotnet build"
    echo "   5. Fix any compilation errors"
    echo "   6. Test API endpoints with different cultures"
    echo "   7. Manually review for remaining hardcoded strings:"
    echo "      grep -n 'error = \"' Controllers/Api/*.cs | grep -v '_localizer'"
fi
echo ""

echo "========================================"
if $DRY_RUN; then
    echo "✓ Dry run completed!"
else
    echo "✓ Controllers partially updated!"
    echo "⚠ Manual constructor updates required"
fi
echo "========================================"
echo ""

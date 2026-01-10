#!/bin/bash

# Simple script to add IStringLocalizer to controllers
PROJECT_ROOT="/home/Shahin-ai/Shahin-Jan-2026"
CONTROLLERS_PATH="$PROJECT_ROOT/src/GrcMvc/Controllers/Api"

echo "=========================================="
echo "Adding IStringLocalizer to API Controllers"
echo "=========================================="
echo ""

# Skip controllers that already have it
SKIP=("ReportController.cs" "EnhancedReportController.cs" "WorkflowsController.cs" "LocalizationDiagController.cs")

updated=0
skipped=0

for controller in "$CONTROLLERS_PATH"/*.cs; do
    filename=$(basename "$controller")

    # Skip if in skip list
    skip_this=false
    for skip_file in "${SKIP[@]}"; do
        if [[ "$filename" == "$skip_file" ]]; then
            skip_this=true
            break
        fi
    done

    if $skip_this; then
        echo "⊘ Skipping $filename (already has i18n)"
        ((skipped++))
        continue
    fi

    # Check if already has IStringLocalizer
    if grep -q "IStringLocalizer<SharedResource>" "$controller"; then
        echo "⊘ Skipping $filename (already has IStringLocalizer)"
        ((skipped++))
        continue
    fi

    echo "✓ Processing $filename..."

    # Create backup
    cp "$controller" "${controller}.backup-$(date +%Y%m%d-%H%M%S)"

    # Add using statements if missing
    if ! grep -q "using Microsoft.Extensions.Localization;" "$controller"; then
        # Find namespace line and insert before it
        sed -i '/^namespace/i using Microsoft.Extensions.Localization;\nusing GrcMvc.Resources;' "$controller"
    fi

    # Add field after first private readonly field
    if ! grep -q "_localizer" "$controller"; then
        sed -i '0,/private readonly.*_;/s//&\n        private readonly IStringLocalizer<SharedResource> _localizer;/' "$controller"
    fi

    ((updated++))
done

echo ""
echo "Summary:"
echo "  Updated: $updated"
echo "  Skipped: $skipped"
echo ""
echo "NEXT STEPS (MANUAL):"
echo "1. For each updated controller, add to constructor:"
echo "   - Parameter: IStringLocalizer<SharedResource> localizer"
echo "   - Assignment: _localizer = localizer;"
echo ""
echo "2. Replace hardcoded strings with: _localizer[\"ResourceKey\"]"
echo ""
echo "3. Build project: cd src/GrcMvc && dotnet build"
echo ""

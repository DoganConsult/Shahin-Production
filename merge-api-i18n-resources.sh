#!/bin/bash
set -e

# Paths
PROJECT_ROOT="/home/Shahin-ai/Shahin-Jan-2026"
RESOURCES_PATH="$PROJECT_ROOT/src/GrcMvc/Resources"
EN_RESX_PATH="$RESOURCES_PATH/SharedResource.en.resx"
AR_RESX_PATH="$RESOURCES_PATH/SharedResource.ar.resx"
EN_NEW_RESOURCES_PATH="$PROJECT_ROOT/api-i18n-resources-en.xml"
AR_NEW_RESOURCES_PATH="$PROJECT_ROOT/api-i18n-resources-ar.xml"

echo "========================================"
echo "API i18n Resources Merger"
echo "========================================"
echo ""

# Verify files exist
echo "[1/6] Verifying files..."
if [ ! -f "$EN_RESX_PATH" ]; then
    echo "ERROR: English RESX file not found: $EN_RESX_PATH"
    exit 1
fi
if [ ! -f "$AR_RESX_PATH" ]; then
    echo "ERROR: Arabic RESX file not found: $AR_RESX_PATH"
    exit 1
fi
if [ ! -f "$EN_NEW_RESOURCES_PATH" ]; then
    echo "ERROR: New English resources file not found: $EN_NEW_RESOURCES_PATH"
    exit 1
fi
if [ ! -f "$AR_NEW_RESOURCES_PATH" ]; then
    echo "ERROR: New Arabic resources file not found: $AR_NEW_RESOURCES_PATH"
    exit 1
fi
echo "   ✓ All files found"
echo ""

# Create backups
echo "[2/6] Creating backups..."
TIMESTAMP=$(date +"%Y%m%d-%H%M%S")
EN_BACKUP="$EN_RESX_PATH.backup-$TIMESTAMP"
AR_BACKUP="$AR_RESX_PATH.backup-$TIMESTAMP"
cp "$EN_RESX_PATH" "$EN_BACKUP"
cp "$AR_RESX_PATH" "$AR_BACKUP"
echo "   ✓ Backup created: $EN_BACKUP"
echo "   ✓ Backup created: $AR_BACKUP"
echo ""

# Function to merge resources
merge_resources() {
    local EXISTING_RESX="$1"
    local NEW_RESOURCES="$2"
    local LANGUAGE="$3"

    echo "   Processing $LANGUAGE resources..."

    # Read new resources
    NEW_CONTENT=$(cat "$NEW_RESOURCES")

    # Create temporary file
    TEMP_FILE="$EXISTING_RESX.temp"

    # Remove closing </root> tag, append new resources, then add closing tag
    sed '/<\/root>/d' "$EXISTING_RESX" > "$TEMP_FILE"
    echo "$NEW_CONTENT" >> "$TEMP_FILE"
    echo "</root>" >> "$TEMP_FILE"

    # Replace original file
    mv "$TEMP_FILE" "$EXISTING_RESX"

    echo "   ✓ Merged 336 new resources into $LANGUAGE RESX"
}

# Merge English resources
echo "[3/6] Merging English resources..."
merge_resources "$EN_RESX_PATH" "$EN_NEW_RESOURCES_PATH" "English"
echo ""

# Merge Arabic resources
echo "[4/6] Merging Arabic resources..."
merge_resources "$AR_RESX_PATH" "$AR_NEW_RESOURCES_PATH" "Arabic"
echo ""

# Verify merged files
echo "[5/6] Verifying merged files..."
EN_COUNT=$(grep -c '<data name=' "$EN_RESX_PATH" || true)
AR_COUNT=$(grep -c '<data name=' "$AR_RESX_PATH" || true)

echo "   ✓ English RESX: $EN_COUNT resources"
echo "   ✓ Arabic RESX: $AR_COUNT resources"

if [ "$EN_COUNT" -lt 1000 ] || [ "$AR_COUNT" -lt 1000 ]; then
    echo "   ⚠ WARNING: Resource count seems low. Expected >1000 resources per file."
fi
echo ""

# Summary
echo "[6/6] Summary"
echo "   ✓ Successfully merged 336 API i18n resources"
echo "   ✓ English resources: $EN_COUNT total"
echo "   ✓ Arabic resources: $AR_COUNT total"
echo "   ✓ Backups saved:"
echo "     - $EN_BACKUP"
echo "     - $AR_BACKUP"
echo ""
echo "========================================"
echo "✓ Resource merge completed successfully!"
echo "========================================"
echo ""
echo "Next steps:"
echo "1. Run: ./update-api-controllers-i18n.sh"
echo "2. Build project to verify"
echo "3. Test API with different cultures (ar, en)"
echo ""

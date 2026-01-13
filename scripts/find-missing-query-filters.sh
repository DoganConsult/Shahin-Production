#!/bin/bash
# Find entities with TenantId property but missing query filters

set -e

MODELS_DIR="/home/user/Shahin-Jan-2026/src/GrcMvc/Models/Entities"
DBCONTEXT="/home/user/Shahin-Jan-2026/src/GrcMvc/Data/GrcDbContext.cs"

echo "🔍 Finding entities missing query filters..."
echo "==========================================="
echo ""

# Get all DbSet entity names from GrcDbContext
echo "📋 Extracting entity names from DbContext..."
ENTITIES=$(grep "public DbSet" "$DBCONTEXT" | sed 's/.*DbSet<\([^>]*\)>.*/\1/' | sort)

echo "✅ Found $(echo "$ENTITIES" | wc -l) total entities"
echo ""

# Find entities with TenantId property
echo "🔍 Checking which entities have TenantId property..."
ENTITIES_WITH_TENANT_ID=()

for entity in $ENTITIES; do
    # Try to find the entity file
    ENTITY_FILE=$(find "$MODELS_DIR" -name "${entity}.cs" 2>/dev/null | head -1)

    if [ -n "$ENTITY_FILE" ] && [ -f "$ENTITY_FILE" ]; then
        # Check if entity has TenantId property
        if grep -q "TenantId" "$ENTITY_FILE" 2>/dev/null; then
            ENTITIES_WITH_TENANT_ID+=("$entity")
        fi
    fi
done

echo "✅ Found ${#ENTITIES_WITH_TENANT_ID[@]} entities with TenantId property"
echo ""

# Check which entities are missing query filters
echo "🔍 Finding entities missing query filters..."
MISSING_FILTERS=()

for entity in "${ENTITIES_WITH_TENANT_ID[@]}"; do
    if ! grep -q "Entity<$entity>.*HasQueryFilter" "$DBCONTEXT" 2>/dev/null; then
        MISSING_FILTERS+=("$entity")
    fi
done

echo "⚠️  Found ${#MISSING_FILTERS[@]} entities missing query filters"
echo ""

if [ ${#MISSING_FILTERS[@]} -gt 0 ]; then
    echo "📋 Entities missing query filters:"
    echo "=================================="
    for entity in "${MISSING_FILTERS[@]}"; do
        echo "  - $entity"
    done
else
    echo "✅ All entities with TenantId have query filters!"
fi

#!/bin/bash
# Railway SSH Test Commands
# Run these commands after connecting via Railway SSH

echo "========================================"
echo "Railway Database Connection Test"
echo "========================================"
echo ""

# Check DATABASE_URL
echo "Step 1: Checking DATABASE_URL..."
if [ -z "$DATABASE_URL" ]; then
    echo "  ❌ DATABASE_URL not set!"
    echo ""
    echo "  This should be set automatically by Railway."
    echo "  Check Railway Dashboard → Your Service → Variables"
    exit 1
else
    echo "  ✅ DATABASE_URL is set"
    # Mask password for display
    MASKED_URL=$(echo "$DATABASE_URL" | sed 's/:[^@]*@/:***@/')
    echo "  Format: $MASKED_URL"
fi
echo ""

# Test PostgreSQL connection
echo "Step 2: Testing PostgreSQL Connection..."
if command -v psql &> /dev/null; then
    psql "$DATABASE_URL" -c "SELECT version(), current_database(), current_user;" 2>&1
    if [ $? -eq 0 ]; then
        echo ""
        echo "  ✅ PostgreSQL connection successful!"
    else
        echo ""
        echo "  ❌ PostgreSQL connection failed!"
        exit 1
    fi
else
    echo "  ⚠️  psql not found. Testing with .NET application instead..."
fi
echo ""

# Test with .NET application (if available)
echo "Step 3: Testing with Application..."
if [ -f "/app/GrcMvc.dll" ] || [ -f "./GrcMvc.dll" ]; then
    echo "  Running application test..."
    dotnet GrcMvc.dll TestDb
elif [ -d "/app" ]; then
    echo "  Application found in /app"
    cd /app
    if [ -f "GrcMvc.dll" ]; then
        dotnet GrcMvc.dll TestDb
    elif [ -f "GrcMvc.csproj" ]; then
        dotnet run -- TestDb
    else
        echo "  ⚠️  Application files not found"
    fi
else
    echo "  ⚠️  Application directory not found"
fi
echo ""

# Show connection string details (parsed)
echo "Step 4: Connection String Details..."
if [ ! -z "$DATABASE_URL" ]; then
    # Parse DATABASE_URL
    # Format: postgresql://user:pass@host:port/database
    PROTOCOL=$(echo "$DATABASE_URL" | cut -d: -f1)
    REST=$(echo "$DATABASE_URL" | cut -d/ -f3)
    USER_PASS=$(echo "$REST" | cut -d@ -f1)
    HOST_PORT=$(echo "$REST" | cut -d@ -f2)
    HOST=$(echo "$HOST_PORT" | cut -d: -f1)
    PORT=$(echo "$HOST_PORT" | cut -d: -f2 | cut -d/ -f1)
    DATABASE=$(echo "$DATABASE_URL" | cut -d/ -f4)
    USER=$(echo "$USER_PASS" | cut -d: -f1)
    
    echo "  Protocol: $PROTOCOL"
    echo "  Host: $HOST"
    echo "  Port: $PORT"
    echo "  Database: $DATABASE"
    echo "  Username: $USER"
    echo ""
fi

echo "========================================"
echo "Test Complete"
echo "========================================"

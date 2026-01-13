#!/bin/bash
# ============================================
# Create GRC Databases Script
# ============================================
# Purpose: Create GrcMvcDb and GrcAuthDb databases
# Usage: ./scripts/create-databases.sh
# ============================================

set -e  # Exit on error

echo "============================================"
echo "GRC Database Creation Script"
echo "============================================"
echo ""

# Check if PostgreSQL is running (Docker or local)
if command -v docker &> /dev/null; then
    CONTAINER_NAME="grc-db"

    # Check if Docker container is running
    if docker ps --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
        echo "✓ PostgreSQL container '${CONTAINER_NAME}' is running"

        # Execute database creation commands
        echo ""
        echo "Creating GrcMvcDb database..."
        docker exec -i ${CONTAINER_NAME} psql -U postgres -c "
            SELECT 'Checking if GrcMvcDb exists...' AS status;
            SELECT CASE
                WHEN EXISTS (SELECT FROM pg_database WHERE datname = 'GrcMvcDb')
                THEN 'GrcMvcDb already exists'
                ELSE 'Creating GrcMvcDb...'
            END AS status;
        "

        docker exec -i ${CONTAINER_NAME} psql -U postgres -tc "SELECT 1 FROM pg_database WHERE datname = 'GrcMvcDb'" | grep -q 1 || \
        docker exec -i ${CONTAINER_NAME} psql -U postgres -c "
            CREATE DATABASE \"GrcMvcDb\"
                WITH OWNER = postgres
                ENCODING = 'UTF8'
                LC_COLLATE = 'en_US.utf8'
                LC_CTYPE = 'en_US.utf8'
                TEMPLATE = template0;
        " && echo "✓ GrcMvcDb created successfully" || echo "✓ GrcMvcDb already exists"

        echo ""
        echo "Creating GrcAuthDb database..."
        docker exec -i ${CONTAINER_NAME} psql -U postgres -c "
            SELECT 'Checking if GrcAuthDb exists...' AS status;
            SELECT CASE
                WHEN EXISTS (SELECT FROM pg_database WHERE datname = 'GrcAuthDb')
                THEN 'GrcAuthDb already exists'
                ELSE 'Creating GrcAuthDb...'
            END AS status;
        "

        docker exec -i ${CONTAINER_NAME} psql -U postgres -tc "SELECT 1 FROM pg_database WHERE datname = 'GrcAuthDb'" | grep -q 1 || \
        docker exec -i ${CONTAINER_NAME} psql -U postgres -c "
            CREATE DATABASE \"GrcAuthDb\"
                WITH OWNER = postgres
                ENCODING = 'UTF8'
                LC_COLLATE = 'en_US.utf8'
                LC_CTYPE = 'en_US.utf8'
                TEMPLATE = template0;
        " && echo "✓ GrcAuthDb created successfully" || echo "✓ GrcAuthDb already exists"

        echo ""
        echo "Running initialization script..."
        docker exec -i ${CONTAINER_NAME} psql -U postgres < scripts/init-db.sql

        echo ""
        echo "============================================"
        echo "Database Creation Complete!"
        echo "============================================"
        echo ""
        echo "Created databases:"
        echo "  ✓ GrcMvcDb (Main Application Database)"
        echo "  ✓ GrcAuthDb (Authentication Database)"
        echo ""
        echo "Listing all databases:"
        docker exec -i ${CONTAINER_NAME} psql -U postgres -c "\l"

    else
        echo "✗ PostgreSQL container '${CONTAINER_NAME}' is not running"
        echo ""
        echo "Please start the container first:"
        echo "  docker-compose up -d db"
        exit 1
    fi
else
    echo "✗ Docker is not installed or not in PATH"
    echo ""
    echo "If PostgreSQL is running locally, run:"
    echo "  psql -U postgres -c \"CREATE DATABASE GrcMvcDb;\""
    echo "  psql -U postgres -c \"CREATE DATABASE GrcAuthDb;\""
    echo "  psql -U postgres -f scripts/init-db.sql"
    exit 1
fi

echo ""
echo "Next steps:"
echo "  1. Update appsettings.json connection strings (already done)"
echo "  2. Run EF Core migrations:"
echo "     cd src/GrcMvc"
echo "     dotnet ef database update --context GrcDbContext"
echo "     dotnet ef database update --context GrcAuthDbContext"
echo "  3. Start the application:"
echo "     dotnet run"
echo ""
echo "============================================"

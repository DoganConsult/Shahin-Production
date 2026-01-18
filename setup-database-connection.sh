#!/bin/bash

# Database Connection Setup Script for Shahin AI GRC Platform
# This script helps configure database connections for Linux/Mac environments

echo "=================================================="
echo "  Shahin AI GRC - Database Connection Setup"
echo "=================================================="
echo ""

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Check if PostgreSQL is installed
echo -e "${YELLOW}Checking PostgreSQL installation...${NC}"
if command -v psql &> /dev/null; then
    echo -e "${GREEN}✅ PostgreSQL is installed${NC}"
    psql --version
else
    echo -e "${RED}❌ PostgreSQL is not installed${NC}"
    echo -e "${YELLOW}Please install PostgreSQL:${NC}"
    echo "  Ubuntu/Debian: sudo apt-get install postgresql postgresql-contrib"
    echo "  CentOS/RHEL:   sudo yum install postgresql-server postgresql-contrib"
    echo "  macOS:         brew install postgresql"
    echo ""
    read -p "Do you want to continue anyway? (y/n) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
fi

echo ""
echo "=================================================="
echo "  Database Configuration"
echo "=================================================="
echo ""

# Get database configuration
read -p "Database Host (default: localhost): " DB_HOST
DB_HOST=${DB_HOST:-localhost}

read -p "Database Port (default: 5432): " DB_PORT
DB_PORT=${DB_PORT:-5432}

read -p "Database Username (default: postgres): " DB_USER
DB_USER=${DB_USER:-postgres}

read -sp "Database Password: " DB_PASSWORD
echo ""

echo ""
echo "=================================================="
echo "  JWT Configuration"
echo "=================================================="
echo ""

# Generate JWT Secret
echo -e "${YELLOW}Generating secure JWT secret...${NC}"
JWT_SECRET=$(openssl rand -base64 48 | tr -d "=+/" | cut -c1-64)
echo -e "${GREEN}✅ JWT Secret generated (64 characters)${NC}"

echo ""
echo "=================================================="
echo "  SMTP Configuration (Optional)"
echo "=================================================="
echo ""

read -p "Do you want to configure SMTP for email? (y/n) " -n 1 -r CONFIGURE_SMTP
echo ""

SMTP_FROM_EMAIL=""
SMTP_USERNAME=""
SMTP_PASSWORD=""

if [[ $CONFIGURE_SMTP =~ ^[Yy]$ ]]; then
    read -p "SMTP From Email (e.g., info@shahin-ai.com): " SMTP_FROM_EMAIL
    read -p "SMTP Username (usually same as From Email): " SMTP_USERNAME
    read -sp "SMTP Password: " SMTP_PASSWORD
    echo ""
fi

echo ""
echo "=================================================="
echo "  Creating Configuration File"
echo "=================================================="
echo ""

# Build connection strings
DEFAULT_CONNECTION="Host=$DB_HOST;Port=$DB_PORT;Database=GrcMvcDb;Username=$DB_USER;Password=$DB_PASSWORD;Include Error Detail=true"
AUTH_CONNECTION="Host=$DB_HOST;Port=$DB_PORT;Database=GrcAuthDb;Username=$DB_USER;Password=$DB_PASSWORD;Include Error Detail=true"
HANGFIRE_CONNECTION="Host=$DB_HOST;Port=$DB_PORT;Database=HangfireDb;Username=$DB_USER;Password=$DB_PASSWORD;Include Error Detail=true"
REDIS_CONNECTION="localhost:6379,abortConnect=false"

# Create appsettings.Local.json
CONFIG_PATH="src/GrcMvc/appsettings.Local.json"

cat > "$CONFIG_PATH" <<EOF
{
  "ConnectionStrings": {
    "DefaultConnection": "$DEFAULT_CONNECTION",
    "GrcAuthDb": "$AUTH_CONNECTION",
    "Redis": "$REDIS_CONNECTION",
    "HangfireConnection": "$HANGFIRE_CONNECTION"
  },
  "JwtSettings": {
    "Secret": "$JWT_SECRET",
    "Issuer": "ShahinAI",
    "Audience": "ShahinAIUsers",
    "ExpiryMinutes": 60
  }
EOF

if [[ $CONFIGURE_SMTP =~ ^[Yy]$ ]]; then
    cat >> "$CONFIG_PATH" <<EOF
,
  "SmtpSettings": {
    "Host": "smtp.office365.com",
    "Port": 587,
    "EnableSsl": true,
    "FromEmail": "$SMTP_FROM_EMAIL",
    "FromName": "Shahin AI",
    "Username": "$SMTP_USERNAME",
    "Password": "$SMTP_PASSWORD"
  }
EOF
fi

cat >> "$CONFIG_PATH" <<EOF
}
EOF

echo -e "${GREEN}✅ Configuration file created: $CONFIG_PATH${NC}"

echo ""
echo "=================================================="
echo "  Setting Environment Variables"
echo "=================================================="
echo ""

# Create .env file for environment variables
ENV_FILE=".env"

cat > "$ENV_FILE" <<EOF
# Database Connection Strings
export ConnectionStrings__DefaultConnection="$DEFAULT_CONNECTION"
export ConnectionStrings__GrcAuthDb="$AUTH_CONNECTION"
export ConnectionStrings__Redis="$REDIS_CONNECTION"
export ConnectionStrings__HangfireConnection="$HANGFIRE_CONNECTION"

# JWT Settings
export JWT_SECRET="$JWT_SECRET"
EOF

if [[ $CONFIGURE_SMTP =~ ^[Yy]$ ]]; then
    cat >> "$ENV_FILE" <<EOF

# SMTP Settings
export SMTP_FROM_EMAIL="$SMTP_FROM_EMAIL"
export SMTP_USERNAME="$SMTP_USERNAME"
export SMTP_PASSWORD="$SMTP_PASSWORD"
EOF
fi

echo -e "${GREEN}✅ Environment variables file created: $ENV_FILE${NC}"
echo -e "${YELLOW}To load these variables, run: source .env${NC}"

# Load environment variables for current session
source "$ENV_FILE"

echo ""
echo "=================================================="
echo "  Database Setup"
echo "=================================================="
echo ""

read -p "Do you want to create the databases now? (y/n) " -n 1 -r CREATE_DBS
echo ""

if [[ $CREATE_DBS =~ ^[Yy]$ ]]; then
    echo -e "${YELLOW}Creating databases...${NC}"
    
    # Create SQL script
    cat > create-databases.sql <<EOF
-- Create databases if they don't exist
SELECT 'CREATE DATABASE "GrcMvcDb"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'GrcMvcDb')\gexec

SELECT 'CREATE DATABASE "GrcAuthDb"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'GrcAuthDb')\gexec

SELECT 'CREATE DATABASE "HangfireDb"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'HangfireDb')\gexec
EOF

    echo "SQL script created: create-databases.sql"
    
    # Try to create databases
    echo -e "${YELLOW}Attempting to create databases...${NC}"
    PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -U $DB_USER -p $DB_PORT -d postgres -f create-databases.sql
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ Databases created successfully${NC}"
    else
        echo -e "${RED}❌ Failed to create databases${NC}"
        echo -e "${YELLOW}You may need to run this manually:${NC}"
        echo "  PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -U $DB_USER -p $DB_PORT -d postgres -f create-databases.sql"
    fi
fi

echo ""
echo "=================================================="
echo "  Next Steps"
echo "=================================================="
echo ""

echo -e "${YELLOW}1. Load environment variables:${NC}"
echo -e "   ${CYAN}source .env${NC}"
echo ""

echo -e "${YELLOW}2. Run database migrations:${NC}"
echo -e "   ${CYAN}cd src/GrcMvc${NC}"
echo -e "   ${CYAN}dotnet ef database update --context GrcDbContext${NC}"
echo -e "   ${CYAN}dotnet ef database update --context GrcAuthDbContext${NC}"
echo ""

echo -e "${YELLOW}3. Start the application:${NC}"
echo -e "   ${CYAN}cd src/GrcMvc${NC}"
echo -e "   ${CYAN}dotnet run${NC}"
echo ""

echo -e "${YELLOW}4. Test the application:${NC}"
echo -e "   ${CYAN}Navigate to: http://localhost:5000/account/register${NC}"
echo ""

echo "=================================================="
echo "  Configuration Summary"
echo "=================================================="
echo ""
echo "Database Host:     $DB_HOST"
echo "Database Port:     $DB_PORT"
echo "Database User:     $DB_USER"
echo "Config File:       $CONFIG_PATH"
echo "Env File:          $ENV_FILE"
echo "JWT Secret:        Generated (64 chars)"
if [[ $CONFIGURE_SMTP =~ ^[Yy]$ ]]; then
    echo "SMTP Configured:   Yes"
    echo "SMTP From:         $SMTP_FROM_EMAIL"
fi
echo ""

echo -e "${GREEN}✅ Setup complete!${NC}"
echo ""

# Offer to run migrations
read -p "Do you want to run database migrations now? (y/n) " -n 1 -r RUN_MIGRATIONS
echo ""

if [[ $RUN_MIGRATIONS =~ ^[Yy]$ ]]; then
    echo ""
    echo -e "${YELLOW}Running migrations...${NC}"
    
    cd src/GrcMvc
    
    echo -e "${YELLOW}Running GrcDbContext migrations...${NC}"
    dotnet ef database update --context GrcDbContext
    
    echo -e "${YELLOW}Running GrcAuthDbContext migrations...${NC}"
    dotnet ef database update --context GrcAuthDbContext
    
    echo ""
    echo -e "${GREEN}✅ Migrations complete!${NC}"
    
    cd ../..
fi

echo ""
echo -e "${CYAN}Setup script completed successfully!${NC}"
echo ""

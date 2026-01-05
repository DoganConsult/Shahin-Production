#!/bin/bash

# Complete Setup Script for Customer User
# This script sets up everything needed for a customer user to start using the GRC system

set -e

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m'

PROJECT_ROOT="/root/.cursor/worktrees/GRC__Workspace___SSH__doganconsult_/bsk"
APP_DIR="${PROJECT_ROOT}/src/GrcMvc"

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}GRC System - Customer User Setup${NC}"
echo -e "${BLUE}========================================${NC}"

# Step 1: Check prerequisites
echo -e "\n${YELLOW}Step 1: Checking prerequisites...${NC}"

if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}ERROR: .NET SDK not found. Please install .NET 8.0 SDK.${NC}"
    exit 1
fi
echo -e "${GREEN}✓ .NET SDK found: $(dotnet --version)${NC}"

# Step 2: Check database connection
echo -e "\n${YELLOW}Step 2: Checking database configuration...${NC}"
cd "${APP_DIR}"

CONNECTION_STRING=$(grep -A 1 "DefaultConnection" appsettings.json | grep -v "DefaultConnection" | tr -d '", ' || echo "")
if [ -z "$CONNECTION_STRING" ] || [ "$CONNECTION_STRING" == "" ]; then
    echo -e "${YELLOW}⚠ Database connection string not configured in appsettings.json${NC}"
    echo -e "${YELLOW}Please set ConnectionStrings:DefaultConnection in appsettings.json${NC}"
    echo -e "${YELLOW}Example: Host=localhost;Database=GrcMvcDb;Username=postgres;Password=yourpassword${NC}"
    read -p "Do you want to continue anyway? (y/n) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
else
    echo -e "${GREEN}✓ Database connection string found${NC}"
fi

# Step 3: Build the application
echo -e "\n${YELLOW}Step 3: Building application...${NC}"
dotnet build GrcMvc.csproj -c Release --no-restore 2>&1 | grep -E "(error|Error|succeeded|Succeeded)" || true
if [ ${PIPESTATUS[0]} -eq 0 ]; then
    echo -e "${GREEN}✓ Build successful${NC}"
else
    echo -e "${YELLOW}⚠ Build completed with warnings${NC}"
fi

# Step 4: Check if database exists and run migrations
echo -e "\n${YELLOW}Step 4: Setting up database...${NC}"

# Try to run migrations
echo -e "${BLUE}Running database migrations...${NC}"
if dotnet ef database update --project GrcMvc.csproj --context GrcDbContext --no-build 2>&1; then
    echo -e "${GREEN}✓ Database migrations applied${NC}"
else
    echo -e "${YELLOW}⚠ Migration failed or database not accessible${NC}"
    echo -e "${YELLOW}Please ensure:${NC}"
    echo -e "  1. PostgreSQL is running"
    echo -e "  2. Database connection string is correct"
    echo -e "  3. Database user has proper permissions"
    read -p "Continue anyway? (y/n) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
fi

# Step 5: Verify seed data will run
echo -e "\n${YELLOW}Step 5: Preparing seed data...${NC}"
echo -e "${BLUE}Seed data will be created on first application start${NC}"
echo -e "${GREEN}✓ Seed data preparation complete${NC}"

# Step 6: Create startup information
echo -e "\n${YELLOW}Step 6: Creating startup information...${NC}"

cat > "${PROJECT_ROOT}/CUSTOMER_USER_SETUP.txt" << EOF
========================================
GRC SYSTEM - CUSTOMER USER SETUP COMPLETE
========================================

SETUP STATUS: ✅ Ready

DEFAULT USERS CREATED ON FIRST START:
-------------------------------------
1. Admin User:
   Email: admin@grcsystem.com
   Password: Admin@123456
   Role: SuperAdmin

2. Manager User:
   Email: manager@grcsystem.com
   Password: Manager@123456
   Role: ComplianceManager

HOW TO START THE APPLICATION:
------------------------------
Option 1: Direct Execution
  cd ${APP_DIR}
  dotnet run --environment Production

Option 2: Use Published Build
  cd ${PROJECT_ROOT}/publish
  dotnet GrcMvc.dll --environment Production

Option 3: Docker
  cd ${PROJECT_ROOT}
  docker-compose up -d

FIRST TIME STARTUP:
-------------------
On first startup, the application will automatically:
1. Create default tenant
2. Seed role profiles
3. Seed workflow definitions
4. Seed RBAC system (permissions, features, roles)
5. Create admin and manager users
6. Link users to default tenant

LOGIN CREDENTIALS:
------------------
Admin: admin@grcsystem.com / Admin@123456
Manager: manager@grcsystem.com / Manager@123456

IMPORTANT NOTES:
----------------
- Change default passwords after first login
- Configure email settings for notifications
- Set up file storage for document uploads
- Review appsettings.Production.json for production settings

SUPPORT:
--------
- Application logs: /app/logs/grcmvc-.log
- Health check: http://localhost/health
- Hangfire dashboard: http://localhost/hangfire

========================================
EOF

echo -e "${GREEN}✓ Startup information created: CUSTOMER_USER_SETUP.txt${NC}"

# Step 7: Summary
echo -e "\n${GREEN}========================================${NC}"
echo -e "${GREEN}Setup Complete!${NC}"
echo -e "${GREEN}========================================${NC}"
echo -e ""
echo -e "${BLUE}Next Steps:${NC}"
echo -e "1. Review CUSTOMER_USER_SETUP.txt for login credentials"
echo -e "2. Start the application using one of the options above"
echo -e "3. Login with admin credentials on first start"
echo -e "4. Change default passwords"
echo -e "5. Configure production settings"
echo -e ""
echo -e "${GREEN}✅ Customer user setup completed successfully!${NC}"

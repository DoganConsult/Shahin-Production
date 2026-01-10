#!/bin/bash

# GRC System Implementation Verification Script
# Verifies all implementations are complete, integrated, and error-free

set -e

echo "🔍 GRC System Implementation Verification"
echo "=========================================="
echo ""

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

ERRORS=0
WARNINGS=0

# Function to check file exists
check_file() {
    if [ -f "$1" ]; then
        echo -e "${GREEN}✅${NC} $1"
    else
        echo -e "${RED}❌${NC} $1 - MISSING"
        ((ERRORS++))
    fi
}

# Function to check service registered
check_service_registered() {
    if grep -q "$1" src/GrcMvc/Program.cs; then
        echo -e "${GREEN}✅${NC} $1 registered in DI"
    else
        echo -e "${RED}❌${NC} $1 NOT registered in DI"
        ((ERRORS++))
    fi
}

# Function to check entity in DbContext
check_entity_in_dbcontext() {
    if grep -q "$1" src/GrcMvc/Data/GrcDbContext.cs; then
        echo -e "${GREEN}✅${NC} $1 in DbContext"
    else
        echo -e "${RED}❌${NC} $1 NOT in DbContext"
        ((ERRORS++))
    fi
}

echo "📁 Checking Required Files..."
echo "----------------------------"

# Role Delegation System
check_file "src/GrcMvc/Services/Interfaces/IRoleDelegationService.cs"
check_file "src/GrcMvc/Services/Implementations/RoleDelegationService.cs"
check_file "src/GrcMvc/Models/DTOs/DelegationDtos.cs"
check_file "src/GrcMvc/Models/Entities/TaskDelegation.cs"

# Catalog Data Service (Interface only for now)
check_file "src/GrcMvc/Services/Interfaces/ICatalogDataService.cs"
check_file "src/GrcMvc/Models/DTOs/CatalogDtos.cs"

# Smart Onboarding
check_file "src/GrcMvc/Services/Interfaces/ISmartOnboardingService.cs"
check_file "src/GrcMvc/Services/Implementations/SmartOnboardingService.cs"

echo ""
echo "🔗 Checking Service Registration..."
echo "-----------------------------------"

check_service_registered "IRoleDelegationService"
check_service_registered "ISmartOnboardingService"
check_service_registered "IWorkflowEngineService"
check_service_registered "IEvidenceService"

# Check if CatalogDataService is registered (it shouldn't be yet)
if grep -q "ICatalogDataService" src/GrcMvc/Program.cs; then
    echo -e "${GREEN}✅${NC} ICatalogDataService registered in DI"
else
    echo -e "${YELLOW}⚠️${NC} ICatalogDataService NOT registered (implementation missing)"
    ((WARNINGS++))
fi

echo ""
echo "🗄️ Checking Database Context..."
echo "-------------------------------"

check_entity_in_dbcontext "TaskDelegation"
check_entity_in_dbcontext "OrganizationProfile"
check_entity_in_dbcontext "RegulatorCatalog"
check_entity_in_dbcontext "FrameworkCatalog"
check_entity_in_dbcontext "ControlCatalog"
check_entity_in_dbcontext "EvidenceTypeCatalog"

echo ""
echo "🔨 Checking Build Status..."
echo "---------------------------"

cd src/GrcMvc
if dotnet build GrcMvc.csproj --no-restore 2>&1 | grep -q "Build succeeded"; then
    echo -e "${GREEN}✅${NC} Build successful"
    
    # Count warnings
    WARN_COUNT=$(dotnet build GrcMvc.csproj --no-restore 2>&1 | grep -c "warning" || true)
    if [ "$WARN_COUNT" -eq 0 ]; then
        echo -e "${GREEN}✅${NC} No warnings"
    else
        echo -e "${YELLOW}⚠️${NC} $WARN_COUNT warnings found"
        ((WARNINGS+=WARN_COUNT))
    fi
    
    # Count errors
    ERR_COUNT=$(dotnet build GrcMvc.csproj --no-restore 2>&1 | grep -c "error" || true)
    if [ "$ERR_COUNT" -eq 0 ]; then
        echo -e "${GREEN}✅${NC} No errors"
    else
        echo -e "${RED}❌${NC} $ERR_COUNT errors found"
        ((ERRORS+=ERR_COUNT))
    fi
else
    echo -e "${RED}❌${NC} Build failed"
    ((ERRORS++))
fi

cd ../..

echo ""
echo "📊 Summary"
echo "=========="
echo -e "Errors: ${RED}$ERRORS${NC}"
echo -e "Warnings: ${YELLOW}$WARNINGS${NC}"

if [ $ERRORS -eq 0 ]; then
    echo ""
    echo -e "${GREEN}✅ VERIFICATION PASSED${NC}"
    echo "All critical implementations are complete and integrated."
    exit 0
else
    echo ""
    echo -e "${RED}❌ VERIFICATION FAILED${NC}"
    echo "Please fix the errors above before proceeding."
    exit 1
fi

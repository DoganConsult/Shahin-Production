#!/bin/bash

# Parallel Migration Implementation - Verification Script
# This script verifies that all components were created successfully

echo "========================================"
echo "Parallel Migration - Verification"
echo "========================================"
echo ""

SUCCESS=0
FAILED=0

# Function to check file exists
check_file() {
    if [ -f "$1" ]; then
        echo "✅ $1"
        ((SUCCESS++))
    else
        echo "❌ MISSING: $1"
        ((FAILED++))
    fi
}

echo "📁 Checking Configuration Files..."
check_file "/home/dogan/grc-system/src/GrcMvc/Configuration/GrcFeatureOptions.cs"
check_file "/home/dogan/grc-system/src/GrcMvc/appsettings.json"

echo ""
echo "📁 Checking Service Interfaces..."
check_file "/home/dogan/grc-system/src/GrcMvc/Services/Interfaces/IMetricsService.cs"
check_file "/home/dogan/grc-system/src/GrcMvc/Services/Interfaces/IUserManagementFacade.cs"

echo ""
echo "📁 Checking Service Implementations..."
check_file "/home/dogan/grc-system/src/GrcMvc/Services/Implementations/MetricsService.cs"
check_file "/home/dogan/grc-system/src/GrcMvc/Services/Implementations/UserManagementFacade.cs"

echo ""
echo "📁 Checking Controllers..."
check_file "/home/dogan/grc-system/src/GrcMvc/Controllers/PlatformAdminControllerV2.cs"
check_file "/home/dogan/grc-system/src/GrcMvc/Controllers/MigrationMetricsController.cs"

echo ""
echo "📁 Checking Views..."
check_file "/home/dogan/grc-system/src/GrcMvc/Views/PlatformAdmin/DashboardV2.cshtml"
check_file "/home/dogan/grc-system/src/GrcMvc/Views/PlatformAdmin/MigrationMetrics.cshtml"
check_file "/home/dogan/grc-system/src/GrcMvc/Views/PlatformAdmin/UsersV2.cshtml"

echo ""
echo "📁 Checking Documentation..."
check_file "/home/dogan/grc-system/PARALLEL_MIGRATION_COMPLETE.md"

echo ""
echo "========================================"
echo "Verification Summary"
echo "========================================"
echo "✅ Files Present: $SUCCESS"
echo "❌ Files Missing: $FAILED"

if [ $FAILED -eq 0 ]; then
    echo ""
    echo "🎉 SUCCESS! All files created successfully!"
    echo ""
    echo "Next Steps:"
    echo "1. Build the project: cd src/GrcMvc && dotnet build"
    echo "2. Run the application: dotnet run"
    echo "3. Access V2 Dashboard: https://localhost:5010/platform-admin/v2/dashboard"
    echo "4. View Migration Metrics: https://localhost:5010/platform-admin/migration-metrics"
    echo ""
    exit 0
else
    echo ""
    echo "⚠️  WARNING: $FAILED file(s) missing!"
    echo "Please review the implementation."
    echo ""
    exit 1
fi

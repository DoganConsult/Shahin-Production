#!/bin/bash

# Script to update remaining 5 controllers with policy enforcement
# This script adds PolicyEnforcementHelper injection and enforcement calls

CONTROLLERS=(
  "AssessmentController"
  "PolicyController"
  "RiskController"
  "AuditController"
  "ControlController"
)

for controller in "${CONTROLLERS[@]}"; do
  echo "Processing $controller..."
  
  file="src/GrcMvc/Controllers/${controller}.cs"
  
  if [ ! -f "$file" ]; then
    echo "  ❌ File not found: $file"
    continue
  fi
  
  # Backup
  cp "$file" "$file.bak"
  
  # Add imports
  sed -i '/^using GrcMvc.Models.DTOs;/a using GrcMvc.Application.Permissions;\nusing GrcMvc.Application.Policy;' "$file"
  
  # Add field declaration
  sed -i '/private readonly ILogger<'$controller'> _logger;/a \        private readonly PolicyEnforcementHelper _policyHelper;' "$file"
  
  # Update constructor parameter
  sed -i '/ILogger<'$controller'> logger,/a \            PolicyEnforcementHelper policyHelper,' "$file"
  
  # Update constructor assignment
  sed -i '/_logger = logger;/a \            _policyHelper = policyHelper;' "$file"
  
  echo "  ✅ Updated $controller"
done

echo ""
echo "✅ All controllers updated. Manual review required for enforcement calls."

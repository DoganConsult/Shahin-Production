#!/bin/bash
# Script to fix @Html.Raw XSS risks by replacing with System.Text.Json.JsonSerializer
# Phase 4 - Issue #6: Fix @Html.Raw XSS risks

set -e

VIEWS_DIR="/home/user/Shahin-Jan-2026/src/GrcMvc/Views"

echo "🔒 Fixing @Html.Raw XSS vulnerabilities..."
echo "==========================================="
echo ""

# Fix pattern 1: @Html.Raw(Json.Serialize(...)) -> @System.Text.Json.JsonSerializer.Serialize(...)
echo "📝 Replacing Json.Serialize with System.Text.Json.JsonSerializer.Serialize..."

find "$VIEWS_DIR" -name "*.cshtml" -type f -exec sed -i \
    's/@Html\.Raw(Json\.Serialize(/@System.Text.Json.JsonSerializer.Serialize(/g' {} +

echo "✅ Updated all Json.Serialize patterns"
echo ""

# List affected files
echo "📋 Files modified:"
grep -r "System.Text.Json.JsonSerializer.Serialize" "$VIEWS_DIR" --include="*.cshtml" -l | sort | uniq

echo ""
echo "✅ All @Html.Raw XSS fixes applied!"
echo ""
echo "⚠️  IMPORTANT: Views still need CSP nonces added manually"
echo "   Pattern: <script> -> <script nonce=\"@ViewData[\"CSPNonce\"]\">"
echo ""
echo "📖 See docs/CSP_NONCE_USAGE_GUIDE.md for details"

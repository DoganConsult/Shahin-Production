#!/bin/bash
# =============================================================================
# SHAHIN AI - Landing Page Deployment Script
# Deploys shahin-ai-website to production
# =============================================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
LANDING_DIR="$PROJECT_ROOT/shahin-ai-website"

echo "=========================================="
echo "  Shahin AI Landing Page Deployment"
echo "=========================================="

# Check if landing page directory exists
if [ ! -d "$LANDING_DIR" ]; then
    echo "ERROR: Landing page directory not found: $LANDING_DIR"
    exit 1
fi

cd "$LANDING_DIR"

# Step 1: Install dependencies
echo ""
echo "[1/4] Installing dependencies..."
npm install

# Step 2: Build production bundle
echo ""
echo "[2/4] Building production bundle..."
npm run build

# Step 3: Deploy based on method
echo ""
echo "[3/4] Deploying..."

# Check for Vercel CLI
if command -v vercel &> /dev/null; then
    echo "Deploying to Vercel..."
    vercel --prod
elif command -v netlify &> /dev/null; then
    echo "Deploying to Netlify..."
    netlify deploy --prod
else
    echo ""
    echo "No deployment CLI found. Build complete."
    echo ""
    echo "Manual deployment options:"
    echo "  1. Vercel:  npm i -g vercel && vercel --prod"
    echo "  2. Netlify: npm i -g netlify-cli && netlify deploy --prod"
    echo "  3. Docker:  See Dockerfile in project root"
    echo ""
    echo "Build output is in: $LANDING_DIR/.next"
fi

# Step 4: Verification
echo ""
echo "[4/4] Deployment complete!"
echo ""
echo "=========================================="
echo "  Post-Deployment Checklist"
echo "=========================================="
echo "[ ] Verify https://shahin-ai.com loads"
echo "[ ] Test trial signup form submission"
echo "[ ] Verify redirect to app.shahin-ai.com works"
echo "[ ] Check Arabic/English locale switching"
echo "[ ] Test mobile responsiveness"
echo "=========================================="

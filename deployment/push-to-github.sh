#!/bin/bash
# ══════════════════════════════════════════════════════════════
# Push to GitHub Repository
# ══════════════════════════════════════════════════════════════

set -e

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$PROJECT_ROOT"

echo -e "${YELLOW}Pushing to GitHub...${NC}"

# Check if git is initialized
if [ ! -d ".git" ]; then
    echo -e "${RED}Error: Not a git repository${NC}"
    exit 1
fi

# Check for changes
if [ -z "$(git status --porcelain)" ]; then
    echo -e "${YELLOW}No changes to commit${NC}"
else
    # Add all changes
    git add .
    
    # Commit with timestamp
    COMMIT_MSG="Deploy: Production build $(date +%Y-%m-%d_%H:%M:%S)"
    git commit -m "$COMMIT_MSG" || echo "Nothing to commit"
fi

# Get current branch
BRANCH=$(git branch --show-current)

# Push to GitHub
echo -e "${YELLOW}Pushing to origin/$BRANCH...${NC}"
git push origin "$BRANCH" || {
    echo -e "${YELLOW}Setting upstream if needed...${NC}"
    git push -u origin "$BRANCH"
}

echo -e "${GREEN}✅ Successfully pushed to GitHub${NC}"
echo -e "   Branch: $BRANCH"
echo -e "   Repository: $(git remote get-url origin)"

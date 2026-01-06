#!/bin/bash
# ============================================================
# Install Git Hooks for Mandatory Quality Checks
# Run once: ./scripts/install-hooks.sh
# ============================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
HOOKS_DIR="$PROJECT_ROOT/.git/hooks"

echo "Installing Shahin GRC Git Hooks..."

# Create pre-commit hook
cat > "$HOOKS_DIR/pre-commit" << 'EOF'
#!/bin/bash
# ============================================================
# PRE-COMMIT HOOK - Mandatory Quality Check
# Prevents commits if validation fails
# ============================================================

echo ""
echo "╔════════════════════════════════════════════════════════════╗"
echo "║  SHAHIN GRC - PRE-COMMIT QUALITY CHECK                     ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo ""

# Run quick validation (no full build for speed)
./scripts/quality-gate.sh quick

if [ $? -ne 0 ]; then
    echo ""
    echo "╔════════════════════════════════════════════════════════════╗"
    echo "║  ❌ COMMIT BLOCKED - Quality check failed                  ║"
    echo "║  Fix the errors above before committing                    ║"
    echo "╚════════════════════════════════════════════════════════════╝"
    exit 1
fi

echo ""
echo "✅ Pre-commit check passed"
EOF

# Create pre-push hook (full validation)
cat > "$HOOKS_DIR/pre-push" << 'EOF'
#!/bin/bash
# ============================================================
# PRE-PUSH HOOK - Full Quality Validation
# Prevents push if full validation fails
# ============================================================

echo ""
echo "╔════════════════════════════════════════════════════════════╗"
echo "║  SHAHIN GRC - PRE-PUSH QUALITY GATE                        ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo ""

# Run full validation before push
./scripts/quality-gate.sh ci

if [ $? -ne 0 ]; then
    echo ""
    echo "╔════════════════════════════════════════════════════════════╗"
    echo "║  ❌ PUSH BLOCKED - Quality gate failed                     ║"
    echo "║  Fix all errors before pushing                             ║"
    echo "╚════════════════════════════════════════════════════════════╝"
    exit 1
fi

echo ""
echo "✅ Pre-push validation passed - Pushing..."
EOF

# Make hooks executable
chmod +x "$HOOKS_DIR/pre-commit"
chmod +x "$HOOKS_DIR/pre-push"

echo ""
echo "✅ Git hooks installed successfully!"
echo ""
echo "Hooks enabled:"
echo "  • pre-commit  - Quick validation before each commit"
echo "  • pre-push    - Full validation before each push"
echo ""
echo "To bypass (NOT recommended):"
echo "  git commit --no-verify"
echo "  git push --no-verify"
EOF

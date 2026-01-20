#!/bin/bash
# Install script for Structured Development: frontend

set -e

echo "Installing Structured Development: frontend workflow..."

# Create directories
mkdir -p .claude/commands
mkdir -p .claude/agents

# Copy command file
cp commands/frontend.md .claude/commands/

# Copy agent files
for f in agents/*.md; do
  [ -e "$f" ] && cp "$f" .claude/agents/
done

# Copy MCP config if exists
if [ -f ".mcp.json" ]; then
  echo "MCP configuration found. Please review .mcp.json for required environment variables."
fi

echo "Installation complete!"
echo "Use '/frontend' to run the workflow."

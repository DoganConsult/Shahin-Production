# Structured Development: frontend - Usage Guide

## Quick Start

1. Extract the ZIP file to your project root
2. Run the install script:
   ```bash
   ./install.sh
   ```
3. Use the workflow:
   ```
   /frontend
   ```

## Requirements

- Claude Code CLI installed and configured
- Required MCP servers (see `.mcp.json` if present)

## Configuration

### Environment Variables

Check `.mcp.json` for required environment variables for MCP servers.

### Customization

You can customize the workflow by editing:
- `.claude/commands/frontend.md` - Main orchestrator
- `.claude/agents/*.md` - Individual agent behaviors

## Troubleshooting

If you encounter issues:
1. Ensure all required environment variables are set
2. Check that MCP servers are properly configured
3. Review agent instructions for any project-specific adjustments needed


# Install script for Structured Development: frontend (Windows PowerShell)

Write-Host "Installing Structured Development: frontend workflow..."

# Create directories
New-Item -ItemType Directory -Force -Path ".claude/commands" | Out-Null
New-Item -ItemType Directory -Force -Path ".claude/agents" | Out-Null

# Copy command file
Copy-Item "commands/frontend.md" -Destination ".claude/commands/"

# Copy agent files
Get-ChildItem "agents/*.md" | ForEach-Object {
    Copy-Item $_.FullName -Destination ".claude/agents/"
}

# Copy MCP config if exists
if (Test-Path ".mcp.json") {
    Write-Host "MCP configuration found. Please review .mcp.json for required environment variables."
}

Write-Host "Installation complete!"
Write-Host "Use '/frontend' to run the workflow."

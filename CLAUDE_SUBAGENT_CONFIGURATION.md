# Claude Sub-Agent Configuration Guide

## Overview
This guide explains how to configure and use Claude sub-agents in your GRC system. The sub-agent system allows you to create specialized AI assistants that can be delegated specific tasks.

## Current Sub-Agents

### 1. DoganFinance Agent
- **Location**: `/root/.claude/agents/DoganFinance.md`
- **Purpose**: Financial compliance and risk management
- **Activation**: Use `@DoganFinance` in your prompts

### 2. Shadow Agent
- **Location**: `/root/.claude/agents/Shadow.md`
- **Purpose**: Backup and shadow operations (template)
- **Activation**: Use `@Shadow` in your prompts

## How to Use Sub-Agents

### Method 1: Direct Invocation
```
@DoganFinance analyze our SOX compliance status
```

### Method 2: Task Delegation
```
Please have @DoganFinance review the Q4 financial controls
```

### Method 3: Multiple Agents
```
@DoganFinance check financial compliance and @Shadow backup the analysis
```

## Creating New Sub-Agents

### Step 1: Create Agent File
Create a new `.md` file in `/root/.claude/agents/` with this structure:

```markdown
# AgentName

## Role
Clear description of the agent's primary responsibility.

## Capabilities
- Specific capability 1
- Specific capability 2
- Additional capabilities...

## Instructions
Detailed operational guidelines for the agent.
```

### Step 2: Configure Agent Capabilities
Based on your GRC system's 9 planned agents:

#### ComplianceAgent.md
```markdown
# ComplianceAgent

## Role
Regulatory compliance analysis and monitoring

## Capabilities
- Regulatory framework mapping
- Compliance gap analysis
- Policy validation
- Regulatory updates tracking

## Instructions
Focus on regulatory compliance across all jurisdictions...
```

#### RiskAssessmentAgent.md
```markdown
# RiskAssessmentAgent

## Role
Enterprise risk scoring and assessment

## Capabilities
- Risk identification
- Impact analysis
- Likelihood assessment
- Risk mitigation strategies

## Instructions
Apply quantitative and qualitative risk assessment methods...
```

### Step 3: Test the Agent
```bash
# In your Claude interface, test with:
@AgentName perform your primary function
```

## Integration with GRC System

### Backend Integration (Planned)
The `Grc.Agents` module will provide programmatic access:

```csharp
// Future implementation in Grc.Agents
public class FinancialComplianceService
{
    private readonly IClaudeAgentService _agentService;

    public async Task<ComplianceReport> AnalyzeFinancialCompliance()
    {
        var result = await _agentService.InvokeAgent(
            "DoganFinance",
            "Analyze current financial compliance status"
        );

        return ParseComplianceReport(result);
    }
}
```

### Configuration Requirements
Add to your `.env` file:
```bash
# Claude API Configuration
ClaudeAgents__ApiKey="sk-ant-api03-xxxxx"
ClaudeAgents__MaxTokens=4000
ClaudeAgents__Temperature=0.7
ClaudeAgents__Model="claude-3-opus-20240229"

# Agent-specific settings
ClaudeAgents__Agents__DoganFinance__Enabled=true
ClaudeAgents__Agents__DoganFinance__Priority=High
ClaudeAgents__Agents__DoganFinance__Timeout=30
```

## Best Practices

### 1. Agent Specialization
- Keep each agent focused on a specific domain
- Avoid overlapping responsibilities
- Define clear boundaries between agents

### 2. Naming Convention
- Use descriptive names (e.g., `FinancialCompliance`, not `FC`)
- Follow PascalCase for agent names
- Match agent names to their domain modules

### 3. Documentation
- Always include comprehensive Role, Capabilities, and Instructions
- Provide examples of typical queries the agent handles
- Document integration points with other agents

### 4. Error Handling
- Each agent should gracefully handle missing data
- Provide fallback responses when information is incomplete
- Log errors for debugging

### 5. Performance
- Keep agent responses concise and actionable
- Set appropriate timeout values
- Monitor agent usage and response times

## Testing Your Configuration

### Quick Test Script
```bash
#!/bin/bash
# test-agent.sh

echo "Testing DoganFinance agent configuration..."

# Check if agent file exists
if [ -f "/root/.claude/agents/DoganFinance.md" ]; then
    echo "✓ DoganFinance agent file found"

    # Check file size
    SIZE=$(stat -c%s "/root/.claude/agents/DoganFinance.md")
    if [ $SIZE -gt 1000 ]; then
        echo "✓ Agent configuration appears complete ($SIZE bytes)"
    else
        echo "⚠ Agent configuration may be incomplete ($SIZE bytes)"
    fi
else
    echo "✗ DoganFinance agent file not found"
fi

# Check Claude API key
if grep -q "ClaudeAgents__ApiKey" .env; then
    echo "✓ Claude API key configured"
else
    echo "✗ Claude API key not configured in .env"
fi

echo "Test complete!"
```

### Manual Testing
1. Open Claude interface
2. Test basic invocation: `@DoganFinance hello`
3. Test specific task: `@DoganFinance analyze financial risks`
4. Verify response quality and relevance

## Troubleshooting

### Agent Not Responding
- Check if agent file exists in `/root/.claude/agents/`
- Verify agent name spelling matches exactly
- Ensure Claude API key is configured

### Poor Response Quality
- Review and enhance agent Instructions section
- Add more specific Capabilities
- Provide clearer Role definition

### Integration Issues
- Check `Grc.Agents` module is referenced in project
- Verify `ClaudeAgents__ApiKey` is set in configuration
- Review application logs for errors

## Advanced Configuration

### Multi-Agent Workflows
```markdown
# Workflow: Complete Financial Audit

1. @DoganFinance performs initial compliance assessment
2. @RiskAssessmentAgent evaluates identified risks
3. @AuditAgent creates audit trail
4. @ReportingAgent generates final report
```

### Agent Chaining
```csharp
// Future implementation
var workflow = new AgentWorkflow()
    .AddStep("DoganFinance", "Assess compliance")
    .AddStep("RiskAssessmentAgent", "Evaluate risks")
    .AddStep("ReportingAgent", "Generate report")
    .Execute();
```

## Next Steps

1. **Complete DoganFinance Configuration**: ✅ Done
2. **Create Shadow Agent**: Configure the Shadow.md template
3. **Add Remaining Agents**: Create the 9 planned GRC agents
4. **Test Agent Interactions**: Verify agents work together
5. **Implement Backend Integration**: Build the Grc.Agents module
6. **Deploy to Production**: Configure production API keys

## Resources

- **Claude API Docs**: https://docs.anthropic.com/claude/reference
- **ABP Framework**: https://docs.abp.io/
- **Project Settings**: `/home/dogan/grc-system/.claude/settings.local.json`
- **Global Settings**: `/root/.claude/settings.json`

## Support

For issues or questions:
- Check `/home/dogan/grc-system/CLAUDE.md` for project guidelines
- Review agent logs in application output
- Test connectivity with `./scripts/test-connectivity.sh`
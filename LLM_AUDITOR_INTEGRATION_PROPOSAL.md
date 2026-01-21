# LLM Auditor Integration Proposal for Shahin AI GRC Platform

## Overview

This proposal outlines the integration of an **LLM Auditor-style verification agent** into the Shahin AI GRC Platform. The agent will provide fact-checking, claim verification, and accuracy validation for compliance statements, risk assessments, evidence descriptions, and policy content.

## Current Architecture

### Existing Agent System
- **Framework**: ASP.NET Core 8.0 MVC (C#)
- **AI Provider**: Anthropic Claude Sonnet 4.5 (not Google Gemini)
- **Agent Pattern**: Unified `ClaudeAgentService` + specialized services
- **12 Agents**: Compliance, Risk, Audit, Policy, Analytics, Report, Diagnostic, Support, Workflow, Evidence, Email, Shahin AI

### LLM Auditor Concept (Google ADK)
- **Framework**: Python with Google ADK
- **AI Provider**: Google Gemini API / Vertex AI
- **Pattern**: Critic Agent + Reviser Agent (two-stage verification)
- **Capabilities**: Fact-checking, claim verification, external source validation

## Proposed Integration

### New Agent: VERIFICATION_AGENT

**Purpose**: Verify accuracy of compliance claims, risk assessments, evidence descriptions, and policy statements.

**Agent Code**: `VERIFICATION_AGENT`

**Type**: Verification/Audit

**Implementation**: New `VerificationAgentService` implementing `IVerificationAgentService`

---

## Architecture Design

### Two-Stage Verification Pattern

```
User Input (Claim/Statement)
    ↓
[CRITIC_AGENT] - Analyzes and verifies claims
    ↓
[REVISER_AGENT] - Generates corrected/improved version
    ↓
Verification Result
```

### Service Structure

```
src/GrcMvc/Services/
├── Interfaces/
│   └── IVerificationAgentService.cs
└── Implementations/
    └── VerificationAgentService.cs
```

### Integration Points

1. **Evidence Verification**: Verify evidence descriptions and claims
2. **Risk Assessment Verification**: Validate risk statements and likelihood assessments
3. **Compliance Claim Verification**: Fact-check compliance statements
4. **Policy Content Verification**: Verify policy accuracy and completeness
5. **Audit Finding Verification**: Validate audit findings and recommendations

---

## Implementation Plan

### Phase 1: Core Verification Service

#### 1.1 Interface Definition

```csharp
public interface IVerificationAgentService
{
    /// <summary>
    /// Verify a compliance claim or statement
    /// </summary>
    Task<VerificationResult> VerifyClaimAsync(
        string claim,
        string? context = null,
        VerificationScope scope = VerificationScope.General,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify evidence description and accuracy
    /// </summary>
    Task<EvidenceVerificationResult> VerifyEvidenceAsync(
        Guid evidenceId,
        string? description = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify risk assessment statement
    /// </summary>
    Task<RiskVerificationResult> VerifyRiskAssessmentAsync(
        Guid riskId,
        string? assessment = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify policy content accuracy
    /// </summary>
    Task<PolicyVerificationResult> VerifyPolicyAsync(
        Guid policyId,
        string? content = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch verify multiple claims
    /// </summary>
    Task<List<VerificationResult>> VerifyClaimsBatchAsync(
        List<string> claims,
        VerificationScope scope = VerificationScope.General,
        CancellationToken cancellationToken = default);
}
```

#### 1.2 Result Models

```csharp
public class VerificationResult
{
    public string OriginalClaim { get; set; } = string.Empty;
    public VerificationVerdict Verdict { get; set; } // Accurate, Inaccurate, PartiallyAccurate, Unverifiable
    public int ConfidenceScore { get; set; } // 0-100
    public List<ClaimAnalysis> ClaimAnalyses { get; set; } = new();
    public string? CorrectedStatement { get; set; }
    public List<string> Justifications { get; set; } = new();
    public List<ExternalSource> Sources { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime VerifiedAt { get; set; }
}

public class ClaimAnalysis
{
    public string ClaimText { get; set; } = string.Empty;
    public VerificationVerdict Verdict { get; set; }
    public string Justification { get; set; } = string.Empty;
    public List<string> Sources { get; set; } = new();
}

public enum VerificationVerdict
{
    Accurate,
    Inaccurate,
    PartiallyAccurate,
    Unverifiable,
    RequiresHumanReview
}

public enum VerificationScope
{
    General,
    Compliance,
    Risk,
    Audit,
    Policy,
    Evidence
}
```

#### 1.3 Service Implementation

**Key Features:**
- Uses `IClaudeAgentService` for AI processing (not Google Gemini)
- Two-stage verification: Critic → Reviser
- Domain-specific prompts for GRC context
- Integration with existing evidence, risk, policy entities
- Fallback when Claude API unavailable

**Critic Agent Prompt Template:**
```
You are a compliance verification expert for the Shahin AI GRC Platform.

Analyze the following {scope} claim/statement and verify its accuracy:

CLAIM: {claim}
CONTEXT: {context}

Your task:
1. Identify all factual claims in the statement
2. Verify each claim against:
   - Regulatory frameworks (ISO 27001, NIST, GDPR, SAMA, NCA, PDPL, CITC)
   - Industry best practices
   - Internal policy requirements
   - Evidence and documentation
3. Provide verdict: Accurate, Inaccurate, Partially Accurate, or Unverifiable
4. Justify your verdict with specific reasons
5. Suggest corrections if inaccurate

Respond in JSON format:
{
  "verdict": "Accurate|Inaccurate|PartiallyAccurate|Unverifiable",
  "confidence": 85,
  "claimAnalyses": [
    {
      "claimText": "...",
      "verdict": "...",
      "justification": "...",
      "sources": ["..."]
    }
  ],
  "justifications": ["..."],
  "recommendations": ["..."]
}
```

**Reviser Agent Prompt Template:**
```
You are a compliance content reviser for the Shahin AI GRC Platform.

Based on the verification analysis, revise the following statement to be accurate and compliant:

ORIGINAL: {originalClaim}
VERIFICATION: {verificationResult}

Provide a corrected version that:
1. Maintains the original intent
2. Corrects any inaccuracies
3. Aligns with regulatory requirements
4. Is clear and professional

CORRECTED STATEMENT:
```

---

### Phase 2: Integration with Existing Services

#### 2.1 Evidence Service Integration

```csharp
// In EvidenceService.cs
public async Task<EvidenceDto> CreateAsync(CreateEvidenceDto dto)
{
    // ... existing validation ...
    
    // Optional: Verify evidence description
    if (_verificationAgent != null && _config.EnableEvidenceVerification)
    {
        var verification = await _verificationAgent.VerifyEvidenceAsync(
            Guid.Empty, // Will be set after creation
            dto.Description);
        
        if (verification.Verdict == VerificationVerdict.Inaccurate)
        {
            _logger.LogWarning("Evidence description verification failed: {Issues}",
                string.Join(", ", verification.Recommendations));
            // Optionally: Flag for review or auto-correct
        }
    }
    
    // ... create evidence ...
}
```

#### 2.2 Risk Service Integration

```csharp
// In RiskService.cs
public async Task<RiskDto> CreateAsync(CreateRiskDto dto)
{
    // ... existing validation ...
    
    // Verify risk assessment statement
    if (_verificationAgent != null)
    {
        var verification = await _verificationAgent.VerifyRiskAssessmentAsync(
            Guid.Empty,
            dto.Description);
        
        // Store verification result as metadata
    }
    
    // ... create risk ...
}
```

#### 2.3 Policy Service Integration

```csharp
// In PolicyService.cs
public async Task<PolicyDto> CreateAsync(CreatePolicyDto dto)
{
    // ... existing validation ...
    
    // Verify policy content
    if (_verificationAgent != null)
    {
        var verification = await _verificationAgent.VerifyPolicyAsync(
            Guid.Empty,
            dto.Content);
        
        if (verification.Verdict == VerificationVerdict.PartiallyAccurate)
        {
            // Suggest improvements
            dto.Content = verification.CorrectedStatement ?? dto.Content;
        }
    }
    
    // ... create policy ...
}
```

---

### Phase 3: API Endpoints

#### 3.1 Verification API Controller

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VerificationController : ControllerBase
{
    private readonly IVerificationAgentService _verificationAgent;
    
    /// <summary>
    /// Verify a compliance claim
    /// POST /api/verification/claim
    /// </summary>
    [HttpPost("claim")]
    public async Task<ActionResult<VerificationResult>> VerifyClaim(
        [FromBody] VerifyClaimRequest request)
    {
        var result = await _verificationAgent.VerifyClaimAsync(
            request.Claim,
            request.Context,
            request.Scope);
        return Ok(result);
    }
    
    /// <summary>
    /// Verify evidence
    /// POST /api/verification/evidence/{evidenceId}
    /// </summary>
    [HttpPost("evidence/{evidenceId}")]
    public async Task<ActionResult<EvidenceVerificationResult>> VerifyEvidence(
        Guid evidenceId)
    {
        var result = await _verificationAgent.VerifyEvidenceAsync(evidenceId);
        return Ok(result);
    }
    
    // ... other endpoints ...
}
```

---

### Phase 4: UI Integration

#### 4.1 Verification Badge Component

Add verification status badges to:
- Evidence details page
- Risk assessment pages
- Policy content pages
- Audit finding pages

#### 4.2 Verification Panel

Add a collapsible verification panel showing:
- Verification verdict
- Confidence score
- Claim analyses
- Sources and justifications
- Corrected statement (if applicable)

---

## Configuration

### appsettings.json

```json
{
  "VerificationAgent": {
    "Enabled": true,
    "AutoVerifyOnCreate": false,
    "AutoVerifyOnUpdate": false,
    "RequireVerificationForPublish": false,
    "ConfidenceThreshold": 80,
    "EnableExternalSources": true,
    "Scopes": {
      "Evidence": true,
      "Risk": true,
      "Policy": true,
      "Compliance": true,
      "Audit": true
    }
  }
}
```

---

## Differences from Google LLM Auditor

| Aspect | Google LLM Auditor | GRC Verification Agent |
|--------|-------------------|----------------------|
| **Language** | Python | C# (.NET 8.0) |
| **AI Provider** | Google Gemini / Vertex AI | Anthropic Claude Sonnet 4.5 |
| **Framework** | Google ADK | ASP.NET Core MVC |
| **Domain** | General fact-checking | GRC-specific (compliance, risk, audit) |
| **Integration** | Standalone agent | Integrated with existing GRC services |
| **Sources** | Google Search | Regulatory frameworks, internal policies, evidence |
| **Output** | General corrections | GRC-compliant corrections with framework alignment |

---

## Benefits

1. **Accuracy**: Ensures compliance claims are factually correct
2. **Consistency**: Validates statements against regulatory frameworks
3. **Quality**: Improves evidence descriptions and risk assessments
4. **Compliance**: Helps maintain alignment with standards (ISO 27001, NIST, etc.)
5. **Audit Trail**: Creates verification records for audit purposes
6. **Automation**: Reduces manual review burden

---

## Implementation Timeline

### Week 1: Core Service
- [ ] Create `IVerificationAgentService` interface
- [ ] Implement `VerificationAgentService` with basic verification
- [ ] Create result models and DTOs
- [ ] Unit tests

### Week 2: Integration
- [ ] Integrate with EvidenceService
- [ ] Integrate with RiskService
- [ ] Integrate with PolicyService
- [ ] Add verification metadata to entities

### Week 3: API & UI
- [ ] Create VerificationController API endpoints
- [ ] Add verification badges to UI
- [ ] Create verification panel component
- [ ] Add verification history tracking

### Week 4: Testing & Documentation
- [ ] Integration tests
- [ ] End-to-end testing
- [ ] Documentation
- [ ] Performance optimization

---

## Next Steps

1. **Review and Approve**: Review this proposal and approve the approach
2. **Start Implementation**: Begin with Phase 1 (Core Service)
3. **Iterate**: Implement incrementally with feedback

---

## Questions for Discussion

1. Should verification be **mandatory** or **optional** for certain operations?
2. Should we store verification results in the database or only return them on-demand?
3. Should we integrate with external fact-checking APIs (beyond Claude)?
4. What confidence threshold should trigger human review?
5. Should verification be synchronous or asynchronous (background job)?

---

**Proposed By**: AI Assistant  
**Date**: 2026-01-10  
**Status**: Proposal - Awaiting Approval

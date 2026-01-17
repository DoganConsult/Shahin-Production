using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Seeds the AI Agent Team Members - Part of Dr. Dogan's Team
/// These are registered AI agents with defined roles, permissions, and capabilities
/// </summary>
public static class AiAgentTeamSeeds
{
    /// <summary>
    /// Seeds all AI Agents as team members with full registration
    /// </summary>
    public static async Task SeedAsync(GrcDbContext context)
    {
        // Check if already seeded (use IgnoreQueryFilters to bypass tenant filtering)
        if (await context.AgentDefinitions.IgnoreQueryFilters().AnyAsync())
        {
            return;
        }

        try
        {
            var agents = GetAgentDefinitions();
            await context.AgentDefinitions.AddRangeAsync(agents);
            await context.SaveChangesAsync();

        // Add capabilities for each agent
        foreach (var agent in agents)
        {
            var capabilities = GetAgentCapabilities(agent);
            await context.AgentCapabilities.AddRangeAsync(capabilities);
        }

        // Add approval gates
        var approvalGates = GetApprovalGates(agents);
        await context.AgentApprovalGates.AddRangeAsync(approvalGates);

        // Add SoD rules
        var sodRules = GetSoDRules();
        await context.AgentSoDRules.AddRangeAsync(sodRules);

        // Add human-retained responsibilities
        var responsibilities = GetHumanRetainedResponsibilities();
        await context.HumanRetainedResponsibilities.AddRangeAsync(responsibilities);

        await context.SaveChangesAsync();
        }
        catch (Exception)
        {
            // Already seeded or concurrent seeding - ignore duplicate key errors
        }
    }

    /// <summary>
    /// Get all AI Agent definitions - Dr. Dogan's AI Team
    /// </summary>
    private static List<AgentDefinition> GetAgentDefinitions()
    {
        return new List<AgentDefinition>
        {
            // ========== SHAHIN - Main AI Assistant ==========
            new AgentDefinition
            {
                Id = Guid.Parse("00000000-0000-0000-0001-000000000001"),
                AgentCode = "SHAHIN_AI",
                Name = "Shahin AI Assistant",
                NameAr = "شاهين - المساعد الذكي",
                Description = "Primary AI assistant for the GRC platform. Orchestrates other agents and provides unified interface for users.",
                AgentType = "Orchestrator",
                CapabilitiesJson = JsonSerializer.Serialize(new[] { 
                    "OrchestrateTasks", "DelegateToAgents", "ProvideGuidance", 
                    "AnswerQuestions", "GenerateReports", "MonitorAgents" 
                }),
                DataSourcesJson = JsonSerializer.Serialize(new[] { 
                    "AllModules", "UserContext", "TenantData", "SystemMetrics" 
                }),
                AllowedActionsJson = JsonSerializer.Serialize(new[] { 
                    "Chat", "Delegate", "Report", "Alert", "Guide" 
                }),
                ApprovalRequiredActionsJson = JsonSerializer.Serialize(new[] { 
                    "ModifySystemConfig", "DeleteData", "OverrideWorkflow" 
                }),
                AutoApprovalConfidenceThreshold = 95,
                OversightRoleCode = "PLATFORM_ADMIN",
                EscalationRoleCode = "PLATFORM_ADMIN",
                IsActive = true,
                Version = "1.0",
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            },

            // ========== COMPLIANCE AGENT ==========
            new AgentDefinition
            {
                Id = Guid.Parse("00000000-0000-0000-0001-000000000002"),
                AgentCode = "COMPLIANCE_AGENT",
                Name = "Compliance Analysis Agent",
                NameAr = "وكيل تحليل الامتثال",
                Description = "Analyzes compliance requirements, identifies gaps, and provides remediation guidance. Specializes in KSA frameworks (NCA ECC, SAMA CSF, PDPL).",
                AgentType = "Analysis",
                CapabilitiesJson = JsonSerializer.Serialize(new[] { 
                    "AnalyzeFramework", "IdentifyGaps", "SuggestRemediation", 
                    "MapRequirements", "ScoreCompliance", "GenerateComplianceReport" 
                }),
                DataSourcesJson = JsonSerializer.Serialize(new[] { 
                    "Frameworks", "Controls", "Assessments", "Evidence", "Mappings" 
                }),
                AllowedActionsJson = JsonSerializer.Serialize(new[] { 
                    "Analyze", "Suggest", "Report", "Map", "Score" 
                }),
                ApprovalRequiredActionsJson = JsonSerializer.Serialize(new[] { 
                    "ModifyFramework", "ApproveAssessment", "CertifyCompliance" 
                }),
                AutoApprovalConfidenceThreshold = 85,
                OversightRoleCode = "COMPLIANCE_OFFICER",
                EscalationRoleCode = "GRC_MANAGER",
                IsActive = true,
                Version = "1.0",
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            },

            // ========== RISK AGENT ==========
            new AgentDefinition
            {
                Id = Guid.Parse("00000000-0000-0000-0001-000000000003"),
                AgentCode = "RISK_AGENT",
                Name = "Risk Assessment Agent",
                NameAr = "وكيل تقييم المخاطر",
                Description = "Analyzes and scores risks, identifies risk factors, and suggests mitigation strategies.",
                AgentType = "Analysis",
                CapabilitiesJson = JsonSerializer.Serialize(new[] { 
                    "AnalyzeRisk", "ScoreRisk", "IdentifyFactors", 
                    "SuggestMitigation", "MonitorRisk", "GenerateRiskReport" 
                }),
                DataSourcesJson = JsonSerializer.Serialize(new[] { 
                    "Risks", "Controls", "Incidents", "Threats", "Vulnerabilities" 
                }),
                AllowedActionsJson = JsonSerializer.Serialize(new[] { 
                    "Analyze", "Score", "Suggest", "Monitor", "Report" 
                }),
                ApprovalRequiredActionsJson = JsonSerializer.Serialize(new[] { 
                    "AcceptRisk", "CloseRisk", "ModifyRiskRating" 
                }),
                AutoApprovalConfidenceThreshold = 80,
                OversightRoleCode = "RISK_MANAGER",
                EscalationRoleCode = "GRC_MANAGER",
                IsActive = true,
                Version = "1.0",
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            },

            // ========== AUDIT AGENT ==========
            new AgentDefinition
            {
                Id = Guid.Parse("00000000-0000-0000-0001-000000000004"),
                AgentCode = "AUDIT_AGENT",
                Name = "Audit Analysis Agent",
                NameAr = "وكيل تحليل التدقيق",
                Description = "Analyzes audit trails, identifies patterns, and provides findings analysis.",
                AgentType = "Analysis",
                CapabilitiesJson = JsonSerializer.Serialize(new[] { 
                    "AnalyzeAudit", "IdentifyPatterns", "AnalyzeFindings", 
                    "SuggestImprovements", "TrackRemediation", "GenerateAuditReport" 
                }),
                DataSourcesJson = JsonSerializer.Serialize(new[] { 
                    "Audits", "Findings", "Evidence", "ActionPlans", "AuditTrail" 
                }),
                AllowedActionsJson = JsonSerializer.Serialize(new[] { 
                    "Analyze", "Identify", "Suggest", "Track", "Report" 
                }),
                ApprovalRequiredActionsJson = JsonSerializer.Serialize(new[] { 
                    "CloseAudit", "ApproveFinding", "SignOffAudit" 
                }),
                AutoApprovalConfidenceThreshold = 75,
                OversightRoleCode = "AUDITOR",
                EscalationRoleCode = "AUDIT_MANAGER",
                IsActive = true,
                Version = "1.0",
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            },

            // ========== POLICY AGENT ==========
            new AgentDefinition
            {
                Id = Guid.Parse("00000000-0000-0000-0001-000000000005"),
                AgentCode = "POLICY_AGENT",
                Name = "Policy Management Agent",
                NameAr = "وكيل إدارة السياسات",
                Description = "Reviews policies, checks alignment with frameworks, and suggests improvements.",
                AgentType = "Analysis",
                CapabilitiesJson = JsonSerializer.Serialize(new[] { 
                    "AnalyzePolicy", "CheckAlignment", "SuggestImprovements", 
                    "IdentifyGaps", "ValidateCompliance", "GeneratePolicyReport" 
                }),
                DataSourcesJson = JsonSerializer.Serialize(new[] { 
                    "Policies", "Frameworks", "Standards", "Controls", "Regulations" 
                }),
                AllowedActionsJson = JsonSerializer.Serialize(new[] { 
                    "Analyze", "Check", "Suggest", "Validate", "Report" 
                }),
                ApprovalRequiredActionsJson = JsonSerializer.Serialize(new[] { 
                    "ApprovePolicy", "PublishPolicy", "RetirePolicy" 
                }),
                AutoApprovalConfidenceThreshold = 80,
                OversightRoleCode = "POLICY_OWNER",
                EscalationRoleCode = "GRC_MANAGER",
                IsActive = true,
                Version = "1.0",
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            },

            // ========== ANALYTICS AGENT ==========
            new AgentDefinition
            {
                Id = Guid.Parse("00000000-0000-0000-0001-000000000006"),
                AgentCode = "ANALYTICS_AGENT",
                Name = "Analytics & Insights Agent",
                NameAr = "وكيل التحليلات والرؤى",
                Description = "Generates insights from GRC data, identifies trends, and provides predictive analytics.",
                AgentType = "Analytics",
                CapabilitiesJson = JsonSerializer.Serialize(new[] { 
                    "GenerateInsights", "IdentifyTrends", "PredictRisks", 
                    "AnalyzeMetrics", "CreateDashboards", "GenerateAnalyticsReport" 
                }),
                DataSourcesJson = JsonSerializer.Serialize(new[] { 
                    "AllModules", "Metrics", "KRIs", "KPIs", "HistoricalData" 
                }),
                AllowedActionsJson = JsonSerializer.Serialize(new[] { 
                    "Analyze", "Predict", "Report", "Dashboard", "Alert" 
                }),
                ApprovalRequiredActionsJson = JsonSerializer.Serialize(new[] { 
                    "ModifyKRI", "ModifyKPI", "PublishReport" 
                }),
                AutoApprovalConfidenceThreshold = 90,
                OversightRoleCode = "GRC_MANAGER",
                EscalationRoleCode = "EXECUTIVE",
                IsActive = true,
                Version = "1.0",
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            },

            // ========== REPORT AGENT ==========
            new AgentDefinition
            {
                Id = Guid.Parse("00000000-0000-0000-0001-000000000007"),
                AgentCode = "REPORT_AGENT",
                Name = "Report Generation Agent",
                NameAr = "وكيل إنشاء التقارير",
                Description = "Generates comprehensive reports in natural language for various stakeholders.",
                AgentType = "Reporting",
                CapabilitiesJson = JsonSerializer.Serialize(new[] { 
                    "GenerateReport", "SummarizeData", "CreateExecutiveSummary", 
                    "FormatDocument", "LocalizeContent", "ScheduleReports" 
                }),
                DataSourcesJson = JsonSerializer.Serialize(new[] { 
                    "AllModules", "Templates", "UserPreferences", "BrandingAssets" 
                }),
                AllowedActionsJson = JsonSerializer.Serialize(new[] { 
                    "Generate", "Summarize", "Format", "Schedule", "Deliver" 
                }),
                ApprovalRequiredActionsJson = JsonSerializer.Serialize(new[] { 
                    "PublishExternalReport", "SendToRegulator", "ArchiveReport" 
                }),
                AutoApprovalConfidenceThreshold = 85,
                OversightRoleCode = "REPORT_OWNER",
                EscalationRoleCode = "GRC_MANAGER",
                IsActive = true,
                Version = "1.0",
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            },

            // ========== DIAGNOSTIC AGENT ==========
            new AgentDefinition
            {
                Id = Guid.Parse("00000000-0000-0000-0001-000000000008"),
                AgentCode = "DIAGNOSTIC_AGENT",
                Name = "System Diagnostic Agent",
                NameAr = "وكيل تشخيص النظام",
                Description = "Monitors system health, analyzes errors, and provides diagnostic insights.",
                AgentType = "Monitoring",
                CapabilitiesJson = JsonSerializer.Serialize(new[] { 
                    "MonitorHealth", "AnalyzeErrors", "DiagnoseIssues", 
                    "SuggestFixes", "TrackPerformance", "GenerateHealthReport" 
                }),
                DataSourcesJson = JsonSerializer.Serialize(new[] { 
                    "SystemLogs", "ErrorLogs", "PerformanceMetrics", "Alerts" 
                }),
                AllowedActionsJson = JsonSerializer.Serialize(new[] { 
                    "Monitor", "Analyze", "Diagnose", "Alert", "Report" 
                }),
                ApprovalRequiredActionsJson = JsonSerializer.Serialize(new[] { 
                    "RestartService", "ClearCache", "ModifyConfig" 
                }),
                AutoApprovalConfidenceThreshold = 80,
                OversightRoleCode = "PLATFORM_ADMIN",
                EscalationRoleCode = "PLATFORM_ADMIN",
                IsActive = true,
                Version = "1.0",
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            },

            // ========== SUPPORT AGENT ==========
            new AgentDefinition
            {
                Id = Guid.Parse("00000000-0000-0000-0001-000000000009"),
                AgentCode = "SUPPORT_AGENT",
                Name = "Customer Support Agent",
                NameAr = "وكيل دعم العملاء",
                Description = "Assists users during onboarding, answers questions, and provides guidance. Primary user-facing AI assistant.",
                AgentType = "Support",
                CapabilitiesJson = JsonSerializer.Serialize(new[] { 
                    "AnswerQuestions", "GuideOnboarding", "ResolveIssues", 
                    "EscalateToHuman", "TrackConversation", "ProvideKnowledge" 
                }),
                DataSourcesJson = JsonSerializer.Serialize(new[] { 
                    "KnowledgeBase", "FAQs", "UserContext", "OnboardingStatus", "Tickets" 
                }),
                AllowedActionsJson = JsonSerializer.Serialize(new[] { 
                    "Chat", "Guide", "Answer", "Escalate", "Track" 
                }),
                ApprovalRequiredActionsJson = JsonSerializer.Serialize(new[] { 
                    "RefundRequest", "AccountModification", "DataDeletion" 
                }),
                AutoApprovalConfidenceThreshold = 90,
                OversightRoleCode = "SUPPORT_MANAGER",
                EscalationRoleCode = "SUPPORT_MANAGER",
                IsActive = true,
                Version = "1.0",
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            },

            // ========== WORKFLOW AGENT ==========
            new AgentDefinition
            {
                Id = Guid.Parse("00000000-0000-0000-0001-000000000010"),
                AgentCode = "WORKFLOW_AGENT",
                Name = "Workflow Optimization Agent",
                NameAr = "وكيل تحسين سير العمل",
                Description = "Optimizes workflow processes, identifies bottlenecks, and suggests improvements.",
                AgentType = "Automation",
                CapabilitiesJson = JsonSerializer.Serialize(new[] { 
                    "OptimizeWorkflow", "IdentifyBottlenecks", "SuggestImprovements", 
                    "AutomateRouting", "ManageDeadlines", "EscalateOverdue" 
                }),
                DataSourcesJson = JsonSerializer.Serialize(new[] { 
                    "Workflows", "Tasks", "SLAs", "ProcessMetrics", "UserWorkload" 
                }),
                AllowedActionsJson = JsonSerializer.Serialize(new[] { 
                    "Optimize", "Route", "Remind", "Escalate", "Report" 
                }),
                ApprovalRequiredActionsJson = JsonSerializer.Serialize(new[] { 
                    "ModifyWorkflow", "OverrideSLA", "ReassignTask" 
                }),
                AutoApprovalConfidenceThreshold = 85,
                OversightRoleCode = "WORKFLOW_ADMIN",
                EscalationRoleCode = "GRC_MANAGER",
                IsActive = true,
                Version = "1.0",
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            },

            // ========== EVIDENCE AGENT ==========
            new AgentDefinition
            {
                Id = Guid.Parse("00000000-0000-0000-0001-000000000011"),
                AgentCode = "EVIDENCE_AGENT",
                Name = "Evidence Collection Agent",
                NameAr = "وكيل جمع الأدلة",
                Description = "Collects evidence from integrated systems, validates completeness, and organizes evidence packs.",
                AgentType = "Collection",
                CapabilitiesJson = JsonSerializer.Serialize(new[] { 
                    "CollectEvidence", "ValidateEvidence", "OrganizeEvidence", 
                    "LinkToControls", "TrackExpiry", "GenerateEvidencePack" 
                }),
                DataSourcesJson = JsonSerializer.Serialize(new[] { 
                    "Integrations", "ERP", "IAM", "SIEM", "ITSM", "CloudSystems" 
                }),
                AllowedActionsJson = JsonSerializer.Serialize(new[] { 
                    "Collect", "Validate", "Organize", "Link", "Track" 
                }),
                ApprovalRequiredActionsJson = JsonSerializer.Serialize(new[] { 
                    "ApproveEvidence", "DeleteEvidence", "ModifyEvidence" 
                }),
                AutoApprovalConfidenceThreshold = 90,
                OversightRoleCode = "EVIDENCE_OWNER",
                EscalationRoleCode = "COMPLIANCE_OFFICER",
                IsActive = true,
                Version = "1.0",
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            },

            // ========== EMAIL AGENT ==========
            new AgentDefinition
            {
                Id = Guid.Parse("00000000-0000-0000-0001-000000000012"),
                AgentCode = "EMAIL_AGENT",
                Name = "Email Classification Agent",
                NameAr = "وكيل تصنيف البريد الإلكتروني",
                Description = "Classifies incoming emails, routes to appropriate teams, and drafts responses.",
                AgentType = "Communication",
                CapabilitiesJson = JsonSerializer.Serialize(new[] { 
                    "ClassifyEmail", "RouteEmail", "DraftResponse", 
                    "ExtractData", "PrioritizeEmail", "TrackSLA" 
                }),
                DataSourcesJson = JsonSerializer.Serialize(new[] { 
                    "Mailboxes", "Templates", "RoutingRules", "PriorityMatrix" 
                }),
                AllowedActionsJson = JsonSerializer.Serialize(new[] { 
                    "Classify", "Route", "Draft", "Extract", "Prioritize" 
                }),
                ApprovalRequiredActionsJson = JsonSerializer.Serialize(new[] { 
                    "SendResponse", "DeleteEmail", "ForwardExternal" 
                }),
                AutoApprovalConfidenceThreshold = 85,
                OversightRoleCode = "EMAIL_ADMIN",
                EscalationRoleCode = "SUPPORT_MANAGER",
                IsActive = true,
                Version = "1.0",
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            }
        };
    }

    /// <summary>
    /// Get capabilities for a specific agent
    /// </summary>
    private static List<AgentCapability> GetAgentCapabilities(AgentDefinition agent)
    {
        var capabilities = new List<AgentCapability>();
        var capabilitiesArray = JsonSerializer.Deserialize<string[]>(agent.CapabilitiesJson ?? "[]") ?? Array.Empty<string>();

        foreach (var cap in capabilitiesArray)
        {
            capabilities.Add(new AgentCapability
            {
                Id = Guid.NewGuid(),
                AgentId = agent.Id,
                CapabilityCode = $"{agent.AgentCode}_{cap.ToUpperInvariant()}",
                Name = FormatCapabilityName(cap),
                Description = $"{agent.Name} capability: {cap}",
                Category = GetCapabilityCategory(cap),
                RiskLevel = GetCapabilityRiskLevel(cap),
                RequiresApproval = IsHighRiskCapability(cap),
                IsActive = true,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            });
        }

        return capabilities;
    }

    private static string FormatCapabilityName(string cap)
    {
        // Convert camelCase to Title Case
        var result = System.Text.RegularExpressions.Regex.Replace(cap, "(\\B[A-Z])", " $1");
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(result.ToLower());
    }

    private static string GetCapabilityCategory(string cap)
    {
        var lowerCap = cap.ToLower();
        if (lowerCap.Contains("read") || lowerCap.Contains("analyze") || lowerCap.Contains("identify") || lowerCap.Contains("monitor"))
            return "Read";
        if (lowerCap.Contains("write") || lowerCap.Contains("create") || lowerCap.Contains("generate") || lowerCap.Contains("modify"))
            return "Write";
        if (lowerCap.Contains("execute") || lowerCap.Contains("run") || lowerCap.Contains("collect"))
            return "Execute";
        if (lowerCap.Contains("suggest") || lowerCap.Contains("recommend") || lowerCap.Contains("draft"))
            return "Suggest";
        if (lowerCap.Contains("notify") || lowerCap.Contains("alert") || lowerCap.Contains("escalate"))
            return "Notify";
        return "Execute";
    }

    private static string GetCapabilityRiskLevel(string cap)
    {
        var lowerCap = cap.ToLower();
        if (lowerCap.Contains("delete") || lowerCap.Contains("approve") || lowerCap.Contains("publish") || lowerCap.Contains("send"))
            return "High";
        if (lowerCap.Contains("modify") || lowerCap.Contains("create") || lowerCap.Contains("generate"))
            return "Medium";
        return "Low";
    }

    private static bool IsHighRiskCapability(string cap)
    {
        var highRiskActions = new[] { "delete", "approve", "publish", "send", "modify", "override", "certify" };
        return highRiskActions.Any(hr => cap.ToLower().Contains(hr));
    }

    /// <summary>
    /// Get approval gates for agents
    /// </summary>
    private static List<AgentApprovalGate> GetApprovalGates(List<AgentDefinition> agents)
    {
        var gates = new List<AgentApprovalGate>();

        foreach (var agent in agents)
        {
            var approvalActions = JsonSerializer.Deserialize<string[]>(agent.ApprovalRequiredActionsJson ?? "[]") ?? Array.Empty<string>();
            
            if (approvalActions.Any())
            {
                gates.Add(new AgentApprovalGate
                {
                    Id = Guid.NewGuid(),
                    AgentId = agent.Id,
                    GateCode = $"{agent.AgentCode}_APPROVAL_GATE",
                    Name = $"{agent.Name} Approval Gate",
                    Description = $"Approval gate for high-risk actions by {agent.Name}",
                    TriggerActionTypes = string.Join(",", approvalActions),
                    ApproverRoleCode = agent.OversightRoleCode ?? "GRC_MANAGER",
                    ApprovalSLAHours = 24,
                    EscalationRoleCode = agent.EscalationRoleCode,
                    AutoRejectHours = 72,
                    BypassConfidenceThreshold = agent.AutoApprovalConfidenceThreshold,
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow
                });
            }
        }

        return gates;
    }

    /// <summary>
    /// Get Segregation of Duties rules for agents
    /// </summary>
    private static List<AgentSoDRule> GetSoDRules()
    {
        return new List<AgentSoDRule>
        {
            new AgentSoDRule
            {
                Id = Guid.NewGuid(),
                RuleCode = "SOD_EVIDENCE_APPROVAL",
                Name = "Evidence Collection vs Approval",
                Description = "Agent that collects evidence cannot approve it",
                Action1 = "CollectEvidence",
                Action1AgentTypes = "EVIDENCE_AGENT",
                Action2 = "ApproveEvidence",
                Action2AgentTypes = "COMPLIANCE_AGENT,AUDIT_AGENT",
                RiskDescription = "Same agent collecting and approving evidence creates conflict of interest",
                Enforcement = "Block",
                IsActive = true,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            },
            new AgentSoDRule
            {
                Id = Guid.NewGuid(),
                RuleCode = "SOD_RISK_ASSESSMENT_ACCEPTANCE",
                Name = "Risk Assessment vs Acceptance",
                Description = "Agent that assesses risk cannot accept it",
                Action1 = "AnalyzeRisk",
                Action1AgentTypes = "RISK_AGENT",
                Action2 = "AcceptRisk",
                Action2AgentTypes = "RISK_AGENT",
                RiskDescription = "Same agent assessing and accepting risk removes human oversight",
                Enforcement = "Block",
                IsActive = true,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            },
            new AgentSoDRule
            {
                Id = Guid.NewGuid(),
                RuleCode = "SOD_AUDIT_SIGNOFF",
                Name = "Audit Analysis vs Sign-off",
                Description = "Agent that analyzes audit cannot sign it off",
                Action1 = "AnalyzeAudit",
                Action1AgentTypes = "AUDIT_AGENT",
                Action2 = "SignOffAudit",
                Action2AgentTypes = "AUDIT_AGENT",
                RiskDescription = "Audit sign-off must be performed by human auditor",
                Enforcement = "Block",
                IsActive = true,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            },
            new AgentSoDRule
            {
                Id = Guid.NewGuid(),
                RuleCode = "SOD_POLICY_PUBLISH",
                Name = "Policy Review vs Publish",
                Description = "Agent that reviews policy cannot publish it",
                Action1 = "AnalyzePolicy",
                Action1AgentTypes = "POLICY_AGENT",
                Action2 = "PublishPolicy",
                Action2AgentTypes = "POLICY_AGENT",
                RiskDescription = "Policy publication requires human approval",
                Enforcement = "Block",
                IsActive = true,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            }
        };
    }

    /// <summary>
    /// Get human-retained responsibilities
    /// </summary>
    private static List<HumanRetainedResponsibility> GetHumanRetainedResponsibilities()
    {
        return new List<HumanRetainedResponsibility>
        {
            new HumanRetainedResponsibility
            {
                Id = Guid.NewGuid(),
                ResponsibilityCode = "HR_RISK_ACCEPTANCE",
                Name = "Risk Acceptance",
                NameAr = "قبول المخاطر",
                Description = "Accepting residual risk on behalf of the organization",
                Category = "RiskAcceptance",
                RoleCode = "RISK_MANAGER",
                NonDelegableReason = "Risk acceptance requires human judgment and accountability - regulatory requirement",
                RegulatoryReference = "NCA ECC 1-7, SAMA CSF",
                AgentSupportDescription = "Agent can analyze risk, provide recommendations, and prepare acceptance documentation",
                IsActive = true,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            },
            new HumanRetainedResponsibility
            {
                Id = Guid.NewGuid(),
                ResponsibilityCode = "HR_COMPLIANCE_ATTESTATION",
                Name = "Compliance Attestation",
                NameAr = "شهادة الامتثال",
                Description = "Attesting to compliance with regulatory frameworks",
                Category = "Attestation",
                RoleCode = "COMPLIANCE_OFFICER",
                NonDelegableReason = "Legal attestation requires human signature and personal accountability",
                RegulatoryReference = "All regulatory frameworks",
                AgentSupportDescription = "Agent can prepare attestation documents, verify evidence, and identify gaps",
                IsActive = true,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            },
            new HumanRetainedResponsibility
            {
                Id = Guid.NewGuid(),
                ResponsibilityCode = "HR_EXCEPTION_APPROVAL",
                Name = "Exception Approval",
                NameAr = "الموافقة على الاستثناءات",
                Description = "Approving exceptions to policies and controls",
                Category = "Exception",
                RoleCode = "GRC_MANAGER",
                NonDelegableReason = "Exception decisions require human judgment of business context",
                RegulatoryReference = "ISO 27001 A.5.1",
                AgentSupportDescription = "Agent can analyze exception request, assess risk, and recommend decision",
                IsActive = true,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            },
            new HumanRetainedResponsibility
            {
                Id = Guid.NewGuid(),
                ResponsibilityCode = "HR_AUDIT_SIGNOFF",
                Name = "Audit Sign-off",
                NameAr = "توقيع التدقيق",
                Description = "Signing off on internal and external audit findings",
                Category = "Attestation",
                RoleCode = "AUDITOR",
                NonDelegableReason = "Audit conclusions require professional auditor judgment",
                RegulatoryReference = "IIA Standards",
                AgentSupportDescription = "Agent can analyze findings, prepare working papers, and draft reports",
                IsActive = true,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            },
            new HumanRetainedResponsibility
            {
                Id = Guid.NewGuid(),
                ResponsibilityCode = "HR_POLICY_APPROVAL",
                Name = "Policy Approval",
                NameAr = "الموافقة على السياسات",
                Description = "Approving and publishing organizational policies",
                Category = "Governance",
                RoleCode = "POLICY_OWNER",
                NonDelegableReason = "Policy approval requires management authority",
                RegulatoryReference = "ISO 27001 5.2",
                AgentSupportDescription = "Agent can review policy, check alignment, and suggest improvements",
                IsActive = true,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            },
            new HumanRetainedResponsibility
            {
                Id = Guid.NewGuid(),
                ResponsibilityCode = "HR_DATA_BREACH_NOTIFICATION",
                Name = "Data Breach Notification",
                NameAr = "إخطار اختراق البيانات",
                Description = "Decision to notify regulators and affected parties of data breaches",
                Category = "Governance",
                RoleCode = "DPO",
                NonDelegableReason = "Legal notification requirements require human decision",
                RegulatoryReference = "PDPL Article 20",
                AgentSupportDescription = "Agent can assess breach severity, prepare notification, and track timeline",
                IsActive = true,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            }
        };
    }
}

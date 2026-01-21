using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Seed data for 7 assessment workflows based on BPMN element mapping.
/// Includes NCA ECC, SAMA CSF, PDPL PIA, ERM, Evidence Review, Finding Remediation, Policy Review
/// </summary>
public static class WorkflowDefinitionSeeds
{
    /// <summary>Seed all 7 workflow definitions into the database (global templates)</summary>
    public static async Task SeedWorkflowDefinitionsAsync(
        GrcDbContext context,
        ILogger logger)
    {
        try
        {
            // Check if workflows already exist
            if (await context.WorkflowDefinitions.AnyAsync())
            {
                logger.LogInformation("Workflow definitions already exist. Skipping seed.");
                return;
            }

            var definitions = new List<WorkflowDefinition>
            {
                CreateNcaEccAssessment(),
                CreateSamaCsfAssessment(),
                CreatePdplPia(),
                CreateErm(),
                CreateEvidenceReviewAndApproval(),
                CreateAuditFindingRemediation(),
                CreatePolicyReviewAndPublication()
            };

            context.WorkflowDefinitions.AddRange(definitions);
            await context.SaveChangesAsync();

            logger.LogInformation($"✅ Successfully seeded {definitions.Count} workflow definitions");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error seeding workflow definitions");
            throw;
        }
    }

    #region NCA ECC Assessment (8 Steps)

    /// <summary>
    /// NCA ECC Assessment Workflow (Network & Information Security)
    /// Steps: Start → Define Scope → Assess Controls → Gap Analysis → Risk Evaluation → 
    ///        Remediation Plan → Compliance Report → Complete
    /// </summary>
    private static WorkflowDefinition CreateNcaEccAssessment()
    {
        var steps = new List<WorkflowStepDefinition>
        {
            new() { id = "start", name = "Start Assessment", type = "startEvent", stepNumber = 1 },
            new() { id = "scopeDefinition", name = "Define Scope", type = "userTask", stepNumber = 2, assignee = "RiskManager", daysToComplete = 3, description = "Define assessment scope and boundaries" },
            new() { id = "controlAssessment", name = "Assess Controls", type = "userTask", stepNumber = 3, assignee = "RiskManager", daysToComplete = 5, description = "Evaluate existing security controls" },
            new() { id = "gapAnalysis", name = "Gap Analysis", type = "userTask", stepNumber = 4, assignee = "ComplianceOfficer", daysToComplete = 4, description = "Identify gaps in controls" },
            new() { id = "riskEvaluation", name = "Risk Evaluation", type = "userTask", stepNumber = 5, assignee = "ComplianceOfficer", daysToComplete = 3, description = "Evaluate and rank risks" },
            new() { id = "remediation", name = "Remediation Plan", type = "userTask", stepNumber = 6, assignee = "RiskManager", daysToComplete = 5, description = "Develop remediation plan" },
            new() { id = "report", name = "Compliance Report", type = "userTask", stepNumber = 7, assignee = "ComplianceOfficer", daysToComplete = 2, description = "Document findings and recommendations" },
            new() { id = "end", name = "Assessment Complete", type = "endEvent", stepNumber = 8 }
        };

        return new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            WorkflowNumber = "WF-NCA-ECC-001",
            Name = "NCA ECC Assessment",
            Category = "Assessment",
            Framework = "NCA",
            Type = "Assessment",
            Status = "Active",
            TriggerType = "Manual",
            DefaultAssignee = "RiskManager",
            Steps = JsonSerializer.Serialize(steps),
            BpmnXml = GenerateNcaEccBpmn(),
            TenantId = null // Global template
        };
    }

    private static string GenerateNcaEccBpmn()
    {
        return @"
<?xml version='1.0' encoding='UTF-8'?>
<bpmn:definitions xmlns:bpmn='http://www.omg.org/spec/BPMN/20100524/MODEL' id='Definition_NcaEcc'>
  <bpmn:process id='Process_NcaEcc' name='NCA ECC Assessment'>
    <bpmn:startEvent id='start' name='Start Assessment' />
    <bpmn:userTask id='scopeDefinition' name='Define Scope' />
    <bpmn:userTask id='controlAssessment' name='Assess Controls' />
    <bpmn:userTask id='gapAnalysis' name='Gap Analysis' />
    <bpmn:userTask id='riskEvaluation' name='Risk Evaluation' />
    <bpmn:userTask id='remediation' name='Remediation Plan' />
    <bpmn:userTask id='report' name='Compliance Report' />
    <bpmn:endEvent id='end' name='Assessment Complete' />
  </bpmn:process>
</bpmn:definitions>";
    }

    #endregion

    #region SAMA CSF Assessment (7 Steps)

    /// <summary>
    /// SAMA CSF Assessment Workflow (Cybersecurity Framework)
    /// Steps: Start → Governance Assessment → Risk Management → Incident Response → 
    ///        Operational Resilience → Compliance Reporting → Complete
    /// </summary>
    private static WorkflowDefinition CreateSamaCsfAssessment()
    {
        var steps = new List<WorkflowStepDefinition>
        {
            new() { id = "start", name = "Start Cyber Assessment", type = "startEvent", stepNumber = 1 },
            new() { id = "governance", name = "Governance Assessment", type = "userTask", stepNumber = 2, assignee = "ComplianceOfficer", daysToComplete = 4, description = "Assess governance framework" },
            new() { id = "riskMgmt", name = "Risk Management", type = "userTask", stepNumber = 3, assignee = "RiskManager", daysToComplete = 4, description = "Evaluate risk management processes" },
            new() { id = "incidentResponse", name = "Incident Response", type = "userTask", stepNumber = 4, assignee = "SecurityManager", daysToComplete = 3, description = "Review incident response capabilities" },
            new() { id = "resilience", name = "Operational Resilience", type = "userTask", stepNumber = 5, assignee = "ITManager", daysToComplete = 4, description = "Assess operational resilience" },
            new() { id = "compliance", name = "Compliance Reporting", type = "userTask", stepNumber = 6, assignee = "ComplianceOfficer", daysToComplete = 3, description = "Prepare compliance report" },
            new() { id = "end", name = "Assessment Complete", type = "endEvent", stepNumber = 7 }
        };

        return new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            WorkflowNumber = "WF-SAMA-CSF-001",
            Name = "SAMA CSF Assessment",
            Category = "Assessment",
            Framework = "SAMA",
            Type = "Assessment",
            Status = "Active",
            TriggerType = "Manual",
            DefaultAssignee = "ComplianceOfficer",
            Steps = JsonSerializer.Serialize(steps),
            BpmnXml = GenerateSamaCsfBpmn(),
            TenantId = null // Global template
        };
    }

    private static string GenerateSamaCsfBpmn()
    {
        return @"
<?xml version='1.0' encoding='UTF-8'?>
<bpmn:definitions xmlns:bpmn='http://www.omg.org/spec/BPMN/20100524/MODEL' id='Definition_SamaCsf'>
  <bpmn:process id='Process_SamaCsf' name='SAMA CSF Assessment'>
    <bpmn:startEvent id='start' name='Start Cyber Assessment' />
    <bpmn:userTask id='governance' name='Governance Assessment' />
    <bpmn:userTask id='riskMgmt' name='Risk Management' />
    <bpmn:userTask id='incidentResponse' name='Incident Response' />
    <bpmn:userTask id='resilience' name='Operational Resilience' />
    <bpmn:userTask id='compliance' name='Compliance Reporting' />
    <bpmn:endEvent id='end' name='Assessment Complete' />
  </bpmn:process>
</bpmn:definitions>";
    }

    #endregion

    #region PDPL PIA (9 Steps)

    /// <summary>
    /// PDPL Privacy Impact Assessment Workflow
    /// Steps: Start → Data Mapping → Legal Basis → Risk Assessment → Safeguards → 
    ///        Consent Management → Rights Management → Documentation → Complete
    /// </summary>
    private static WorkflowDefinition CreatePdplPia()
    {
        var steps = new List<WorkflowStepDefinition>
        {
            new() { id = "start", name = "Start Privacy Assessment", type = "startEvent", stepNumber = 1 },
            new() { id = "dataMapping", name = "Data Mapping", type = "userTask", stepNumber = 2, assignee = "PrivacyOfficer", daysToComplete = 4, description = "Map all personal data flows" },
            new() { id = "legalBasis", name = "Legal Basis", type = "userTask", stepNumber = 3, assignee = "LegalOfficer", daysToComplete = 5, description = "Establish legal basis for processing" },
            new() { id = "riskAssessment", name = "Privacy Risk Assessment", type = "userTask", stepNumber = 4, assignee = "PrivacyOfficer", daysToComplete = 5, description = "Assess privacy risks" },
            new() { id = "safeguards", name = "Safeguards", type = "userTask", stepNumber = 5, assignee = "SecurityManager", daysToComplete = 3, description = "Design privacy safeguards" },
            new() { id = "consentMgmt", name = "Consent Management", type = "userTask", stepNumber = 6, assignee = "PrivacyOfficer", daysToComplete = 3, description = "Define consent management" },
            new() { id = "rightsManagement", name = "Rights Management", type = "userTask", stepNumber = 7, assignee = "LegalOfficer", daysToComplete = 2, description = "Define rights management process" },
            new() { id = "documentation", name = "Documentation", type = "userTask", stepNumber = 8, assignee = "PrivacyOfficer", daysToComplete = 2, description = "Complete PIA documentation" },
            new() { id = "end", name = "Privacy Assessment Complete", type = "endEvent", stepNumber = 9 }
        };

        return new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            WorkflowNumber = "WF-PDPL-PIA-001",
            Name = "PDPL Privacy Impact Assessment",
            Category = "Assessment",
            Framework = "PDPL",
            Type = "Assessment",
            Status = "Active",
            TriggerType = "Manual",
            DefaultAssignee = "PrivacyOfficer",
            Steps = JsonSerializer.Serialize(steps),
            BpmnXml = GeneratePdplPiaBpmn(),
            TenantId = null // Global template
        };
    }

    private static string GeneratePdplPiaBpmn()
    {
        return @"
<?xml version='1.0' encoding='UTF-8'?>
<bpmn:definitions xmlns:bpmn='http://www.omg.org/spec/BPMN/20100524/MODEL' id='Definition_PdplPia'>
  <bpmn:process id='Process_PdplPia' name='PDPL Privacy Impact Assessment'>
    <bpmn:startEvent id='start' name='Start Privacy Assessment' />
    <bpmn:userTask id='dataMapping' name='Data Mapping' />
    <bpmn:userTask id='legalBasis' name='Legal Basis' />
    <bpmn:userTask id='riskAssessment' name='Privacy Risk Assessment' />
    <bpmn:userTask id='safeguards' name='Safeguards' />
    <bpmn:userTask id='consentMgmt' name='Consent Management' />
    <bpmn:userTask id='rightsManagement' name='Rights Management' />
    <bpmn:userTask id='documentation' name='Documentation' />
    <bpmn:endEvent id='end' name='Privacy Assessment Complete' />
  </bpmn:process>
</bpmn:definitions>";
    }

    #endregion

    #region ERM (7 Steps)

    /// <summary>
    /// Enterprise Risk Management Workflow
    /// Steps: Start → Risk Identification → Risk Analysis → Risk Evaluation → 
    ///        Risk Treatment → Monitoring & Review → Complete
    /// </summary>
    private static WorkflowDefinition CreateErm()
    {
        var steps = new List<WorkflowStepDefinition>
        {
            new() { id = "start", name = "Start Risk Assessment", type = "startEvent", stepNumber = 1 },
            new() { id = "identification", name = "Risk Identification", type = "userTask", stepNumber = 2, assignee = "RiskManager", daysToComplete = 4, description = "Identify enterprise risks" },
            new() { id = "analysis", name = "Risk Analysis", type = "userTask", stepNumber = 3, assignee = "RiskManager", daysToComplete = 5, description = "Analyze risk impact and likelihood" },
            new() { id = "evaluation", name = "Risk Evaluation", type = "userTask", stepNumber = 4, assignee = "RiskManager", daysToComplete = 3, description = "Evaluate and prioritize risks" },
            new() { id = "treatment", name = "Risk Treatment", type = "userTask", stepNumber = 5, assignee = "RiskManager", daysToComplete = 5, description = "Develop risk treatment plan" },
            new() { id = "monitoring", name = "Monitoring & Review", type = "userTask", stepNumber = 6, assignee = "RiskManager", daysToComplete = 2, description = "Establish monitoring process" },
            new() { id = "end", name = "Risk Assessment Complete", type = "endEvent", stepNumber = 7 }
        };

        return new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            WorkflowNumber = "WF-ERM-001",
            Name = "Enterprise Risk Management",
            Category = "Assessment",
            Framework = "ERM",
            Type = "Assessment",
            Status = "Active",
            TriggerType = "Manual",
            DefaultAssignee = "RiskManager",
            Steps = JsonSerializer.Serialize(steps),
            BpmnXml = GenerateErmBpmn(),
            TenantId = null // Global template
        };
    }

    private static string GenerateErmBpmn()
    {
        return @"
<?xml version='1.0' encoding='UTF-8'?>
<bpmn:definitions xmlns:bpmn='http://www.omg.org/spec/BPMN/20100524/MODEL' id='Definition_Erm'>
  <bpmn:process id='Process_Erm' name='Enterprise Risk Management'>
    <bpmn:startEvent id='start' name='Start Risk Assessment' />
    <bpmn:userTask id='identification' name='Risk Identification' />
    <bpmn:userTask id='analysis' name='Risk Analysis' />
    <bpmn:userTask id='evaluation' name='Risk Evaluation' />
    <bpmn:userTask id='treatment' name='Risk Treatment' />
    <bpmn:userTask id='monitoring' name='Monitoring & Review' />
    <bpmn:endEvent id='end' name='Risk Assessment Complete' />
  </bpmn:process>
</bpmn:definitions>";
    }

    #endregion

    #region Evidence Review & Approval (5 Steps)

    /// <summary>
    /// Evidence Review & Approval Workflow (Internal)
    /// Steps: Start → Submitted → Initial Review → Technical Review → Final Approval → Approved
    /// Approval chain: Compliance Officer → SME/Specialist → Audit Manager
    /// </summary>
    private static WorkflowDefinition CreateEvidenceReviewAndApproval()
    {
        var steps = new List<WorkflowStepDefinition>
        {
            new() { id = "start", name = "Evidence Submitted", type = "startEvent", stepNumber = 1 },
            new() { id = "initialReview", name = "Initial Review", type = "userTask", stepNumber = 2, assignee = "ComplianceOfficer", daysToComplete = 2, description = "Initial evidence validation and completeness check" },
            new() { id = "technicalReview", name = "Technical Review", type = "userTask", stepNumber = 3, assignee = "SME", daysToComplete = 3, description = "Technical assessment and verification" },
            new() { id = "finalApproval", name = "Final Approval", type = "userTask", stepNumber = 4, assignee = "AuditManager", daysToComplete = 1, description = "Final approval and sign-off" },
            new() { id = "end", name = "Evidence Approved", type = "endEvent", stepNumber = 5 }
        };

        return new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            WorkflowNumber = "WF-EVIDENCE-001",
            Name = "Evidence Review & Approval",
            Category = "Approval",
            Framework = "Internal",
            Type = "Approval",
            Status = "Active",
            TriggerType = "Manual",
            DefaultAssignee = "ComplianceOfficer",
            Steps = JsonSerializer.Serialize(steps),
            BpmnXml = GenerateEvidenceReviewBpmn(),
            TenantId = null // Global template
        };
    }

    private static string GenerateEvidenceReviewBpmn()
    {
        return @"
<?xml version='1.0' encoding='UTF-8'?>
<bpmn:definitions xmlns:bpmn='http://www.omg.org/spec/BPMN/20100524/MODEL' id='Definition_EvidenceReview'>
  <bpmn:process id='Process_EvidenceReview' name='Evidence Review & Approval'>
    <bpmn:startEvent id='start' name='Evidence Submitted' />
    <bpmn:userTask id='initialReview' name='Initial Review' />
    <bpmn:userTask id='technicalReview' name='Technical Review' />
    <bpmn:userTask id='finalApproval' name='Final Approval' />
    <bpmn:endEvent id='end' name='Evidence Approved' />
  </bpmn:process>
</bpmn:definitions>";
    }

    #endregion

    #region Audit Finding Remediation (7 Steps)

    /// <summary>
    /// Audit Finding Remediation Workflow
    /// Steps: Start → Risk Assessment → Planning → Implementation → Validation → Closure → Closed
    /// </summary>
    private static WorkflowDefinition CreateAuditFindingRemediation()
    {
        var steps = new List<WorkflowStepDefinition>
        {
            new() { id = "start", name = "Finding Identified", type = "startEvent", stepNumber = 1 },
            new() { id = "riskAssess", name = "Risk Assessment", type = "userTask", stepNumber = 2, assignee = "RiskManager", daysToComplete = 2, description = "Assess finding severity and impact" },
            new() { id = "planning", name = "Remediation Planning", type = "userTask", stepNumber = 3, assignee = "ProcessOwner", daysToComplete = 5, description = "Develop remediation plan" },
            new() { id = "implementation", name = "Implementation", type = "userTask", stepNumber = 4, assignee = "ImplementationTeam", daysToComplete = 10, description = "Execute remediation activities" },
            new() { id = "validation", name = "Validation Testing", type = "userTask", stepNumber = 5, assignee = "QualityAssurance", daysToComplete = 3, description = "Test remediation effectiveness" },
            new() { id = "closure", name = "Closure Review", type = "userTask", stepNumber = 6, assignee = "AuditManager", daysToComplete = 2, description = "Final closure review and approval" },
            new() { id = "end", name = "Finding Closed", type = "endEvent", stepNumber = 7 }
        };

        return new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            WorkflowNumber = "WF-FINDING-REMEDIATION-001",
            Name = "Audit Finding Remediation",
            Category = "Remediation",
            Framework = "Audit",
            Type = "Remediation",
            Status = "Active",
            TriggerType = "Manual",
            DefaultAssignee = "RiskManager",
            Steps = JsonSerializer.Serialize(steps),
            BpmnXml = GenerateFindingRemediationBpmn(),
            TenantId = null // Global template
        };
    }

    private static string GenerateFindingRemediationBpmn()
    {
        return @"
<?xml version='1.0' encoding='UTF-8'?>
<bpmn:definitions xmlns:bpmn='http://www.omg.org/spec/BPMN/20100524/MODEL' id='Definition_FindingRemediation'>
  <bpmn:process id='Process_FindingRemediation' name='Audit Finding Remediation'>
    <bpmn:startEvent id='start' name='Finding Identified' />
    <bpmn:userTask id='riskAssess' name='Risk Assessment' />
    <bpmn:userTask id='planning' name='Remediation Planning' />
    <bpmn:userTask id='implementation' name='Implementation' />
    <bpmn:userTask id='validation' name='Validation Testing' />
    <bpmn:userTask id='closure' name='Closure Review' />
    <bpmn:endEvent id='end' name='Finding Closed' />
  </bpmn:process>
</bpmn:definitions>";
    }

    #endregion

    #region Policy Review & Publication (7 Steps)

    /// <summary>
    /// Policy Review & Publication Workflow
    /// Steps: Start → Draft → Legal Review → Compliance → Executive → Publication → Acknowledgment → Active
    /// </summary>
    private static WorkflowDefinition CreatePolicyReviewAndPublication()
    {
        var steps = new List<WorkflowStepDefinition>
        {
            new() { id = "start", name = "Policy Draft Created", type = "startEvent", stepNumber = 1 },
            new() { id = "legalReview", name = "Legal Review", type = "userTask", stepNumber = 2, assignee = "LegalOfficer", daysToComplete = 5, description = "Legal assessment and review" },
            new() { id = "complianceCheck", name = "Compliance Check", type = "userTask", stepNumber = 3, assignee = "ComplianceOfficer", daysToComplete = 3, description = "Compliance framework alignment check" },
            new() { id = "executiveApproval", name = "Executive Approval", type = "userTask", stepNumber = 4, assignee = "Director", daysToComplete = 2, description = "Executive leadership approval" },
            new() { id = "publication", name = "Publication", type = "userTask", stepNumber = 5, assignee = "Communications", daysToComplete = 1, description = "Publish policy to organization" },
            new() { id = "acknowledgment", name = "Staff Acknowledgment", type = "userTask", stepNumber = 6, assignee = "AllStaff", daysToComplete = 14, description = "Require staff acknowledgment" },
            new() { id = "end", name = "Policy Active", type = "endEvent", stepNumber = 7 }
        };

        return new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            WorkflowNumber = "WF-POLICY-001",
            Name = "Policy Review & Publication",
            Category = "Governance",
            Framework = "Compliance",
            Type = "Policy",
            Status = "Active",
            TriggerType = "Manual",
            DefaultAssignee = "LegalOfficer",
            Steps = JsonSerializer.Serialize(steps),
            BpmnXml = GeneratePolicyReviewBpmn(),
            TenantId = null // Global template
        };
    }

    private static string GeneratePolicyReviewBpmn()
    {
        return @"
<?xml version='1.0' encoding='UTF-8'?>
<bpmn:definitions xmlns:bpmn='http://www.omg.org/spec/BPMN/20100524/MODEL' id='Definition_PolicyReview'>
  <bpmn:process id='Process_PolicyReview' name='Policy Review & Publication'>
    <bpmn:startEvent id='start' name='Policy Draft Created' />
    <bpmn:userTask id='legalReview' name='Legal Review' />
    <bpmn:userTask id='complianceCheck' name='Compliance Check' />
    <bpmn:userTask id='executiveApproval' name='Executive Approval' />
    <bpmn:userTask id='publication' name='Publication' />
    <bpmn:userTask id='acknowledgment' name='Staff Acknowledgment' />
    <bpmn:endEvent id='end' name='Policy Active' />
  </bpmn:process>
</bpmn:definitions>";
    }

    #endregion
}

/// <summary>BPMN Step Definition (JSON serializable)</summary>
public class WorkflowStepDefinition
{
    public string id { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public string type { get; set; } = string.Empty; // startEvent, userTask, endEvent
    public int stepNumber { get; set; }
    public string? assignee { get; set; }
    public int? daysToComplete { get; set; }
    public string? description { get; set; }
}

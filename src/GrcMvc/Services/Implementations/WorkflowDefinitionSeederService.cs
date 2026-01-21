using GrcMvc.Data;
using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Seeds the 7 pre-defined workflow definitions with BPMN structure
    /// Based on KSA GRC requirements: NCA ECC, SAMA CSF, PDPL PIA, ERM, Evidence, Audit, Policy
    /// </summary>
    public class WorkflowDefinitionSeederService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<WorkflowDefinitionSeederService> _logger;

        public WorkflowDefinitionSeederService(GrcDbContext context, ILogger<WorkflowDefinitionSeederService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAllWorkflowDefinitionsAsync()
        {
            if (await _context.WorkflowDefinitions.AnyAsync())
            {
                _logger.LogInformation("Workflow definitions already seeded, skipping...");
                return;
            }

            var definitions = new List<WorkflowDefinition>
            {
                CreateNcaEccAssessmentWorkflow(),
                CreateSamaCsfAssessmentWorkflow(),
                CreatePdplPiaWorkflow(),
                CreateErmWorkflow(),
                CreateEvidenceReviewWorkflow(),
                CreateAuditFindingRemediationWorkflow(),
                CreatePolicyReviewWorkflow()
            };

            await _context.WorkflowDefinitions.AddRangeAsync(definitions);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Seeded {Count} workflow definitions", definitions.Count);
        }

        /// <summary>
        /// 1️⃣ NCA Essential Cybersecurity Controls Assessment (8 steps)
        /// </summary>
        private WorkflowDefinition CreateNcaEccAssessmentWorkflow()
        {
            var steps = new[]
            {
                new WorkflowStepDefinition
                {
                    StepNo = 1,
                    StepId = "start",
                    StepName = "Start Assessment",
                    StepType = "Start",
                    BpmnType = "startEvent",
                    Trigger = "Manual",
                    ActorRoleCode = "COMPLIANCE_OFFICER",
                    AssigneeRule = "ByInitiator",
                    UIPageRoute = "~/Workflows",
                    ApiEndpoint = "POST /api/workflow/start",
                    StatusIn = "None",
                    StatusOut = "Pending",
                    SlaDays = 0,
                    PermissionsRequired = "Grc.Workflows.Execute",
                    Description = "Initiate NCA ECC assessment workflow"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 2,
                    StepId = "scopeDefinition",
                    StepName = "Define Scope",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "COMPLIANCE_OFFICER",
                    AssigneeRule = "ByTenantRole",
                    UIPageRoute = "~/Assessments/Create",
                    ApiEndpoint = "POST /api/assessment",
                    StatusIn = "Pending",
                    StatusOut = "InProgress",
                    SlaDays = 3,
                    PermissionsRequired = "Grc.Assessments.Create",
                    Description = "Define assessment scope and select applicable controls"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 3,
                    StepId = "controlAssessment",
                    StepName = "Assess Controls",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "CONTROL_OWNER",
                    AssigneeRule = "ByControlOwner",
                    UIPageRoute = "~/ControlAssessments",
                    ApiEndpoint = "POST /api/control-assessment/bulk",
                    StatusIn = "InProgress",
                    StatusOut = "InProgress",
                    SlaDays = 7,
                    PermissionsRequired = "Grc.ControlAssessments.Assess",
                    Description = "Assess each control against ECC requirements"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 4,
                    StepId = "gapAnalysis",
                    StepName = "Gap Analysis",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "COMPLIANCE_OFFICER",
                    AssigneeRule = "ByDepartment",
                    UIPageRoute = "~/Assessments/Details/{id}",
                    ApiEndpoint = "PUT /api/assessment/{id}/gaps",
                    StatusIn = "InProgress",
                    StatusOut = "InProgress",
                    SlaDays = 2,
                    PermissionsRequired = "Grc.Assessments.Edit",
                    Description = "Analyze gaps between current state and requirements"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 5,
                    StepId = "riskEvaluation",
                    StepName = "Risk Evaluation",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "RISK_MANAGER",
                    AssigneeRule = "ByTenantRole",
                    UIPageRoute = "~/Risks/Create",
                    ApiEndpoint = "POST /api/risk/from-gaps",
                    StatusIn = "InProgress",
                    StatusOut = "InProgress",
                    SlaDays = 5,
                    PermissionsRequired = "Grc.Risks.Create",
                    Description = "Evaluate risks associated with identified gaps"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 6,
                    StepId = "remediation",
                    StepName = "Remediation Plan",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "ACTION_OWNER",
                    AssigneeRule = "ByAssignee",
                    UIPageRoute = "~/ActionPlans",
                    ApiEndpoint = "POST /api/action-plan/bulk",
                    StatusIn = "InProgress",
                    StatusOut = "InProgress",
                    SlaDays = 3,
                    PermissionsRequired = "Grc.ActionPlans.Create",
                    Description = "Create remediation action plans for gaps"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 7,
                    StepId = "report",
                    StepName = "Compliance Report",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "COMPLIANCE_OFFICER",
                    AssigneeRule = "ByInitiator",
                    UIPageRoute = "~/Reports/Compliance",
                    ApiEndpoint = "POST /api/report/compliance",
                    StatusIn = "InProgress",
                    StatusOut = "Approved",
                    SlaDays = 2,
                    PermissionsRequired = "Grc.Reports.Generate",
                    Description = "Generate final compliance report"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 8,
                    StepId = "end",
                    StepName = "Assessment Complete",
                    StepType = "End",
                    BpmnType = "endEvent",
                    Trigger = "Automatic",
                    ActorRoleCode = "SYSTEM",
                    AssigneeRule = "N/A",
                    UIPageRoute = "~/Assessments/Details/{id}",
                    ApiEndpoint = "",
                    StatusIn = "Approved",
                    StatusOut = "Completed",
                    SlaDays = 0,
                    PermissionsRequired = "",
                    Description = "Assessment workflow completed"
                }
            };

            return new WorkflowDefinition
            {
                Id = Guid.NewGuid(),
                WorkflowNumber = "WF-NCA-ECC-001",
                Name = "NCA ECC Assessment",
                Description = "NCA Essential Cybersecurity Controls Assessment Workflow (109 controls)",
                WorkflowType = "Assessment",
                Category = "Compliance",
                Version = "1.0",
                Status = "Active",
                IsActive = true,
                DefaultAssigneeRoleCode = "COMPLIANCE_OFFICER",
                TotalSteps = steps.Length,
                EstimatedDays = 22,
                StepsJson = JsonSerializer.Serialize(steps),
                BpmnXml = GenerateBpmnXml("NCA_ECC_Assessment", steps),
            };
        }

        /// <summary>
        /// 2️⃣ SAMA Cybersecurity Framework Assessment (7 steps)
        /// </summary>
        private WorkflowDefinition CreateSamaCsfAssessmentWorkflow()
        {
            var steps = new[]
            {
                new WorkflowStepDefinition
                {
                    StepNo = 1,
                    StepId = "start",
                    StepName = "Start Cyber Assessment",
                    StepType = "Start",
                    BpmnType = "startEvent",
                    Trigger = "Manual",
                    ActorRoleCode = "GRC_MANAGER",
                    AssigneeRule = "ByInitiator",
                    UIPageRoute = "~/Workflows",
                    ApiEndpoint = "POST /api/workflow/start",
                    StatusIn = "None",
                    StatusOut = "Pending",
                    SlaDays = 0,
                    PermissionsRequired = "Grc.Workflows.Execute",
                    Description = "Initiate SAMA CSF assessment"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 2,
                    StepId = "governance",
                    StepName = "Governance Assessment",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "GRC_MANAGER",
                    AssigneeRule = "ByTenantRole",
                    UIPageRoute = "~/Assessments/Create",
                    ApiEndpoint = "POST /api/assessment",
                    StatusIn = "Pending",
                    StatusOut = "InProgress",
                    SlaDays = 5,
                    PermissionsRequired = "Grc.Assessments.Create",
                    Description = "Assess governance controls per SAMA framework"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 3,
                    StepId = "riskMgmt",
                    StepName = "Risk Management",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "RISK_MANAGER",
                    AssigneeRule = "ByDepartment",
                    UIPageRoute = "~/Risks",
                    ApiEndpoint = "POST /api/risk",
                    StatusIn = "InProgress",
                    StatusOut = "InProgress",
                    SlaDays = 7,
                    PermissionsRequired = "Grc.Risks.Create",
                    Description = "Assess risk management controls"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 4,
                    StepId = "incidentResponse",
                    StepName = "Incident Response",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "SECURITY_OFFICER",
                    AssigneeRule = "ByRole",
                    UIPageRoute = "~/Incidents",
                    ApiEndpoint = "POST /api/incident/review",
                    StatusIn = "InProgress",
                    StatusOut = "InProgress",
                    SlaDays = 3,
                    PermissionsRequired = "Grc.Incidents.Manage",
                    Description = "Review incident response capabilities"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 5,
                    StepId = "resilience",
                    StepName = "Operational Resilience",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "OPERATIONS_MANAGER",
                    AssigneeRule = "ByDepartment",
                    UIPageRoute = "~/Resilience",
                    ApiEndpoint = "POST /api/resilience/assess",
                    StatusIn = "InProgress",
                    StatusOut = "InProgress",
                    SlaDays = 5,
                    PermissionsRequired = "Grc.Resilience.Assess",
                    Description = "Assess operational resilience and BCP"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 6,
                    StepId = "compliance",
                    StepName = "Compliance Reporting",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "COMPLIANCE_OFFICER",
                    AssigneeRule = "ByInitiator",
                    UIPageRoute = "~/Reports/Banking",
                    ApiEndpoint = "POST /api/report/sama",
                    StatusIn = "InProgress",
                    StatusOut = "Approved",
                    SlaDays = 3,
                    PermissionsRequired = "Grc.Reports.Generate",
                    Description = "Generate SAMA compliance report"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 7,
                    StepId = "end",
                    StepName = "Assessment Complete",
                    StepType = "End",
                    BpmnType = "endEvent",
                    Trigger = "Automatic",
                    ActorRoleCode = "SYSTEM",
                    AssigneeRule = "N/A",
                    UIPageRoute = "~/Assessments/Details/{id}",
                    ApiEndpoint = "",
                    StatusIn = "Approved",
                    StatusOut = "Completed",
                    SlaDays = 0,
                    PermissionsRequired = "",
                    Description = "SAMA assessment completed"
                }
            };

            return new WorkflowDefinition
            {
                Id = Guid.NewGuid(),
                WorkflowNumber = "WF-SAMA-CSF-001",
                Name = "SAMA CSF Assessment",
                Description = "SAMA Cybersecurity Framework Assessment for Financial Institutions",
                WorkflowType = "Assessment",
                Category = "Financial",
                Version = "1.0",
                Status = "Active",
                IsActive = true,
                DefaultAssigneeRoleCode = "GRC_MANAGER",
                TotalSteps = steps.Length,
                EstimatedDays = 23,
                StepsJson = JsonSerializer.Serialize(steps),
                BpmnXml = GenerateBpmnXml("SAMA_CSF_Assessment", steps),
            };
        }

        /// <summary>
        /// 3️⃣ PDPL Privacy Impact Assessment (9 steps)
        /// </summary>
        private WorkflowDefinition CreatePdplPiaWorkflow()
        {
            var steps = new[]
            {
                new WorkflowStepDefinition
                {
                    StepNo = 1,
                    StepId = "start",
                    StepName = "Start Privacy Assessment",
                    StepType = "Start",
                    BpmnType = "startEvent",
                    Trigger = "Manual",
                    ActorRoleCode = "DPO",
                    AssigneeRule = "ByInitiator",
                    UIPageRoute = "~/Workflows",
                    ApiEndpoint = "POST /api/workflow/start",
                    StatusIn = "None",
                    StatusOut = "Pending",
                    SlaDays = 0,
                    PermissionsRequired = "Grc.Workflows.Execute",
                    Description = "Initiate PDPL Privacy Impact Assessment"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 2,
                    StepId = "dataMapping",
                    StepName = "Data Mapping",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "PRIVACY_ANALYST",
                    AssigneeRule = "ByRole",
                    UIPageRoute = "~/DataMapping",
                    ApiEndpoint = "POST /api/data-mapping",
                    StatusIn = "Pending",
                    StatusOut = "InProgress",
                    SlaDays = 5,
                    PermissionsRequired = "Grc.DataPrivacy.Map",
                    Description = "Map personal data flows and processing activities"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 3,
                    StepId = "legalBasis",
                    StepName = "Legal Basis",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "LEGAL_COUNSEL",
                    AssigneeRule = "ByDepartment",
                    UIPageRoute = "~/LegalBasis",
                    ApiEndpoint = "POST /api/legal-basis",
                    StatusIn = "InProgress",
                    StatusOut = "InProgress",
                    SlaDays = 3,
                    PermissionsRequired = "Grc.Legal.Review",
                    Description = "Determine legal basis for each processing activity"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 4,
                    StepId = "riskAssessment",
                    StepName = "Privacy Risk Assessment",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "RISK_MANAGER",
                    AssigneeRule = "ByRole",
                    UIPageRoute = "~/Risks/Privacy",
                    ApiEndpoint = "POST /api/risk/privacy",
                    StatusIn = "InProgress",
                    StatusOut = "InProgress",
                    SlaDays = 7,
                    PermissionsRequired = "Grc.Risks.Create",
                    Description = "Assess privacy risks for data processing"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 5,
                    StepId = "safeguards",
                    StepName = "Safeguards",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "SECURITY_OFFICER",
                    AssigneeRule = "ByRole",
                    UIPageRoute = "~/Safeguards",
                    ApiEndpoint = "POST /api/safeguards",
                    StatusIn = "InProgress",
                    StatusOut = "InProgress",
                    SlaDays = 5,
                    PermissionsRequired = "Grc.Security.Implement",
                    Description = "Define technical and organizational safeguards"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 6,
                    StepId = "consentMgmt",
                    StepName = "Consent Management",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "DPO",
                    AssigneeRule = "ByInitiator",
                    UIPageRoute = "~/Consent",
                    ApiEndpoint = "POST /api/consent",
                    StatusIn = "InProgress",
                    StatusOut = "InProgress",
                    SlaDays = 3,
                    PermissionsRequired = "Grc.Consent.Manage",
                    Description = "Define consent collection and management processes"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 7,
                    StepId = "rightsManagement",
                    StepName = "Rights Management",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "DPO",
                    AssigneeRule = "ByInitiator",
                    UIPageRoute = "~/DataRights",
                    ApiEndpoint = "POST /api/data-rights",
                    StatusIn = "InProgress",
                    StatusOut = "InProgress",
                    SlaDays = 2,
                    PermissionsRequired = "Grc.DataRights.Configure",
                    Description = "Configure data subject rights processes"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 8,
                    StepId = "documentation",
                    StepName = "Documentation",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "COMPLIANCE_OFFICER",
                    AssigneeRule = "ByRole",
                    UIPageRoute = "~/Reports/Privacy",
                    ApiEndpoint = "POST /api/report/pia",
                    StatusIn = "InProgress",
                    StatusOut = "Approved",
                    SlaDays = 3,
                    PermissionsRequired = "Grc.Reports.Generate",
                    Description = "Generate PIA documentation and report"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 9,
                    StepId = "end",
                    StepName = "Assessment Complete",
                    StepType = "End",
                    BpmnType = "endEvent",
                    Trigger = "Automatic",
                    ActorRoleCode = "SYSTEM",
                    AssigneeRule = "N/A",
                    UIPageRoute = "~/DataPrivacy/Reports/{id}",
                    ApiEndpoint = "",
                    StatusIn = "Approved",
                    StatusOut = "Completed",
                    SlaDays = 0,
                    PermissionsRequired = "",
                    Description = "Privacy Impact Assessment completed"
                }
            };

            return new WorkflowDefinition
            {
                Id = Guid.NewGuid(),
                WorkflowNumber = "WF-PDPL-PIA-001",
                Name = "PDPL Privacy Impact Assessment",
                Description = "PDPL Privacy Impact Assessment Workflow for SDAIA Compliance",
                WorkflowType = "Assessment",
                Category = "Privacy",
                Version = "1.0",
                Status = "Active",
                IsActive = true,
                DefaultAssigneeRoleCode = "DPO",
                TotalSteps = steps.Length,
                EstimatedDays = 28,
                StepsJson = JsonSerializer.Serialize(steps),
                BpmnXml = GenerateBpmnXml("PDPL_PIA", steps),
            };
        }

        /// <summary>
        /// 4️⃣ Enterprise Risk Management Assessment (7 steps)
        /// </summary>
        private WorkflowDefinition CreateErmWorkflow()
        {
            var steps = new[]
            {
                new WorkflowStepDefinition
                {
                    StepNo = 1,
                    StepId = "start",
                    StepName = "Start Risk Assessment",
                    StepType = "Start",
                    BpmnType = "startEvent",
                    Trigger = "Manual",
                    ActorRoleCode = "RISK_MANAGER",
                    AssigneeRule = "ByInitiator",
                    UIPageRoute = "~/Workflows",
                    ApiEndpoint = "POST /api/workflow/start",
                    StatusIn = "None",
                    StatusOut = "Pending",
                    SlaDays = 0,
                    PermissionsRequired = "Grc.Workflows.Execute",
                    Description = "Initiate Enterprise Risk Assessment"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 2,
                    StepId = "identification",
                    StepName = "Risk Identification",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "RISK_ANALYST",
                    AssigneeRule = "ByDepartment",
                    UIPageRoute = "~/Risks/Identify",
                    ApiEndpoint = "POST /api/risk/identify",
                    StatusIn = "Pending",
                    StatusOut = "InProgress",
                    SlaDays = 7,
                    PermissionsRequired = "Grc.Risks.Create",
                    Description = "Identify and document risks"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 3,
                    StepId = "analysis",
                    StepName = "Risk Analysis",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "RISK_ANALYST",
                    AssigneeRule = "ByRiskOwner",
                    UIPageRoute = "~/Risks/Analysis",
                    ApiEndpoint = "PUT /api/risk/{id}/analyze",
                    StatusIn = "InProgress",
                    StatusOut = "InProgress",
                    SlaDays = 5,
                    PermissionsRequired = "Grc.Risks.Edit",
                    Description = "Analyze risk likelihood and impact"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 4,
                    StepId = "evaluation",
                    StepName = "Risk Evaluation",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "RISK_MANAGER",
                    AssigneeRule = "ByRole",
                    UIPageRoute = "~/Risks/HeatMap",
                    ApiEndpoint = "PUT /api/risk/{id}/evaluate",
                    StatusIn = "InProgress",
                    StatusOut = "InProgress",
                    SlaDays = 3,
                    PermissionsRequired = "Grc.Risks.Assess",
                    Description = "Evaluate and prioritize risks"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 5,
                    StepId = "treatment",
                    StepName = "Risk Treatment",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "CONTROL_OWNER",
                    AssigneeRule = "ByControlOwner",
                    UIPageRoute = "~/Risks/Treatment",
                    ApiEndpoint = "POST /api/risk/{id}/treatment",
                    StatusIn = "InProgress",
                    StatusOut = "InProgress",
                    SlaDays = 7,
                    PermissionsRequired = "Grc.Risks.Treat",
                    Description = "Define and implement risk treatment plans"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 6,
                    StepId = "monitoring",
                    StepName = "Monitoring & Review",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "RISK_MANAGER",
                    AssigneeRule = "ByInitiator",
                    UIPageRoute = "~/Risks/Monitor",
                    ApiEndpoint = "PUT /api/risk/{id}/monitor",
                    StatusIn = "InProgress",
                    StatusOut = "Approved",
                    SlaDays = 0,
                    PermissionsRequired = "Grc.Risks.Monitor",
                    Description = "Ongoing risk monitoring and review"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 7,
                    StepId = "end",
                    StepName = "Assessment Complete",
                    StepType = "End",
                    BpmnType = "endEvent",
                    Trigger = "Automatic",
                    ActorRoleCode = "SYSTEM",
                    AssigneeRule = "N/A",
                    UIPageRoute = "~/Risks",
                    ApiEndpoint = "",
                    StatusIn = "Approved",
                    StatusOut = "Completed",
                    SlaDays = 0,
                    PermissionsRequired = "",
                    Description = "Risk assessment cycle completed"
                }
            };

            return new WorkflowDefinition
            {
                Id = Guid.NewGuid(),
                WorkflowNumber = "WF-ERM-001",
                Name = "Enterprise Risk Management",
                Description = "Enterprise Risk Management Assessment Workflow",
                WorkflowType = "Assessment",
                Category = "Risk",
                Version = "1.0",
                Status = "Active",
                IsActive = true,
                DefaultAssigneeRoleCode = "RISK_MANAGER",
                TotalSteps = steps.Length,
                EstimatedDays = 22,
                StepsJson = JsonSerializer.Serialize(steps),
                BpmnXml = GenerateBpmnXml("ERM_Assessment", steps),
            };
        }

        /// <summary>
        /// 5️⃣ Evidence Review & Approval Workflow (5 steps)
        /// </summary>
        private WorkflowDefinition CreateEvidenceReviewWorkflow()
        {
            var steps = new[]
            {
                new WorkflowStepDefinition
                {
                    StepNo = 1,
                    StepId = "start",
                    StepName = "Evidence Submitted",
                    StepType = "Start",
                    BpmnType = "startEvent",
                    Trigger = "Event",
                    ActorRoleCode = "CONTROL_OWNER",
                    AssigneeRule = "ByUploader",
                    UIPageRoute = "~/Evidence/Upload",
                    ApiEndpoint = "POST /api/evidence",
                    StatusIn = "None",
                    StatusOut = "PendingReview",
                    SlaDays = 0,
                    PermissionsRequired = "Grc.Evidence.Create",
                    Description = "Evidence document submitted for review"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 2,
                    StepId = "initialReview",
                    StepName = "Initial Review",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "COMPLIANCE_OFFICER",
                    AssigneeRule = "ByApprovalChain",
                    UIPageRoute = "~/Evidence/Review",
                    ApiEndpoint = "POST /api/approval-instance/process",
                    StatusIn = "PendingReview",
                    StatusOut = "InReview",
                    SlaDays = 2,
                    PermissionsRequired = "Grc.ApprovalWorkflows.Process",
                    Description = "Initial compliance review of evidence"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 3,
                    StepId = "technicalReview",
                    StepName = "Technical Review",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Sequential",
                    ActorRoleCode = "SME",
                    AssigneeRule = "ByControlCategory",
                    UIPageRoute = "~/Evidence/Review",
                    ApiEndpoint = "POST /api/approval-instance/process",
                    StatusIn = "InReview",
                    StatusOut = "InReview",
                    SlaDays = 3,
                    PermissionsRequired = "Grc.ApprovalWorkflows.Process",
                    Description = "Technical SME review of evidence"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 4,
                    StepId = "finalApproval",
                    StepName = "Final Approval",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Sequential",
                    ActorRoleCode = "AUDIT_MANAGER",
                    AssigneeRule = "ByApprovalChain",
                    UIPageRoute = "~/Evidence/Review",
                    ApiEndpoint = "POST /api/approval-instance/process",
                    StatusIn = "InReview",
                    StatusOut = "Approved",
                    SlaDays = 1,
                    PermissionsRequired = "Grc.ApprovalWorkflows.Process",
                    Description = "Final approval decision"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 5,
                    StepId = "end",
                    StepName = "Evidence Approved",
                    StepType = "End",
                    BpmnType = "endEvent",
                    Trigger = "Automatic",
                    ActorRoleCode = "SYSTEM",
                    AssigneeRule = "N/A",
                    UIPageRoute = "~/Evidence/Details/{id}",
                    ApiEndpoint = "",
                    StatusIn = "Approved",
                    StatusOut = "Approved",
                    SlaDays = 0,
                    PermissionsRequired = "",
                    Description = "Evidence approved and linked to control"
                }
            };

            return new WorkflowDefinition
            {
                Id = Guid.NewGuid(),
                WorkflowNumber = "WF-EVIDENCE-001",
                Name = "Evidence Review & Approval",
                Description = "Multi-level Evidence Review and Approval Workflow",
                WorkflowType = "Approval",
                Category = "Evidence",
                Version = "1.0",
                Status = "Active",
                IsActive = true,
                DefaultAssigneeRoleCode = "COMPLIANCE_OFFICER",
                TotalSteps = steps.Length,
                EstimatedDays = 6,
                StepsJson = JsonSerializer.Serialize(steps),
                BpmnXml = GenerateBpmnXml("Evidence_Review", steps),
            };
        }

        /// <summary>
        /// 6️⃣ Audit Finding Remediation Workflow (7 steps)
        /// </summary>
        private WorkflowDefinition CreateAuditFindingRemediationWorkflow()
        {
            var steps = new[]
            {
                new WorkflowStepDefinition
                {
                    StepNo = 1,
                    StepId = "start",
                    StepName = "Finding Identified",
                    StepType = "Start",
                    BpmnType = "startEvent",
                    Trigger = "Event",
                    ActorRoleCode = "AUDITOR",
                    AssigneeRule = "ByAuditor",
                    UIPageRoute = "~/Audits/Findings",
                    ApiEndpoint = "POST /api/audit/finding",
                    StatusIn = "None",
                    StatusOut = "Open",
                    SlaDays = 0,
                    PermissionsRequired = "Grc.Audits.Create",
                    Description = "Audit finding identified and documented"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 2,
                    StepId = "riskAssessment",
                    StepName = "Risk Assessment",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "RISK_MANAGER",
                    AssigneeRule = "BySeverity",
                    UIPageRoute = "~/Risks/Create",
                    ApiEndpoint = "POST /api/risk/from-finding",
                    StatusIn = "Open",
                    StatusOut = "InProgress",
                    SlaDays = 2,
                    PermissionsRequired = "Grc.Risks.Create",
                    Description = "Assess risk associated with finding"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 3,
                    StepId = "actionPlan",
                    StepName = "Action Plan Created",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "PROCESS_OWNER",
                    AssigneeRule = "ByFindingOwner",
                    UIPageRoute = "~/ActionPlans/Create",
                    ApiEndpoint = "POST /api/action-plan",
                    StatusIn = "InProgress",
                    StatusOut = "InProgress",
                    SlaDays = 3,
                    PermissionsRequired = "Grc.ActionPlans.Create",
                    Description = "Create remediation action plan"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 4,
                    StepId = "implementation",
                    StepName = "Implementation",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "ACTION_OWNER",
                    AssigneeRule = "ByActionAssignee",
                    UIPageRoute = "~/ActionPlans/Execute",
                    ApiEndpoint = "PUT /api/action-plan/{id}/complete",
                    StatusIn = "InProgress",
                    StatusOut = "InProgress",
                    SlaDays = 30,
                    PermissionsRequired = "Grc.ActionPlans.Execute",
                    Description = "Implement remediation actions"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 5,
                    StepId = "validation",
                    StepName = "Validation Review",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "AUDITOR",
                    AssigneeRule = "ByOriginalAuditor",
                    UIPageRoute = "~/Audits/Validate",
                    ApiEndpoint = "PUT /api/audit/finding/{id}/validate",
                    StatusIn = "InProgress",
                    StatusOut = "Validated",
                    SlaDays = 5,
                    PermissionsRequired = "Grc.Audits.Validate",
                    Description = "Validate remediation effectiveness"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 6,
                    StepId = "closureApproval",
                    StepName = "Closure Approval",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Sequential",
                    ActorRoleCode = "AUDIT_MANAGER",
                    AssigneeRule = "ByApprovalChain",
                    UIPageRoute = "~/Audits/Close",
                    ApiEndpoint = "POST /api/approval-instance/process",
                    StatusIn = "Validated",
                    StatusOut = "Closed",
                    SlaDays = 2,
                    PermissionsRequired = "Grc.ApprovalWorkflows.Process",
                    Description = "Approve finding closure"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 7,
                    StepId = "end",
                    StepName = "Finding Closed",
                    StepType = "End",
                    BpmnType = "endEvent",
                    Trigger = "Automatic",
                    ActorRoleCode = "SYSTEM",
                    AssigneeRule = "N/A",
                    UIPageRoute = "~/Audits/Findings/{id}",
                    ApiEndpoint = "",
                    StatusIn = "Closed",
                    StatusOut = "Closed",
                    SlaDays = 0,
                    PermissionsRequired = "",
                    Description = "Audit finding closed"
                }
            };

            return new WorkflowDefinition
            {
                Id = Guid.NewGuid(),
                WorkflowNumber = "WF-AUDIT-REM-001",
                Name = "Audit Finding Remediation",
                Description = "Audit Finding Remediation and Closure Workflow",
                WorkflowType = "Remediation",
                Category = "Audit",
                Version = "1.0",
                Status = "Active",
                IsActive = true,
                DefaultAssigneeRoleCode = "AUDITOR",
                TotalSteps = steps.Length,
                EstimatedDays = 42,
                StepsJson = JsonSerializer.Serialize(steps),
                BpmnXml = GenerateBpmnXml("Audit_Remediation", steps),
            };
        }

        /// <summary>
        /// 7️⃣ Policy Review & Publication Workflow (7 steps)
        /// </summary>
        private WorkflowDefinition CreatePolicyReviewWorkflow()
        {
            var steps = new[]
            {
                new WorkflowStepDefinition
                {
                    StepNo = 1,
                    StepId = "start",
                    StepName = "Policy Draft Created",
                    StepType = "Start",
                    BpmnType = "startEvent",
                    Trigger = "Manual",
                    ActorRoleCode = "POLICY_OWNER",
                    AssigneeRule = "ByInitiator",
                    UIPageRoute = "~/Policies/Create",
                    ApiEndpoint = "POST /api/policy",
                    StatusIn = "None",
                    StatusOut = "Draft",
                    SlaDays = 0,
                    PermissionsRequired = "Grc.Policies.Create",
                    Description = "Create new policy draft"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 2,
                    StepId = "legalReview",
                    StepName = "Legal Review",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "LEGAL_COUNSEL",
                    AssigneeRule = "ByDepartment",
                    UIPageRoute = "~/Policies/Review",
                    ApiEndpoint = "POST /api/approval-instance",
                    StatusIn = "Draft",
                    StatusOut = "InReview",
                    SlaDays = 5,
                    PermissionsRequired = "Grc.ApprovalWorkflows.Process",
                    Description = "Legal review of policy content"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 3,
                    StepId = "complianceReview",
                    StepName = "Compliance Review",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Parallel",
                    ActorRoleCode = "COMPLIANCE_OFFICER",
                    AssigneeRule = "ByApprovalChain",
                    UIPageRoute = "~/Policies/Review",
                    ApiEndpoint = "POST /api/approval-instance/process",
                    StatusIn = "InReview",
                    StatusOut = "InReview",
                    SlaDays = 3,
                    PermissionsRequired = "Grc.ApprovalWorkflows.Process",
                    Description = "Compliance review and sign-off"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 4,
                    StepId = "executiveApproval",
                    StepName = "Executive Approval",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Sequential",
                    ActorRoleCode = "CISO",
                    AssigneeRule = "ByApprovalChain",
                    UIPageRoute = "~/Policies/Approve",
                    ApiEndpoint = "POST /api/approval-instance/process",
                    StatusIn = "InReview",
                    StatusOut = "Approved",
                    SlaDays = 2,
                    PermissionsRequired = "Grc.ApprovalWorkflows.Process",
                    Description = "Executive approval of policy"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 5,
                    StepId = "publication",
                    StepName = "Publication",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "POLICY_ADMIN",
                    AssigneeRule = "ByRole",
                    UIPageRoute = "~/Policies/Publish",
                    ApiEndpoint = "PUT /api/policy/{id}/publish",
                    StatusIn = "Approved",
                    StatusOut = "Published",
                    SlaDays = 1,
                    PermissionsRequired = "Grc.Policies.Publish",
                    Description = "Publish policy to organization"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 6,
                    StepId = "acknowledgment",
                    StepName = "Staff Acknowledgment",
                    StepType = "Task",
                    BpmnType = "userTask",
                    Trigger = "Automatic",
                    ActorRoleCode = "ALL_STAFF",
                    AssigneeRule = "ByTenantUsers",
                    UIPageRoute = "~/Policies/Acknowledge",
                    ApiEndpoint = "POST /api/policy/{id}/ack",
                    StatusIn = "Published",
                    StatusOut = "Acknowledged",
                    SlaDays = 7,
                    PermissionsRequired = "",
                    Description = "Staff acknowledgment of policy"
                },
                new WorkflowStepDefinition
                {
                    StepNo = 7,
                    StepId = "end",
                    StepName = "Policy Active",
                    StepType = "End",
                    BpmnType = "endEvent",
                    Trigger = "Automatic",
                    ActorRoleCode = "SYSTEM",
                    AssigneeRule = "N/A",
                    UIPageRoute = "~/Policies/Details/{id}",
                    ApiEndpoint = "",
                    StatusIn = "Acknowledged",
                    StatusOut = "Active",
                    SlaDays = 0,
                    PermissionsRequired = "",
                    Description = "Policy is now active"
                }
            };

            return new WorkflowDefinition
            {
                Id = Guid.NewGuid(),
                WorkflowNumber = "WF-POLICY-001",
                Name = "Policy Review & Publication",
                Description = "Policy Review, Approval, and Publication Workflow",
                WorkflowType = "Approval",
                Category = "Governance",
                Version = "1.0",
                Status = "Active",
                IsActive = true,
                DefaultAssigneeRoleCode = "POLICY_OWNER",
                TotalSteps = steps.Length,
                EstimatedDays = 18,
                StepsJson = JsonSerializer.Serialize(steps),
                BpmnXml = GenerateBpmnXml("Policy_Review", steps),
            };
        }

        /// <summary>
        /// Generate simple BPMN 2.0 XML for the workflow
        /// </summary>
        private string GenerateBpmnXml(string processId, WorkflowStepDefinition[] steps)
        {
            var flowNodes = string.Join("\n    ", steps.Select(s =>
                s.BpmnType == "startEvent" ? $"<bpmn:startEvent id=\"{s.StepId}\" name=\"{s.StepName}\" />" :
                s.BpmnType == "endEvent" ? $"<bpmn:endEvent id=\"{s.StepId}\" name=\"{s.StepName}\" />" :
                $"<bpmn:userTask id=\"{s.StepId}\" name=\"{s.StepName}\" />"
            ));

            var sequenceFlows = string.Join("\n    ", steps.Skip(1).Select((s, i) =>
                $"<bpmn:sequenceFlow id=\"flow_{i}\" sourceRef=\"{steps[i].StepId}\" targetRef=\"{s.StepId}\" />"
            ));

            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<bpmn:definitions xmlns:bpmn=""http://www.omg.org/spec/BPMN/20100524/MODEL"" id=""Definitions_{processId}"">
  <bpmn:process id=""{processId}"" isExecutable=""true"">
    {flowNodes}
    {sequenceFlows}
  </bpmn:process>
</bpmn:definitions>";
        }
    }

    /// <summary>
    /// Workflow step definition for JSON serialization
    /// </summary>
    public class WorkflowStepDefinition
    {
        public int StepNo { get; set; }
        public string StepId { get; set; } = string.Empty;
        public string StepName { get; set; } = string.Empty;
        public string StepType { get; set; } = string.Empty; // Start, Task, End
        public string BpmnType { get; set; } = string.Empty; // startEvent, userTask, endEvent
        public string Trigger { get; set; } = string.Empty; // Manual, Automatic, Event, Sequential, Parallel
        public string ActorRoleCode { get; set; } = string.Empty;
        public string AssigneeRule { get; set; } = string.Empty;
        public string UIPageRoute { get; set; } = string.Empty;
        public string ApiEndpoint { get; set; } = string.Empty;
        public string StatusIn { get; set; } = string.Empty;
        public string StatusOut { get; set; } = string.Empty;
        public int SlaDays { get; set; }
        public string PermissionsRequired { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}

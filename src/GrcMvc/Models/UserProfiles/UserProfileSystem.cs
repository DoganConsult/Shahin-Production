namespace GrcMvc.Models.UserProfiles
{
    /// <summary>
    /// User Profile System - 14 Distinct Roles for Complete GRC Workflow Management
    /// Manages all stages: Creation, Review, Approval, Implementation, Testing, Monitoring
    /// </summary>

    /// <summary>
    /// 1. Creator - Initiates workflows and creates content
    /// Permissions: Create controls, risks, policies, initiate workflows
    /// </summary>
    public interface ICreatorProfile
    {
        Task<bool> CreateControlAsync(int tenantId, object control);
        Task<bool> CreateRiskAsync(int tenantId, object risk);
        Task<bool> CreatePolicyAsync(int tenantId, object policy);
        Task<bool> InitiateWorkflowAsync(int entityId, string workflowType);
    }

    /// <summary>
    /// 2. Submitter - Submits work for review
    /// Permissions: Submit evidence, test results, implementations
    /// </summary>
    public interface ISubmitterProfile
    {
        Task<bool> SubmitEvidenceAsync(int workflowId, object evidence);
        Task<bool> SubmitTestResultsAsync(int workflowId, object results);
        Task<bool> SubmitImplementationAsync(int workflowId, object details);
    }

    /// <summary>
    /// 3. Reviewer - Reviews submissions before approval
    /// Permissions: Review evidence, test results, provide feedback, request revisions
    /// </summary>
    public interface IReviewerProfile
    {
        Task<bool> ReviewSubmissionAsync(int workflowId, string feedback);
        Task<bool> RequestRevisionsAsync(int workflowId, string revisionNotes);
        Task<bool> ApproveReviewAsync(int workflowId);
    }

    /// <summary>
    /// 4. Manager - Approves at management level
    /// Permissions: Approve submissions, sign off on implementations
    /// </summary>
    public interface IManagerProfile
    {
        Task<bool> ApproveAsync(int workflowId, string comments);
        Task<bool> RejectAsync(int workflowId, string reason);
        Task<bool> RequestChangesAsync(int workflowId, string changes);
    }

    /// <summary>
    /// 5. Compliance Officer - Ensures regulatory compliance
    /// Permissions: Approve from compliance perspective, audit, enforce standards
    /// </summary>
    public interface IComplianceOfficerProfile
    {
        Task<bool> ApproveComplianceAsync(int workflowId, string complianceNotes);
        Task<bool> FlagNonComplianceAsync(int workflowId, string issues);
        Task<bool> AuditControlAsync(int controlId);
    }

    /// <summary>
    /// 6. Risk Manager - Manages risk-related workflows
    /// Permissions: Assess risks, approve risk workflows, manage remediation
    /// </summary>
    public interface IRiskManagerProfile
    {
        Task<bool> AssessRiskAsync(int riskId, string assessment);
        Task<bool> ApproveRiskRemediationAsync(int workflowId);
        Task<bool> UpdateRiskStatusAsync(int riskId, string status);
    }

    /// <summary>
    /// 7. Control Reviewer - Reviews control implementations
    /// Permissions: Review controls, approve implementations, assess effectiveness
    /// </summary>
    public interface IControlReviewerProfile
    {
        Task<bool> ReviewControlAsync(int controlId, string review);
        Task<bool> ApproveControlAsync(int workflowId);
        Task<bool> RejectControlAsync(int workflowId, string reason);
    }

    /// <summary>
    /// 8. Auditor - Conducts compliance testing and audits
    /// Permissions: Execute tests, conduct audits, document findings
    /// </summary>
    public interface IAuditorProfile
    {
        Task<bool> ConductComplianceTestAsync(int controlId, object testPlan);
        Task<bool> DocumentTestResultsAsync(int workflowId, object results);
        Task<bool> ConductAuditAsync(int auditId);
    }

    /// <summary>
    /// 9. Executive - High-level approval authority
    /// Permissions: Final sign-off, approval of major initiatives, policy approval
    /// </summary>
    public interface IExecutiveProfile
    {
        Task<bool> SignOffAsync(int workflowId, string comments);
        Task<bool> ApproveInitiativeAsync(int entityId, string entityType);
        Task<bool> RejectWithFeedbackAsync(int workflowId, string feedback);
    }

    /// <summary>
    /// 10. Approver - General approval authority across workflows
    /// Permissions: Approve various workflows, sign documents
    /// </summary>
    public interface IApproverProfile
    {
        Task<bool> ApproveWorkflowAsync(int workflowId, string approvalType, string comments);
        Task<bool> RejectWorkflowAsync(int workflowId, string reason);
        Task<bool> GetPendingApprovalsAsync(int tenantId);
    }

    /// <summary>
    /// 11. Exception Approver - Approves policy exceptions
    /// Permissions: Approve exceptions, set conditions, monitor exceptions
    /// </summary>
    public interface IExceptionApproverProfile
    {
        Task<bool> ApproveExceptionAsync(int exceptionId, string conditions);
        Task<bool> RejectExceptionAsync(int exceptionId, string reason);
        Task<bool> MonitorExceptionAsync(int exceptionId);
    }

    /// <summary>
    /// 12. Process Owner - Owns and manages workflows
    /// Permissions: Create workflows, reassign tasks, manage process flow
    /// </summary>
    public interface IProcessOwnerProfile
    {
        Task<bool> CreateWorkflowAsync(string workflowType, object config);
        Task<bool> ReassignTaskAsync(int taskId, string newAssignee);
        Task<bool> EscalateTaskAsync(int taskId, string escalationReason);
    }

    /// <summary>
    /// 13. Administrator - Full system access
    /// Permissions: All operations, user management, system configuration
    /// </summary>
    public interface IAdministratorProfile
    {
        Task<bool> ManageAllWorkflowsAsync();
        Task<bool> ConfigureSystemAsync();
        Task<bool> ManageUsersAsync();
    }

    /// <summary>
    /// 14. Monitor - Monitors workflow progress
    /// Permissions: View workflows, generate reports, track metrics
    /// </summary>
    public interface IMonitorProfile
    {
        Task<object> GetWorkflowMetricsAsync(int tenantId);
        Task<object> GenerateComplianceReportAsync(DateTime startDate, DateTime endDate);
        Task<object> GetWorkflowStatusAsync(int workflowId);
    }

    // ===== PROFILE DEFINITIONS =====

    /// <summary>User profile configuration model</summary>
    public class UserProfile
    {
        public int Id { get; set; }
        public string ProfileName { get; set; }
        public string Description { get; set; }
        public List<string> Permissions { get; set; } = new();
        public List<string> WorkflowRoles { get; set; } = new();
        public int TenantId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>User profile assignments</summary>
    public class UserProfileAssignment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProfileId { get; set; }
        public int TenantId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public UserProfile Profile { get; set; }
    }

    /// <summary>Profile permission model</summary>
    public class ProfilePermission
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public string Permission { get; set; }
        public string ResourceType { get; set; } // Control, Risk, Policy, Workflow, etc.
        public string Action { get; set; } // Create, Read, Update, Delete, Approve, etc.
    }

    /// <summary>14 Default Profile Configurations</summary>
    public static class DefaultProfiles
    {
        public static UserProfile CreatorProfile => new()
        {
            ProfileName = "Creator",
            Description = "Initiates workflows and creates content",
            Permissions = new List<string>
            {
                "control:create", "risk:create", "policy:create",
                "workflow:initiate", "evidence:submit"
            },
            WorkflowRoles = new List<string> { "creator", "submitter" }
        };

        public static UserProfile SubmitterProfile => new()
        {
            ProfileName = "Submitter",
            Description = "Submits work for review",
            Permissions = new List<string>
            {
                "evidence:submit", "results:submit", "implementation:submit",
                "workflow:view"
            },
            WorkflowRoles = new List<string> { "submitter" }
        };

        public static UserProfile ReviewerProfile => new()
        {
            ProfileName = "Reviewer",
            Description = "Reviews submissions before approval",
            Permissions = new List<string>
            {
                "submission:review", "feedback:provide", "revision:request",
                "workflow:view", "workflow:comment"
            },
            WorkflowRoles = new List<string> { "reviewer" }
        };

        public static UserProfile ManagerProfile => new()
        {
            ProfileName = "Manager",
            Description = "Approves at management level",
            Permissions = new List<string>
            {
                "workflow:approve", "workflow:reject", "workflow:view",
                "changes:request", "team:manage"
            },
            WorkflowRoles = new List<string> { "manager", "approver" }
        };

        public static UserProfile ComplianceOfficerProfile => new()
        {
            ProfileName = "Compliance Officer",
            Description = "Ensures regulatory compliance",
            Permissions = new List<string>
            {
                "compliance:assess", "noncompliance:flag", "control:audit",
                "workflow:approve", "standards:enforce", "audit:view"
            },
            WorkflowRoles = new List<string> { "compliance_officer", "approver", "auditor" }
        };

        public static UserProfile RiskManagerProfile => new()
        {
            ProfileName = "Risk Manager",
            Description = "Manages risk-related workflows",
            Permissions = new List<string>
            {
                "risk:assess", "risk:approve", "remediation:approve",
                "risk:update", "workflow:view", "escalation:manage"
            },
            WorkflowRoles = new List<string> { "risk_manager", "approver" }
        };

        public static UserProfile ControlReviewerProfile => new()
        {
            ProfileName = "Control Reviewer",
            Description = "Reviews control implementations",
            Permissions = new List<string>
            {
                "control:review", "control:approve", "control:reject",
                "effectiveness:assess", "workflow:view"
            },
            WorkflowRoles = new List<string> { "control_reviewer", "approver" }
        };

        public static UserProfile AuditorProfile => new()
        {
            ProfileName = "Auditor",
            Description = "Conducts compliance testing and audits",
            Permissions = new List<string>
            {
                "test:conduct", "test:document", "audit:conduct",
                "findings:report", "workflow:view", "evidence:collect"
            },
            WorkflowRoles = new List<string> { "auditor", "tester" }
        };

        public static UserProfile ExecutiveProfile => new()
        {
            ProfileName = "Executive",
            Description = "High-level approval authority",
            Permissions = new List<string>
            {
                "workflow:signoff", "initiative:approve", "policy:approve",
                "workflow:view", "dashboard:view", "reporting:view"
            },
            WorkflowRoles = new List<string> { "executive", "approver", "signatory" }
        };

        public static UserProfile ApproverProfile => new()
        {
            ProfileName = "Approver",
            Description = "General approval authority",
            Permissions = new List<string>
            {
                "workflow:approve", "workflow:reject", "workflow:view",
                "comments:add", "history:view"
            },
            WorkflowRoles = new List<string> { "approver" }
        };

        public static UserProfile ExceptionApproverProfile => new()
        {
            ProfileName = "Exception Approver",
            Description = "Approves policy exceptions",
            Permissions = new List<string>
            {
                "exception:approve", "exception:reject", "exception:monitor",
                "conditions:set", "workflow:view"
            },
            WorkflowRoles = new List<string> { "exception_approver", "approver" }
        };

        public static UserProfile ProcessOwnerProfile => new()
        {
            ProfileName = "Process Owner",
            Description = "Owns and manages workflows",
            Permissions = new List<string>
            {
                "workflow:create", "task:reassign", "task:escalate",
                "process:manage", "workflow:view", "workflow:configure"
            },
            WorkflowRoles = new List<string> { "process_owner", "manager" }
        };

        public static UserProfile AdministratorProfile => new()
        {
            ProfileName = "Administrator",
            Description = "Full system access",
            Permissions = new List<string>
            {
                "system:*", "workflow:*", "user:manage", "config:manage",
                "audit:view", "reporting:*"
            },
            WorkflowRoles = new List<string> { "admin", "system_admin" }
        };

        public static UserProfile MonitorProfile => new()
        {
            ProfileName = "Monitor",
            Description = "Monitors workflow progress",
            Permissions = new List<string>
            {
                "workflow:view", "metrics:view", "reporting:generate",
                "dashboard:view", "status:track"
            },
            WorkflowRoles = new List<string> { "monitor", "viewer" }
        };

        public static List<UserProfile> GetAllProfiles() => new()
        {
            CreatorProfile,
            SubmitterProfile,
            ReviewerProfile,
            ManagerProfile,
            ComplianceOfficerProfile,
            RiskManagerProfile,
            ControlReviewerProfile,
            AuditorProfile,
            ExecutiveProfile,
            ApproverProfile,
            ExceptionApproverProfile,
            ProcessOwnerProfile,
            AdministratorProfile,
            MonitorProfile
        };
    }

    /// <summary>Workflow stage assignments based on profile</summary>
    public static class ProfileWorkflowMapping
    {
        public static Dictionary<string, List<string>> GetProfileWorkflowRoles() => new()
        {
            { "Creator", new() { "Initiate" } },
            { "Submitter", new() { "Submit" } },
            { "Reviewer", new() { "Review" } },
            { "Manager", new() { "ManagerApprove" } },
            { "Compliance Officer", new() { "ComplianceApprove", "Audit" } },
            { "Risk Manager", new() { "RiskAssess", "RemediationApprove" } },
            { "Control Reviewer", new() { "ControlReview", "ControlApprove" } },
            { "Auditor", new() { "TestConduct", "FindingsDocument" } },
            { "Executive", new() { "ExecutiveSignOff" } },
            { "Approver", new() { "GeneralApprove" } },
            { "Exception Approver", new() { "ExceptionApprove" } },
            { "Process Owner", new() { "ManageProcess", "ReassignTasks" } },
            { "Administrator", new() { "ManageAll" } },
            { "Monitor", new() { "ViewWorkflows", "GenerateReports" } }
        };
    }
}

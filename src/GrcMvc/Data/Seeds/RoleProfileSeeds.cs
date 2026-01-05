using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GrcMvc.Models.Entities;

namespace GrcMvc.Data.Seeds
{
    /// <summary>
    /// Seeds 15 predefined role profiles across organizational layers
    /// with scopes, responsibilities, and approval authorities
    /// </summary>
    public static class RoleProfileSeeds
    {
        public static async Task SeedRoleProfilesAsync(GrcDbContext context, ILogger<ApplicationInitializer> logger)
        {
            try
            {
                // Check if roles already exist
                var existingRoles = await context.RoleProfiles.AnyAsync();
                if (existingRoles)
                {
                    logger.LogInformation("✅ Role profiles already exist. Skipping seed.");
                    return;
                }

                var roleProfiles = new List<RoleProfile>
                {
                    // EXECUTIVE LAYER (3 roles)
                    CreateChiefRiskOfficer(),
                    CreateChiefComplianceOfficer(),
                    CreateExecutiveDirector(),

                    // MANAGEMENT LAYER (5 roles)
                    CreateRiskManager(),
                    CreateComplianceManager(),
                    CreateAuditManager(),
                    CreateSecurityManager(),
                    CreateLegalManager(),

                    // OPERATIONAL LAYER (5 roles)
                    CreateComplianceOfficer(),
                    CreateRiskAnalyst(),
                    CreatePrivacyOfficer(),
                    CreateQualityAssuranceManager(),
                    CreateProcessOwner(),

                    // SUPPORT LAYER (2 roles)
                    CreateDocumentationSpecialist(),
                    CreateReportingAnalyst()
                };

                await context.RoleProfiles.AddRangeAsync(roleProfiles);
                await context.SaveChangesAsync();

                logger.LogInformation("✅ Successfully seeded 15 role profiles");
            }
            catch (Exception ex)
            {
                logger.LogError($"❌ Error seeding role profiles: {ex.Message}");
                throw;
            }
        }

        // EXECUTIVE LAYER
        private static RoleProfile CreateChiefRiskOfficer()
        {
            return new RoleProfile
            {
                Id = Guid.NewGuid(),
                RoleCode = "CRO",
                RoleName = "Chief Risk Officer",
                Layer = "Executive",
                Department = "Risk Management",
                DisplayOrder = 1,
                Description = "Oversees enterprise risk management strategy and governance",
                Scope = "Enterprise-wide risk governance, strategic risk oversight, board reporting, risk committee management",
                Responsibilities = JsonSerializer.Serialize(new[]
                {
                    "Define enterprise risk strategy",
                    "Oversee all risk assessments",
                    "Executive approval of major risks",
                    "Board reporting and compliance",
                    "Risk culture development",
                    "Escalation authority for critical risks"
                }),
                ApprovalLevel = 4,
                ApprovalAuthority = 10000000m,
                CanEscalate = true,
                CanApprove = true,
                CanReject = true,
                CanReassign = true,
                IsActive = true,
                TenantId = null,
                ParticipatingWorkflows = "WF-NCA-ECC-001,WF-SAMA-CSF-001,WF-ERM-001,WF-FINDING-REMEDIATION-001"
            };
        }

        private static RoleProfile CreateChiefComplianceOfficer()
        {
            return new RoleProfile
            {
                Id = Guid.NewGuid(),
                RoleCode = "CCO",
                RoleName = "Chief Compliance Officer",
                Layer = "Executive",
                Department = "Compliance",
                DisplayOrder = 2,
                Description = "Oversees compliance program and regulatory obligations",
                Scope = "Regulatory compliance, policy governance, audit coordination, compliance reporting",
                Responsibilities = JsonSerializer.Serialize(new[]
                {
                    "Manage compliance program",
                    "Regulatory reporting",
                    "Policy review and approval",
                    "Audit coordination",
                    "Compliance training oversight",
                    "Board compliance updates"
                }),
                ApprovalLevel = 4,
                ApprovalAuthority = 5000000m,
                CanEscalate = true,
                CanApprove = true,
                CanReject = true,
                CanReassign = true,
                IsActive = true,
                TenantId = null,
                ParticipatingWorkflows = "WF-SAMA-CSF-001,WF-PDPL-PIA-001,WF-POLICY-001,WF-EVIDENCE-001"
            };
        }

        private static RoleProfile CreateExecutiveDirector()
        {
            return new RoleProfile
            {
                Id = Guid.NewGuid(),
                RoleCode = "ED",
                RoleName = "Executive Director",
                Layer = "Executive",
                Department = "Executive Office",
                DisplayOrder = 3,
                Description = "Overall organizational leadership and strategy",
                Scope = "All departments, strategic decisions, board relations, executive oversight",
                Responsibilities = JsonSerializer.Serialize(new[]
                {
                    "Organizational strategy",
                    "Board relations",
                    "Executive approval authority",
                    "Policy publication approval",
                    "Critical escalation resolution",
                    "Stakeholder communication"
                }),
                ApprovalLevel = 4,
                ApprovalAuthority = 25000000m,
                CanEscalate = true,
                CanApprove = true,
                CanReject = true,
                CanReassign = true,
                IsActive = true,
                TenantId = null,
                ParticipatingWorkflows = "WF-POLICY-001"
            };
        }

        // MANAGEMENT LAYER
        private static RoleProfile CreateRiskManager()
        {
            return new RoleProfile
            {
                Id = Guid.NewGuid(),
                RoleCode = "RM",
                RoleName = "Risk Manager",
                Layer = "Management",
                Department = "Risk Management",
                DisplayOrder = 4,
                Description = "Manages operational risk assessments and remediation",
                Scope = "Risk identification, assessment, remediation planning, risk monitoring, departmental risk oversight",
                Responsibilities = JsonSerializer.Serialize(new[]
                {
                    "Conduct risk assessments",
                    "Develop remediation plans",
                    "Monitor risk indicators",
                    "Team risk oversight",
                    "Assessment report preparation",
                    "Gap analysis execution"
                }),
                ApprovalLevel = 3,
                ApprovalAuthority = 500000m,
                CanEscalate = true,
                CanApprove = true,
                CanReject = false,
                CanReassign = true,
                IsActive = true,
                TenantId = null,
                ParticipatingWorkflows = "WF-NCA-ECC-001,WF-ERM-001,WF-FINDING-REMEDIATION-001"
            };
        }

        private static RoleProfile CreateComplianceManager()
        {
            return new RoleProfile
            {
                Id = Guid.NewGuid(),
                RoleCode = "CM",
                RoleName = "Compliance Manager",
                Layer = "Management",
                Department = "Compliance",
                DisplayOrder = 5,
                Description = "Manages compliance monitoring and audit coordination",
                Scope = "Compliance monitoring, audit management, control evaluation, compliance reporting",
                Responsibilities = JsonSerializer.Serialize(new[]
                {
                    "Compliance monitoring",
                    "Audit scheduling and coordination",
                    "Control effectiveness testing",
                    "Compliance metrics tracking",
                    "Evidence collection oversight",
                    "Audit finding management"
                }),
                ApprovalLevel = 3,
                ApprovalAuthority = 300000m,
                CanEscalate = true,
                CanApprove = true,
                CanReject = false,
                CanReassign = true,
                IsActive = true,
                TenantId = null,
                ParticipatingWorkflows = "WF-SAMA-CSF-001,WF-EVIDENCE-001"
            };
        }

        private static RoleProfile CreateAuditManager()
        {
            return new RoleProfile
            {
                Id = Guid.NewGuid(),
                RoleCode = "AM",
                RoleName = "Audit Manager",
                Layer = "Management",
                Department = "Audit",
                DisplayOrder = 6,
                Description = "Manages internal audit operations and findings",
                Scope = "Audit planning, execution, finding management, remediation tracking, audit reporting",
                Responsibilities = JsonSerializer.Serialize(new[]
                {
                    "Plan and schedule audits",
                    "Manage audit executions",
                    "Issue audit findings",
                    "Track remediation progress",
                    "Audit report finalization",
                    "Final approval of remediation closure"
                }),
                ApprovalLevel = 3,
                ApprovalAuthority = 1000000m,
                CanEscalate = true,
                CanApprove = true,
                CanReject = true,
                CanReassign = true,
                IsActive = true,
                TenantId = null,
                ParticipatingWorkflows = "WF-FINDING-REMEDIATION-001,WF-EVIDENCE-001"
            };
        }

        private static RoleProfile CreateSecurityManager()
        {
            return new RoleProfile
            {
                Id = Guid.NewGuid(),
                RoleCode = "SM",
                RoleName = "Security Manager",
                Layer = "Management",
                Department = "Security & IT",
                DisplayOrder = 7,
                Description = "Oversees information security and data protection",
                Scope = "Incident response, security controls, vulnerability management, safeguards implementation",
                Responsibilities = JsonSerializer.Serialize(new[]
                {
                    "Incident response planning",
                    "Security controls implementation",
                    "Vulnerability assessment",
                    "Data safeguards verification",
                    "Security training oversight",
                    "Resilience testing"
                }),
                ApprovalLevel = 3,
                ApprovalAuthority = 250000m,
                CanEscalate = true,
                CanApprove = true,
                CanReject = false,
                CanReassign = true,
                IsActive = true,
                TenantId = null,
                ParticipatingWorkflows = "WF-SAMA-CSF-001,WF-PDPL-PIA-001"
            };
        }

        private static RoleProfile CreateLegalManager()
        {
            return new RoleProfile
            {
                Id = Guid.NewGuid(),
                RoleCode = "LM",
                RoleName = "Legal Manager",
                Layer = "Management",
                Department = "Legal & Compliance",
                DisplayOrder = 8,
                Description = "Manages legal aspects of governance and compliance",
                Scope = "Legal review, policy compliance, regulatory interpretation, contract governance",
                Responsibilities = JsonSerializer.Serialize(new[]
                {
                    "Legal review of policies",
                    "Regulatory interpretation",
                    "Legal basis determination",
                    "Privacy legal assessment",
                    "Policy publication approval",
                    "Legal compliance certification"
                }),
                ApprovalLevel = 3,
                ApprovalAuthority = 400000m,
                CanEscalate = true,
                CanApprove = true,
                CanReject = true,
                CanReassign = true,
                IsActive = true,
                TenantId = null,
                ParticipatingWorkflows = "WF-PDPL-PIA-001,WF-POLICY-001"
            };
        }

        // OPERATIONAL LAYER
        private static RoleProfile CreateComplianceOfficer()
        {
            return new RoleProfile
            {
                Id = Guid.NewGuid(),
                RoleCode = "CO",
                RoleName = "Compliance Officer",
                Layer = "Operational",
                Department = "Compliance",
                DisplayOrder = 9,
                Description = "Executes compliance activities and evidence collection",
                Scope = "Compliance activities, evidence collection, control testing, compliance documentation",
                Responsibilities = JsonSerializer.Serialize(new[]
                {
                    "Execute compliance activities",
                    "Collect compliance evidence",
                    "Test control effectiveness",
                    "Evidence review",
                    "Compliance documentation",
                    "Initial evidence approval"
                }),
                ApprovalLevel = 2,
                ApprovalAuthority = 50000m,
                CanEscalate = false,
                CanApprove = true,
                CanReject = false,
                CanReassign = false,
                IsActive = true,
                TenantId = null,
                ParticipatingWorkflows = "WF-SAMA-CSF-001,WF-EVIDENCE-001"
            };
        }

        private static RoleProfile CreateRiskAnalyst()
        {
            return new RoleProfile
            {
                Id = Guid.NewGuid(),
                RoleCode = "RA",
                RoleName = "Risk Analyst",
                Layer = "Operational",
                Department = "Risk Management",
                DisplayOrder = 10,
                Description = "Performs detailed risk analysis and control assessment",
                Scope = "Risk analysis, control assessment, remediation planning support, risk documentation",
                Responsibilities = JsonSerializer.Serialize(new[]
                {
                    "Perform detailed risk analysis",
                    "Control assessment execution",
                    "Gap analysis support",
                    "Remediation plan development",
                    "Risk documentation",
                    "Assessment data compilation"
                }),
                ApprovalLevel = 1,
                ApprovalAuthority = 25000m,
                CanEscalate = false,
                CanApprove = false,
                CanReject = false,
                CanReassign = false,
                IsActive = true,
                TenantId = null,
                ParticipatingWorkflows = "WF-NCA-ECC-001,WF-ERM-001"
            };
        }

        private static RoleProfile CreatePrivacyOfficer()
        {
            return new RoleProfile
            {
                Id = Guid.NewGuid(),
                RoleCode = "PO",
                RoleName = "Privacy Officer",
                Layer = "Operational",
                Department = "Privacy & Data Protection",
                DisplayOrder = 11,
                Description = "Manages privacy impact assessments and data protection",
                Scope = "Privacy assessments, data mapping, consent management, rights management, privacy documentation",
                Responsibilities = JsonSerializer.Serialize(new[]
                {
                    "Conduct privacy assessments",
                    "Data mapping execution",
                    "Risk assessment for privacy",
                    "Consent management",
                    "Rights management execution",
                    "Privacy documentation"
                }),
                ApprovalLevel = 2,
                ApprovalAuthority = 100000m,
                CanEscalate = false,
                CanApprove = true,
                CanReject = false,
                CanReassign = false,
                IsActive = true,
                TenantId = null,
                ParticipatingWorkflows = "WF-PDPL-PIA-001"
            };
        }

        private static RoleProfile CreateQualityAssuranceManager()
        {
            return new RoleProfile
            {
                Id = Guid.NewGuid(),
                RoleCode = "QA",
                RoleName = "Quality Assurance Manager",
                Layer = "Operational",
                Department = "Quality Assurance",
                DisplayOrder = 12,
                Description = "Validates findings and approves remediation",
                Scope = "Validation testing, remediation verification, quality assurance, closure verification",
                Responsibilities = JsonSerializer.Serialize(new[]
                {
                    "Validate findings",
                    "Test remediation effectiveness",
                    "Verify control implementation",
                    "Closure verification",
                    "Quality assurance testing",
                    "Finding validation approval"
                }),
                ApprovalLevel = 2,
                ApprovalAuthority = 75000m,
                CanEscalate = false,
                CanApprove = true,
                CanReject = true,
                CanReassign = false,
                IsActive = true,
                TenantId = null,
                ParticipatingWorkflows = "WF-FINDING-REMEDIATION-001"
            };
        }

        private static RoleProfile CreateProcessOwner()
        {
            return new RoleProfile
            {
                Id = Guid.NewGuid(),
                RoleCode = "ProcOwner",
                RoleName = "Process Owner",
                Layer = "Operational",
                Department = "Operations",
                DisplayOrder = 13,
                Description = "Owns operational processes and remediation implementation",
                Scope = "Process governance, remediation implementation, process improvement, control execution",
                Responsibilities = JsonSerializer.Serialize(new[]
                {
                    "Own processes",
                    "Implement remediation",
                    "Process improvement",
                    "Control execution",
                    "Team oversight",
                    "Process documentation"
                }),
                ApprovalLevel = 1,
                ApprovalAuthority = 30000m,
                CanEscalate = false,
                CanApprove = false,
                CanReject = false,
                CanReassign = false,
                IsActive = true,
                TenantId = null,
                ParticipatingWorkflows = "WF-FINDING-REMEDIATION-001"
            };
        }

        // SUPPORT LAYER
        private static RoleProfile CreateDocumentationSpecialist()
        {
            return new RoleProfile
            {
                Id = Guid.NewGuid(),
                RoleCode = "DS",
                RoleName = "Documentation Specialist",
                Layer = "Support",
                Department = "Governance & Documentation",
                DisplayOrder = 14,
                Description = "Manages policy documentation and publication",
                Scope = "Policy documentation, publication, acknowledgment tracking, communications support",
                Responsibilities = JsonSerializer.Serialize(new[]
                {
                    "Prepare policy documentation",
                    "Manage policy publication",
                    "Track staff acknowledgments",
                    "Policy communications",
                    "Documentation updates",
                    "Archive management"
                }),
                ApprovalLevel = 0,
                ApprovalAuthority = null,
                CanEscalate = false,
                CanApprove = false,
                CanReject = false,
                CanReassign = false,
                IsActive = true,
                TenantId = null,
                ParticipatingWorkflows = "WF-POLICY-001"
            };
        }

        private static RoleProfile CreateReportingAnalyst()
        {
            return new RoleProfile
            {
                Id = Guid.NewGuid(),
                RoleCode = "RA_Report",
                RoleName = "Reporting Analyst",
                Layer = "Support",
                Department = "Reporting & Analytics",
                DisplayOrder = 15,
                Description = "Generates workflow and compliance reports",
                Scope = "Report generation, data analysis, compliance metrics, workflow reporting",
                Responsibilities = JsonSerializer.Serialize(new[]
                {
                    "Generate workflow reports",
                    "Create compliance reports",
                    "Analyze metrics",
                    "Data consolidation",
                    "Report distribution",
                    "Analytics support"
                }),
                ApprovalLevel = 0,
                ApprovalAuthority = null,
                CanEscalate = false,
                CanApprove = false,
                CanReject = false,
                CanReassign = false,
                IsActive = true,
                TenantId = null,
                ParticipatingWorkflows = "WF-NCA-ECC-001,WF-SAMA-CSF-001,WF-ERM-001"
            };
        }
    }
}

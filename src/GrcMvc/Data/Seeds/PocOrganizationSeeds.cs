using System;
using System.Collections.Generic;
using System.Text.Json;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Entities.Catalogs;

namespace GrcMvc.Data.Seeds
{
    /// <summary>
    /// POC Organization Seed Data for Shahin-AI Tenant
    /// Complete end-to-end demonstration of the GRC onboarding process
    /// 15 seeded data modules grouped by POC sections
    /// </summary>
    public static class PocOrganizationSeeds
    {
        // Fixed GUIDs for consistent seeding
        public static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid AdminUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static readonly Guid WizardId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        public static readonly Guid OrgProfileId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        public static readonly Guid PlanId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        public static readonly Guid TeamId = Guid.Parse("66666666-6666-6666-6666-666666666666");

        // Assessment IDs
        public static readonly Guid Assessment1Id = Guid.Parse("77777777-7777-7777-7777-777777777771");
        public static readonly Guid Assessment2Id = Guid.Parse("77777777-7777-7777-7777-777777777772");
        public static readonly Guid Assessment3Id = Guid.Parse("77777777-7777-7777-7777-777777777773");

        /// <summary>
        /// POC Section 1: Tenant (Multi-Tenant Foundation)
        /// </summary>
        public static Tenant GetPocTenant()
        {
            return new Tenant
            {
                Id = TenantId,
                TenantSlug = "shahin-ai",
                OrganizationName = "Shahin AI Technologies",
                AdminEmail = "admin@shahin-ai.com",
                Status = "Active",
                SubscriptionTier = "Enterprise",
                ActivatedAt = DateTime.UtcNow.AddDays(-30),
                ActivationToken = string.Empty,
                CreatedDate = DateTime.UtcNow.AddDays(-30),
                CreatedBy = "system"
            };
        }

        /// <summary>
        /// POC Section 2: Onboarding Wizard (Complete 12-Step Data)
        /// </summary>
        public static OnboardingWizard GetPocOnboardingWizard()
        {
            return new OnboardingWizard
            {
                Id = WizardId,
                TenantId = TenantId,

                // Section A: Organization Identity
                OrganizationLegalNameEn = "Shahin AI Technologies Ltd.",
                OrganizationLegalNameAr = "شركة شاهين للتقنيات الذكية المحدودة",
                TradeName = "Shahin AI",
                CountryOfIncorporation = "SA",
                PrimaryHqLocation = "Riyadh, Saudi Arabia",
                DefaultTimezone = "Asia/Riyadh",
                OperatingCountriesJson = "[\"SA\", \"AE\", \"EG\"]",
                PrimaryLanguage = "bilingual",
                CorporateEmailDomainsJson = "[\"shahin-ai.com\", \"shahinai.sa\"]",
                DomainVerificationMethod = "admin_email",
                OrganizationType = "enterprise",
                IndustrySector = "Technology",
                BusinessLinesJson = "[\"ict_services\", \"corporate\"]",
                HasDataResidencyRequirement = true,
                DataResidencyCountriesJson = "[\"SA\"]",

                // Section B: Assurance Objective
                PrimaryDriver = "regulatory",
                DesiredMaturity = "Managed",
                TargetTimeline = DateTime.UtcNow.AddMonths(6),

                // Section C: Regulatory Applicability
                PrimaryRegulatorsJson = "[{\"RegulatorCode\":\"NCA\",\"RegulatorName\":\"National Cybersecurity Authority\"},{\"RegulatorCode\":\"SDAIA\",\"RegulatorName\":\"Saudi Data & AI Authority\"}]",
                SecondaryRegulatorsJson = "[\"CST\"]",
                MandatoryFrameworksJson = "[\"NCA-ECC\", \"PDPL\"]",
                OptionalFrameworksJson = "[\"ISO27001\"]",
                CertificationsHeldJson = "[]",

                // Section D: Scope Definition
                InScopeLegalEntitiesJson = "[\"Shahin AI Technologies Ltd.\", \"Shahin Cloud Services\"]",
                InScopeBusinessUnitsJson = "[\"Engineering\", \"Operations\", \"Sales\", \"HR\"]",
                InScopeSystemsJson = "[\"ERP System\", \"CRM Platform\", \"AI Engine\", \"Cloud Infrastructure\"]",
                ExclusionsJson = "[]",

                // Section E: Data & Risk Profile
                DataTypesProcessedJson = "[\"PII\", \"PHI\", \"Financial\"]",
                HasPaymentCardData = false,
                HasCrossBorderDataTransfers = true,
                CrossBorderTransferCountriesJson = "[\"AE\", \"EG\"]",
                HasThirdPartyDataProcessing = true,
                ThirdPartyDataProcessorsJson = "[\"AWS\", \"Microsoft Azure\"]",

                // Section F: Technology Landscape
                IdentityProvider = "AzureAD",
                ItsmPlatform = "ServiceNow",
                CloudProvidersJson = "[\"AWS\", \"Azure\"]",
                SiemPlatform = "Splunk",

                // Section G: Control Ownership
                ControlOwnershipApproach = "hybrid",
                ExceptionApproverRole = "CISO",

                // Section H: Teams & RACI
                OrgAdminsJson = "[{\"Name\":\"Ahmed Al-Rashid\",\"Email\":\"ahmed@shahin-ai.com\",\"Role\":\"CISO\"},{\"Name\":\"Sara Al-Mutairi\",\"Email\":\"sara@shahin-ai.com\",\"Role\":\"Compliance Officer\"}]",
                CreateTeamsNow = true,
                TeamListJson = "[{\"TeamName\":\"Security Team\",\"TeamCode\":\"SEC\",\"RoleCode\":\"SECURITY_OFFICER\"},{\"TeamName\":\"Compliance Team\",\"TeamCode\":\"COMP\",\"RoleCode\":\"COMPLIANCE_OFFICER\"}]",
                RaciMappingNeeded = true,

                // Section I: Workflow & SLA
                EvidenceFrequencyDefaultsJson = "{\"standard\":90,\"high\":30,\"critical\":7}",
                RemediationSlaJson = "{\"critical\":7,\"high\":14,\"medium\":30,\"low\":90}",

                // Section J: Evidence Rules
                EvidenceRetentionYears = 7,
                AcceptableEvidenceTypesJson = "[\"Document\", \"Screenshot\", \"SystemReport\", \"Attestation\"]",

                // Section K: Baseline Configuration
                AdoptDefaultBaseline = true,
                SelectedOverlaysJson = "[\"sector:Technology\", \"regulator:NCA\"]",

                // Progress Tracking
                CurrentStep = 12,
                ProgressPercent = 100,
                WizardStatus = "Completed",
                CompletedAt = DateTime.UtcNow.AddDays(-25),
                CompletedByUserId = AdminUserId.ToString(),

                CreatedDate = DateTime.UtcNow.AddDays(-30),
                CreatedBy = "system"
            };
        }

        /// <summary>
        /// POC Section 3: Organization Profile (Derived from Wizard)
        /// </summary>
        public static OrganizationProfile GetPocOrganizationProfile()
        {
            return new OrganizationProfile
            {
                Id = OrgProfileId,
                TenantId = TenantId,
                LegalEntityName = "Shahin AI Technologies Ltd.",
                LegalEntityNameAr = "شركة شاهين للتقنيات الذكية المحدودة",
                Country = "SA",
                Sector = "Technology",
                OrganizationType = "enterprise",
                OperatingCountries = "[\"SA\", \"AE\", \"EG\"]",
                HeadquartersLocation = "Riyadh, Saudi Arabia",
                DataTypes = "[\"PII\", \"PHI\", \"Financial\"]",
                ProcessesPersonalData = true,
                ProcessesSensitiveData = true,
                HasThirdPartyDataProcessing = true,
                HostingModel = "Cloud",
                CloudProviders = "[\"AWS\", \"Azure\"]",
                ItSystemsJson = "[\"ERP System\", \"CRM Platform\", \"AI Engine\", \"Cloud Infrastructure\"]",
                PrimaryRegulator = "[\"NCA\", \"SDAIA\"]",
                SecondaryRegulators = "[\"CST\"]",
                RegulatoryCertifications = "[]",
                ComplianceOfficerName = "Sara Al-Mutairi",
                ComplianceOfficerEmail = "sara@shahin-ai.com",
                OnboardingStartedAt = DateTime.UtcNow.AddDays(-30),
                OnboardingCompletedAt = DateTime.UtcNow.AddDays(-25),
                OnboardingCompletedBy = AdminUserId.ToString(),
                OnboardingStatus = "Completed",
                OnboardingProgressPercent = 100,
                LastScopeDerivedAt = DateTime.UtcNow.AddDays(-25),
                CreatedDate = DateTime.UtcNow.AddDays(-30),
                CreatedBy = "system"
            };
        }

        /// <summary>
        /// POC Section 4: Derived Scope - Tenant Baselines
        /// </summary>
        public static List<TenantBaseline> GetPocTenantBaselines()
        {
            return new List<TenantBaseline>
            {
                new TenantBaseline
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    BaselineCode = "TECH-BASELINE",
                    BaselineName = "Technology Sector Baseline",
                    Applicability = "auto",
                    ReasonJson = "{\"rule\": \"sector_match\", \"sector\": \"Technology\", \"confidence\": 95}",
                    CreatedDate = DateTime.UtcNow.AddDays(-25),
                    CreatedBy = "rules_engine"
                },
                new TenantBaseline
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    BaselineCode = "NCA-BASELINE",
                    BaselineName = "NCA Regulated Entity Baseline",
                    Applicability = "auto",
                    ReasonJson = "{\"rule\": \"regulator_match\", \"regulator\": \"NCA\", \"confidence\": 100}",
                    CreatedDate = DateTime.UtcNow.AddDays(-25),
                    CreatedBy = "rules_engine"
                }
            };
        }

        /// <summary>
        /// POC Section 5: Derived Scope - Tenant Packages
        /// </summary>
        public static List<TenantPackage> GetPocTenantPackages()
        {
            return new List<TenantPackage>
            {
                new TenantPackage
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    PackageCode = "NCA-ECC-PKG",
                    PackageName = "NCA Essential Cybersecurity Controls",
                    Applicability = "mandatory",
                    ReasonJson = "{\"rule\": \"framework_required\", \"framework\": \"NCA-ECC\", \"mandatory\": true}",
                    CreatedDate = DateTime.UtcNow.AddDays(-25),
                    CreatedBy = "rules_engine"
                },
                new TenantPackage
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    PackageCode = "PDPL-PKG",
                    PackageName = "Personal Data Protection Law",
                    Applicability = "mandatory",
                    ReasonJson = "{\"rule\": \"data_type_match\", \"dataTypes\": [\"PII\"], \"mandatory\": true}",
                    CreatedDate = DateTime.UtcNow.AddDays(-25),
                    CreatedBy = "rules_engine"
                },
                new TenantPackage
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    PackageCode = "ISO27001-PKG",
                    PackageName = "ISO 27001 Information Security",
                    Applicability = "voluntary",
                    ReasonJson = "{\"rule\": \"certification_goal\", \"certification\": \"ISO27001\", \"voluntary\": true}",
                    CreatedDate = DateTime.UtcNow.AddDays(-25),
                    CreatedBy = "rules_engine"
                }
            };
        }

        /// <summary>
        /// POC Section 6: GRC Plan
        /// </summary>
        public static Plan GetPocPlan()
        {
            return new Plan
            {
                Id = PlanId,
                TenantId = TenantId,
                PlanCode = "PLAN-20260106-001",
                Name = "Shahin AI - Initial Compliance Plan",
                Description = "Comprehensive compliance plan covering NCA-ECC, PDPL, and ISO 27001",
                PlanType = "Full",
                Status = "InProgress",
                StartDate = DateTime.UtcNow.AddDays(-20),
                TargetEndDate = DateTime.UtcNow.AddMonths(6),
                CreatedDate = DateTime.UtcNow.AddDays(-25),
                CreatedBy = "system"
            };
        }

        /// <summary>
        /// POC Section 7: Plan Phases
        /// </summary>
        public static List<PlanPhase> GetPocPlanPhases()
        {
            return new List<PlanPhase>
            {
                new PlanPhase
                {
                    Id = Guid.NewGuid(),
                    PlanId = PlanId,
                    PhaseCode = "PHASE-1",
                    Name = "Quick Scan",
                    Description = "Initial gap assessment and baseline establishment",
                    Sequence = 1,
                    PlannedStartDate = DateTime.UtcNow.AddDays(-20),
                    PlannedEndDate = DateTime.UtcNow.AddDays(10),
                    Status = "InProgress",
                    CreatedDate = DateTime.UtcNow.AddDays(-25),
                    CreatedBy = "system"
                },
                new PlanPhase
                {
                    Id = Guid.NewGuid(),
                    PlanId = PlanId,
                    PhaseCode = "PHASE-2",
                    Name = "Full Assessment",
                    Description = "Detailed control assessment and evidence collection",
                    Sequence = 2,
                    PlannedStartDate = DateTime.UtcNow.AddDays(11),
                    PlannedEndDate = DateTime.UtcNow.AddMonths(3),
                    Status = "Planned",
                    CreatedDate = DateTime.UtcNow.AddDays(-25),
                    CreatedBy = "system"
                },
                new PlanPhase
                {
                    Id = Guid.NewGuid(),
                    PlanId = PlanId,
                    PhaseCode = "PHASE-3",
                    Name = "Remediation",
                    Description = "Address gaps and implement controls",
                    Sequence = 3,
                    PlannedStartDate = DateTime.UtcNow.AddMonths(3),
                    PlannedEndDate = DateTime.UtcNow.AddMonths(6),
                    Status = "Planned",
                    CreatedDate = DateTime.UtcNow.AddDays(-25),
                    CreatedBy = "system"
                }
            };
        }

        /// <summary>
        /// POC Section 8: Assessments
        /// </summary>
        public static List<Assessment> GetPocAssessments()
        {
            return new List<Assessment>
            {
                new Assessment
                {
                    Id = Assessment1Id,
                    TenantId = TenantId,
                    AssessmentNumber = "ASM-20260106-001",
                    AssessmentCode = "NCA-ECC-2024",
                    Name = "NCA ECC Assessment 2024",
                    Description = "Essential Cybersecurity Controls assessment for NCA compliance",
                    Type = "Compliance",
                    FrameworkCode = "NCA-ECC",
                    TemplateCode = "TPL-NCA-ECC",
                    PlanId = PlanId,
                    Status = "InProgress",
                    StartDate = DateTime.UtcNow.AddDays(-15),
                    DueDate = DateTime.UtcNow.AddDays(45),
                    AssignedTo = "sara@shahin-ai.com",
                    CreatedDate = DateTime.UtcNow.AddDays(-20),
                    CreatedBy = "system"
                },
                new Assessment
                {
                    Id = Assessment2Id,
                    TenantId = TenantId,
                    AssessmentNumber = "ASM-20260106-002",
                    AssessmentCode = "PDPL-PIA-2024",
                    Name = "PDPL Privacy Impact Assessment",
                    Description = "Privacy impact assessment for PDPL compliance",
                    Type = "Privacy",
                    FrameworkCode = "PDPL",
                    TemplateCode = "TPL-PDPL",
                    PlanId = PlanId,
                    Status = "Pending",
                    StartDate = DateTime.UtcNow.AddDays(10),
                    DueDate = DateTime.UtcNow.AddDays(70),
                    AssignedTo = "ahmed@shahin-ai.com",
                    CreatedDate = DateTime.UtcNow.AddDays(-20),
                    CreatedBy = "system"
                },
                new Assessment
                {
                    Id = Assessment3Id,
                    TenantId = TenantId,
                    AssessmentNumber = "ASM-20260106-003",
                    AssessmentCode = "ISO27001-2024",
                    Name = "ISO 27001 Gap Assessment",
                    Description = "Gap assessment for ISO 27001 certification readiness",
                    Type = "Certification",
                    FrameworkCode = "ISO27001",
                    TemplateCode = "TPL-ISO27001",
                    PlanId = PlanId,
                    Status = "Pending",
                    StartDate = DateTime.UtcNow.AddMonths(1),
                    DueDate = DateTime.UtcNow.AddMonths(4),
                    AssignedTo = "sara@shahin-ai.com",
                    CreatedDate = DateTime.UtcNow.AddDays(-20),
                    CreatedBy = "system"
                }
            };
        }

        /// <summary>
        /// POC Section 9: Team
        /// </summary>
        public static Team GetPocTeam()
        {
            return new Team
            {
                Id = TeamId,
                TenantId = TenantId,
                TeamCode = "GRC-TEAM",
                Name = "GRC Core Team",
                Description = "Core team responsible for GRC activities",
                Purpose = "GRC Operations",
                TeamType = "Governance",
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-25),
                CreatedBy = "system"
            };
        }

        /// <summary>
        /// POC Section 10: Team Members
        /// </summary>
        public static List<TeamMember> GetPocTeamMembers()
        {
            return new List<TeamMember>
            {
                new TeamMember
                {
                    Id = Guid.NewGuid(),
                    TeamId = TeamId,
                    UserId = AdminUserId,
                    RoleCode = "CISO",
                    IsPrimaryForRole = true,
                    CanApprove = true,
                    CanDelegate = true,
                    JoinedDate = DateTime.UtcNow.AddDays(-25),
                    CreatedDate = DateTime.UtcNow.AddDays(-25),
                    CreatedBy = "system"
                },
                new TeamMember
                {
                    Id = Guid.NewGuid(),
                    TeamId = TeamId,
                    UserId = Guid.Parse("22222222-2222-2222-2222-222222222223"),
                    RoleCode = "COMPLIANCE_OFFICER",
                    IsPrimaryForRole = true,
                    CanApprove = true,
                    CanDelegate = true,
                    JoinedDate = DateTime.UtcNow.AddDays(-25),
                    CreatedDate = DateTime.UtcNow.AddDays(-25),
                    CreatedBy = "system"
                },
                new TeamMember
                {
                    Id = Guid.NewGuid(),
                    TeamId = TeamId,
                    UserId = Guid.Parse("22222222-2222-2222-2222-222222222224"),
                    RoleCode = "SECURITY_OFFICER",
                    IsPrimaryForRole = true,
                    CanApprove = false,
                    CanDelegate = false,
                    JoinedDate = DateTime.UtcNow.AddDays(-20),
                    CreatedDate = DateTime.UtcNow.AddDays(-20),
                    CreatedBy = "system"
                }
            };
        }

        /// <summary>
        /// POC Section 11: RACI Assignments
        /// </summary>
        public static List<RACIAssignment> GetPocRaciAssignments()
        {
            return new List<RACIAssignment>
            {
                new RACIAssignment
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    ScopeType = "Assessment",
                    ScopeId = "NCA-ECC-ASSESSMENT",
                    TeamId = TeamId,
                    RACI = "R",
                    RoleCode = "COMPLIANCE_OFFICER",
                    Priority = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-25),
                    CreatedBy = "system"
                },
                new RACIAssignment
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    ScopeType = "Evidence",
                    ScopeId = "EVIDENCE-COLLECTION",
                    TeamId = TeamId,
                    RACI = "A",
                    RoleCode = "CISO",
                    Priority = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-25),
                    CreatedBy = "system"
                }
            };
        }

        /// <summary>
        /// POC Section 12: Evidence (Sample)
        /// </summary>
        public static List<Evidence> GetPocEvidence()
        {
            return new List<Evidence>
            {
                new Evidence
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    EvidenceNumber = "EVD-20260106-001",
                    Title = "Information Security Policy v2.0",
                    Description = "Approved information security policy document",
                    Type = "Document",
                    FileName = "InfoSec_Policy_v2.0.pdf",
                    MimeType = "application/pdf",
                    FileSize = 245000,
                    FilePath = "/evidence/shahin-ai/policies/",
                    CollectedBy = "ahmed@shahin-ai.com",
                    CollectionDate = DateTime.UtcNow.AddDays(-10),
                    VerificationStatus = "Verified",
                    VerifiedBy = "sara@shahin-ai.com",
                    VerificationDate = DateTime.UtcNow.AddDays(-8),
                    AssessmentId = Assessment1Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-10),
                    CreatedBy = "ahmed@shahin-ai.com"
                },
                new Evidence
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    EvidenceNumber = "EVD-20260106-002",
                    Title = "Access Control Matrix",
                    Description = "User access rights matrix for critical systems",
                    Type = "SystemReport",
                    FileName = "Access_Control_Matrix_Q1.xlsx",
                    MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    FileSize = 85000,
                    FilePath = "/evidence/shahin-ai/access/",
                    CollectedBy = "security@shahin-ai.com",
                    CollectionDate = DateTime.UtcNow.AddDays(-5),
                    VerificationStatus = "Pending",
                    AssessmentId = Assessment1Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-5),
                    CreatedBy = "security@shahin-ai.com"
                }
            };
        }

        /// <summary>
        /// POC Section 13: Workflow Instance
        /// </summary>
        public static WorkflowInstance GetPocWorkflowInstance()
        {
            return new WorkflowInstance
            {
                Id = Guid.NewGuid(),
                TenantId = TenantId,
                InstanceNumber = "WF-20260106-001",
                WorkflowType = "NCA-ECC-ASSESSMENT",
                CurrentState = "InProgress",
                Status = "Active",
                EntityType = "Assessment",
                EntityId = Assessment1Id,
                StartedAt = DateTime.UtcNow.AddDays(-15),
                SlaDueDate = DateTime.UtcNow.AddDays(45),
                SlaBreached = false,
                InitiatedByUserId = AdminUserId,
                InitiatedByUserName = "Ahmed Al-Rashid",
                CreatedDate = DateTime.UtcNow.AddDays(-15),
                CreatedBy = "system"
            };
        }

        /// <summary>
        /// POC Section 14: Audit Events
        /// </summary>
        public static List<AuditEvent> GetPocAuditEvents()
        {
            var correlationId = Guid.NewGuid().ToString();
            return new List<AuditEvent>
            {
                new AuditEvent
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    EventId = "EVT-001",
                    EventType = "TenantActivated",
                    CorrelationId = correlationId,
                    AffectedEntityType = "Tenant",
                    AffectedEntityId = TenantId.ToString(),
                    Actor = "system",
                    Action = "Tenant activated after email verification",
                    Status = "Completed",
                    EventTimestamp = DateTime.UtcNow.AddDays(-30),
                    CreatedDate = DateTime.UtcNow.AddDays(-30),
                    CreatedBy = "system"
                },
                new AuditEvent
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    EventId = "EVT-002",
                    EventType = "OnboardingCompleted",
                    CorrelationId = correlationId,
                    AffectedEntityType = "OnboardingWizard",
                    AffectedEntityId = WizardId.ToString(),
                    Actor = AdminUserId.ToString(),
                    Action = "Onboarding wizard completed with 12/12 steps",
                    Status = "Completed",
                    EventTimestamp = DateTime.UtcNow.AddDays(-25),
                    CreatedDate = DateTime.UtcNow.AddDays(-25),
                    CreatedBy = "system"
                },
                new AuditEvent
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    EventId = "EVT-003",
                    EventType = "ScopeGenerated",
                    CorrelationId = correlationId,
                    AffectedEntityType = "TenantScope",
                    AffectedEntityId = TenantId.ToString(),
                    Actor = "rules_engine",
                    Action = "Derived scope: 2 baselines, 3 packages, 5 templates",
                    Status = "Completed",
                    EventTimestamp = DateTime.UtcNow.AddDays(-25),
                    CreatedDate = DateTime.UtcNow.AddDays(-25),
                    CreatedBy = "system"
                },
                new AuditEvent
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    EventId = "EVT-004",
                    EventType = "PlanCreated",
                    CorrelationId = correlationId,
                    AffectedEntityType = "Plan",
                    AffectedEntityId = PlanId.ToString(),
                    Actor = "system",
                    Action = "Initial compliance plan created with 3 phases",
                    Status = "Completed",
                    EventTimestamp = DateTime.UtcNow.AddDays(-25),
                    CreatedDate = DateTime.UtcNow.AddDays(-25),
                    CreatedBy = "system"
                },
                new AuditEvent
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    EventId = "EVT-005",
                    EventType = "AssessmentCreated",
                    CorrelationId = correlationId,
                    AffectedEntityType = "Assessment",
                    AffectedEntityId = Assessment1Id.ToString(),
                    Actor = "system",
                    Action = "NCA ECC Assessment created and assigned",
                    Status = "Completed",
                    EventTimestamp = DateTime.UtcNow.AddDays(-20),
                    CreatedDate = DateTime.UtcNow.AddDays(-20),
                    CreatedBy = "system"
                }
            };
        }

        /// <summary>
        /// POC Section 15: Policy Decisions (Smart Logic Audit)
        /// </summary>
        public static List<PolicyDecision> GetPocPolicyDecisions()
        {
            return new List<PolicyDecision>
            {
                new PolicyDecision
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    PolicyType = "ScopeDerivation",
                    PolicyVersion = "1.0",
                    ContextHash = "sha256:abc123",
                    ContextJson = "{\"sector\": \"Technology\", \"regulators\": [\"NCA\", \"SDAIA\"]}",
                    Decision = "DeriveScope",
                    Reason = "Technology sector with NCA regulator requires ECC baseline",
                    RulesEvaluated = 12,
                    RulesMatched = 4,
                    ConfidenceScore = 95,
                    EvaluatedAt = DateTime.UtcNow.AddDays(-25),
                    ExpiresAt = DateTime.UtcNow.AddDays(5),
                    IsCached = true,
                    RelatedEntityType = "Tenant",
                    RelatedEntityId = TenantId,
                    CreatedDate = DateTime.UtcNow.AddDays(-25),
                    CreatedBy = "rules_engine"
                },
                new PolicyDecision
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    PolicyType = "PriorityCalculation",
                    PolicyVersion = "1.0",
                    ContextHash = "sha256:def456",
                    ContextJson = "{\"framework\": \"NCA-ECC\", \"hasUpcomingAudit\": true}",
                    Decision = "High",
                    Reason = "NCA-ECC is mandatory framework with upcoming audit",
                    RulesEvaluated = 5,
                    RulesMatched = 2,
                    ConfidenceScore = 100,
                    EvaluatedAt = DateTime.UtcNow.AddDays(-20),
                    ExpiresAt = DateTime.UtcNow.AddDays(10),
                    IsCached = true,
                    RelatedEntityType = "Assessment",
                    RelatedEntityId = Assessment1Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-20),
                    CreatedBy = "rules_engine"
                }
            };
        }
    }
}

using GrcMvc.Services.Interfaces;
using GrcMvc.Models.DTOs;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GrcMvc.Agents
{
    /// <summary>
    /// AI-powered onboarding agent that provides Fast Start recommendations
    /// and generates mission-based onboarding plans
    /// </summary>
    public interface IOnboardingAgent
    {
        Task<OnboardingRecommendation> GetFastStartRecommendationsAsync(Guid tenantId);
        Task<List<Mission>> GenerateMissionsAsync(Guid tenantId, OnboardingAnswers answers);
        Task<ProgressReport> TrackProgressAsync(Guid tenantId);
        Task<List<string>> GetNextBestActionsAsync(Guid tenantId);
        Task<FrameworkRecommendation> RecommendFrameworksAsync(Guid tenantId, OnboardingAnswers answers);
    }

    public class OnboardingAgent : IOnboardingAgent
    {
        private readonly IOnboardingService _onboardingService;
        private readonly ILlmService _llmService;
        private readonly ITenantService _tenantService;
        private readonly ILogger<OnboardingAgent> _logger;

        public OnboardingAgent(
            IOnboardingService onboardingService,
            ILlmService llmService,
            ITenantService tenantService,
            ILogger<OnboardingAgent> logger)
        {
            _onboardingService = onboardingService ?? throw new ArgumentNullException(nameof(onboardingService));
            _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService));
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generate Fast Start recommendations based on organization profile
        /// </summary>
        public async Task<OnboardingRecommendation> GetFastStartRecommendationsAsync(Guid tenantId)
        {
            _logger.LogInformation($"Generating Fast Start recommendations for tenant {tenantId}");

            try
            {
                // Get tenant and onboarding data
                var tenant = await _tenantService.GetByIdAsync(tenantId);
                if (tenant == null)
                {
                    throw new InvalidOperationException($"Tenant {tenantId} not found");
                }

                var onboardingData = await _onboardingService.GetOnboardingDataAsync(tenantId);
                if (onboardingData == null)
                {
                    _logger.LogWarning($"No onboarding data found for tenant {tenantId}");
                    return GetDefaultRecommendations(tenantId);
                }

                // Build AI prompt
                var prompt = BuildFastStartPrompt(onboardingData);

                // Get AI recommendations
                var aiResponse = await _llmService.GenerateCompletionAsync(prompt);

                // Parse and structure recommendations
                var recommendations = ParseRecommendations(aiResponse);

                return new OnboardingRecommendation
                {
                    TenantId = tenantId,
                    OrganizationName = tenant.Name,
                    Recommendations = recommendations,
                    Priority = "High",
                    EstimatedTimeToComplete = CalculateEstimatedTime(recommendations),
                    GeneratedAt = DateTime.UtcNow,
                    GeneratedBy = "OnboardingAgent"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating Fast Start recommendations for tenant {tenantId}");
                return GetDefaultRecommendations(tenantId);
            }
        }

        /// <summary>
        /// Generate mission-based onboarding plan
        /// </summary>
        public async Task<List<Mission>> GenerateMissionsAsync(Guid tenantId, OnboardingAnswers answers)
        {
            _logger.LogInformation($"Generating missions for tenant {tenantId}");

            var missions = new List<Mission>();

            try
            {
                // Mission 1: Framework Selection
                missions.Add(new Mission
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Title = "Select Compliance Frameworks",
                    Description = "Choose applicable frameworks based on your industry and regulatory requirements",
                    Priority = 1,
                    EstimatedHours = 2,
                    Status = "NotStarted",
                    Category = "Setup",
                    Dependencies = new List<Guid>(),
                    Deliverables = new List<string>
                    {
                        "List of applicable frameworks",
                        "Framework priority ranking",
                        "Implementation timeline"
                    }
                });

                // Mission 2: Team Setup
                missions.Add(new Mission
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Title = "Setup GRC Team",
                    Description = "Assign roles and responsibilities to team members",
                    Priority = 2,
                    EstimatedHours = 4,
                    Status = "NotStarted",
                    Category = "Team",
                    Dependencies = new List<Guid> { missions[0].Id },
                    Deliverables = new List<string>
                    {
                        "RACI matrix",
                        "Role assignments",
                        "Team member invitations sent"
                    }
                });

                // Mission 3: Asset Inventory
                missions.Add(new Mission
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Title = "Create Asset Inventory",
                    Description = "Document all IT assets, systems, and data repositories",
                    Priority = 3,
                    EstimatedHours = 8,
                    Status = "NotStarted",
                    Category = "Documentation",
                    Dependencies = new List<Guid> { missions[1].Id },
                    Deliverables = new List<string>
                    {
                        "Complete asset inventory",
                        "Data classification matrix",
                        "System criticality ratings"
                    }
                });

                // Mission 4: Risk Assessment
                missions.Add(new Mission
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Title = "Conduct Initial Risk Assessment",
                    Description = "Identify and assess key risks to the organization",
                    Priority = 4,
                    EstimatedHours = 12,
                    Status = "NotStarted",
                    Category = "Risk",
                    Dependencies = new List<Guid> { missions[2].Id },
                    Deliverables = new List<string>
                    {
                        "Risk register",
                        "Risk heat map",
                        "Mitigation priorities"
                    }
                });

                // Mission 5: Control Implementation
                missions.Add(new Mission
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Title = "Implement Priority Controls",
                    Description = "Deploy controls to address high-priority risks",
                    Priority = 5,
                    EstimatedHours = 20,
                    Status = "NotStarted",
                    Category = "Implementation",
                    Dependencies = new List<Guid> { missions[3].Id },
                    Deliverables = new List<string>
                    {
                        "Control implementation plan",
                        "Deployed controls documentation",
                        "Control effectiveness metrics"
                    }
                });

                // Mission 6: Evidence Collection
                missions.Add(new Mission
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Title = "Establish Evidence Collection",
                    Description = "Setup automated evidence collection for compliance",
                    Priority = 6,
                    EstimatedHours = 6,
                    Status = "NotStarted",
                    Category = "Compliance",
                    Dependencies = new List<Guid> { missions[4].Id },
                    Deliverables = new List<string>
                    {
                        "Evidence collection procedures",
                        "Automated collection rules",
                        "Evidence repository setup"
                    }
                });

                // Mission 7: Monitoring & Reporting
                missions.Add(new Mission
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Title = "Configure Monitoring & Reporting",
                    Description = "Setup dashboards and automated reporting",
                    Priority = 7,
                    EstimatedHours = 4,
                    Status = "NotStarted",
                    Category = "Monitoring",
                    Dependencies = new List<Guid> { missions[5].Id },
                    Deliverables = new List<string>
                    {
                        "Executive dashboard",
                        "Automated compliance reports",
                        "Alert configurations"
                    }
                });

                _logger.LogInformation($"Generated {missions.Count} missions for tenant {tenantId}");
                return missions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating missions for tenant {tenantId}");
                throw;
            }
        }

        /// <summary>
        /// Track onboarding progress and provide insights
        /// </summary>
        public async Task<ProgressReport> TrackProgressAsync(Guid tenantId)
        {
            _logger.LogInformation($"Tracking progress for tenant {tenantId}");

            try
            {
                var onboardingStatus = await _onboardingService.GetStatusAsync(tenantId);
                
                var totalSteps = 12; // 12-step wizard
                var completedSteps = onboardingStatus?.CompletedSteps ?? 0;
                var completionPercentage = (completedSteps * 100) / totalSteps;

                return new ProgressReport
                {
                    TenantId = tenantId,
                    CompletionPercentage = completionPercentage,
                    CompletedSteps = completedSteps,
                    TotalSteps = totalSteps,
                    RemainingSteps = totalSteps - completedSteps,
                    EstimatedTimeToComplete = (totalSteps - completedSteps) * 2, // 2 hours per step
                    CurrentPhase = GetCurrentPhase(completedSteps),
                    NextMilestone = GetNextMilestone(completedSteps),
                    ReportGeneratedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error tracking progress for tenant {tenantId}");
                throw;
            }
        }

        /// <summary>
        /// Get next best actions for the organization
        /// </summary>
        public async Task<List<string>> GetNextBestActionsAsync(Guid tenantId)
        {
            _logger.LogInformation($"Getting next best actions for tenant {tenantId}");

            var actions = new List<string>();

            try
            {
                var progress = await TrackProgressAsync(tenantId);

                if (progress.CompletionPercentage < 25)
                {
                    actions.Add("Complete the onboarding wizard to establish your GRC foundation");
                    actions.Add("Invite team members to collaborate on GRC activities");
                }
                else if (progress.CompletionPercentage < 50)
                {
                    actions.Add("Conduct initial risk assessment");
                    actions.Add("Document your IT asset inventory");
                }
                else if (progress.CompletionPercentage < 75)
                {
                    actions.Add("Implement priority controls");
                    actions.Add("Setup evidence collection procedures");
                }
                else
                {
                    actions.Add("Configure monitoring dashboards");
                    actions.Add("Schedule first compliance audit");
                    actions.Add("Review and approve GRC policies");
                }

                return actions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting next best actions for tenant {tenantId}");
                return new List<string> { "Complete onboarding wizard", "Contact support for assistance" };
            }
        }

        /// <summary>
        /// Recommend compliance frameworks based on organization profile
        /// </summary>
        public async Task<FrameworkRecommendation> RecommendFrameworksAsync(Guid tenantId, OnboardingAnswers answers)
        {
            _logger.LogInformation($"Recommending frameworks for tenant {tenantId}");

            try
            {
                var recommendations = new List<FrameworkItem>();

                // Industry-based recommendations
                if (answers.Industry?.Contains("Financial") == true)
                {
                    recommendations.Add(new FrameworkItem
                    {
                        Name = "PCI DSS",
                        Priority = "High",
                        Reason = "Required for payment card processing",
                        EstimatedEffort = "High"
                    });
                }

                if (answers.Industry?.Contains("Healthcare") == true)
                {
                    recommendations.Add(new FrameworkItem
                    {
                        Name = "HIPAA",
                        Priority = "High",
                        Reason = "Required for healthcare data protection",
                        EstimatedEffort = "High"
                    });
                }

                // Region-based recommendations
                if (answers.Region?.Contains("Saudi Arabia") == true)
                {
                    recommendations.Add(new FrameworkItem
                    {
                        Name = "NCA ECC",
                        Priority = "High",
                        Reason = "Saudi Arabia cybersecurity compliance",
                        EstimatedEffort = "Medium"
                    });
                }

                // Default recommendations
                recommendations.Add(new FrameworkItem
                {
                    Name = "ISO 27001",
                    Priority = "Medium",
                    Reason = "International information security standard",
                    EstimatedEffort = "High"
                });

                return new FrameworkRecommendation
                {
                    TenantId = tenantId,
                    Frameworks = recommendations,
                    GeneratedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error recommending frameworks for tenant {tenantId}");
                throw;
            }
        }

        #region Private Helper Methods

        private string BuildFastStartPrompt(object onboardingData)
        {
            return $@"You are a GRC (Governance, Risk, and Compliance) expert assistant.
Based on the following organization profile, provide Fast Start recommendations:

{JsonSerializer.Serialize(onboardingData, new JsonSerializerOptions { WriteIndented = true })}

Provide 5-7 actionable recommendations to help this organization quickly establish their GRC program.
Focus on quick wins and high-impact activities.
Format as a numbered list with brief explanations.";
        }

        private List<string> ParseRecommendations(string aiResponse)
        {
            // Simple parsing - split by newlines and filter numbered items
            return aiResponse
                .Split('\n')
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim())
                .Where(line => char.IsDigit(line.FirstOrDefault()))
                .ToList();
        }

        private int CalculateEstimatedTime(List<string> recommendations)
        {
            // Estimate 2 hours per recommendation
            return recommendations.Count * 2;
        }

        private OnboardingRecommendation GetDefaultRecommendations(Guid tenantId)
        {
            return new OnboardingRecommendation
            {
                TenantId = tenantId,
                Recommendations = new List<string>
                {
                    "Complete the 12-step onboarding wizard",
                    "Invite team members to collaborate",
                    "Conduct initial risk assessment",
                    "Document IT asset inventory",
                    "Select applicable compliance frameworks"
                },
                Priority = "High",
                EstimatedTimeToComplete = 10,
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = "OnboardingAgent (Default)"
            };
        }

        private string GetCurrentPhase(int completedSteps)
        {
            return completedSteps switch
            {
                < 4 => "Organization Setup",
                < 8 => "Compliance Configuration",
                < 12 => "Team & Process Setup",
                _ => "Completed"
            };
        }

        private string GetNextMilestone(int completedSteps)
        {
            return completedSteps switch
            {
                < 4 => "Complete organization profile",
                < 8 => "Configure compliance frameworks",
                < 12 => "Setup team and workflows",
                _ => "Begin GRC operations"
            };
        }

        #endregion
    }

    #region DTOs

    public class OnboardingRecommendation
    {
        public Guid TenantId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
        public string Priority { get; set; } = string.Empty;
        public int EstimatedTimeToComplete { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
    }

    public class Mission
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Priority { get; set; }
        public int EstimatedHours { get; set; }
        public string Status { get; set; } = "NotStarted";
        public string Category { get; set; } = string.Empty;
        public List<Guid> Dependencies { get; set; } = new();
        public List<string> Deliverables { get; set; } = new();
    }

    public class ProgressReport
    {
        public Guid TenantId { get; set; }
        public int CompletionPercentage { get; set; }
        public int CompletedSteps { get; set; }
        public int TotalSteps { get; set; }
        public int RemainingSteps { get; set; }
        public int EstimatedTimeToComplete { get; set; }
        public string CurrentPhase { get; set; } = string.Empty;
        public string NextMilestone { get; set; } = string.Empty;
        public DateTime ReportGeneratedAt { get; set; }
    }

    public class OnboardingAnswers
    {
        public string? Industry { get; set; }
        public string? Region { get; set; }
        public int? EmployeeCount { get; set; }
        public List<string>? ComplianceFrameworks { get; set; }
    }

    public class FrameworkRecommendation
    {
        public Guid TenantId { get; set; }
        public List<FrameworkItem> Frameworks { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class FrameworkItem
    {
        public string Name { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string EstimatedEffort { get; set; } = string.Empty;
    }

    #endregion
}

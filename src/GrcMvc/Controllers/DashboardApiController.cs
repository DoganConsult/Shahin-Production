using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Models;
using GrcMvc.Services.Interfaces;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Dashboard API Controller
    /// Handles REST API requests for dashboard data, analytics, and compliance metrics
    /// Route: /api/dashboard
    /// </summary>
    [Route("api/dashboard")]
    [ApiController]
    [Authorize]
    public class DashboardApiController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IAssessmentService _assessmentService;
        private readonly IRiskService _riskService;
        private readonly IControlService _controlService;

        public DashboardApiController(
            IReportService reportService,
            IAssessmentService assessmentService,
            IRiskService riskService,
            IControlService controlService)
        {
            _reportService = reportService;
            _assessmentService = assessmentService;
            _riskService = riskService;
            _controlService = controlService;
        }

        /// <summary>
        /// Get overall compliance dashboard
        /// Returns key metrics and compliance status overview
        /// </summary>
        [HttpGet("compliance")]
        public async Task<IActionResult> GetComplianceDashboard()
        {
            try
            {
                var assessments = await _assessmentService.GetAllAsync();
                var risks = await _riskService.GetAllAsync();
                var controls = await _controlService.GetAllAsync();

                var dashboard = new
                {
                    timestamp = DateTime.UtcNow,
                    summary = new
                    {
                        totalAssessments = assessments.Count(),
                        completedAssessments = assessments.Count(a => a.Status == "Completed"),
                        pendingAssessments = assessments.Count(a => a.Status == "Pending"),
                        totalRisks = risks.Count(),
                        highRisks = risks.Count(r => (r.Probability * r.Impact) >= 15),
                        totalControls = controls.Count(),
                        effectiveControls = controls.Count(c => c.Status == "Effective")
                    },
                    complianceScore = CalculateComplianceScore(assessments, controls),
                    riskLevel = CalculateOverallRiskLevel(risks),
                    trends = new
                    {
                        assessmentTrend = "Improving",
                        riskTrend = "Stable",
                        complianceTrend = "Improving"
                    }
                };

                return Ok(ApiResponse<object>.SuccessResponse(dashboard, "Compliance dashboard retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get risk dashboard
        /// Returns risk metrics and distribution
        /// </summary>
        [HttpGet("risk")]
        public async Task<IActionResult> GetRiskDashboard()
        {
            try
            {
                var risks = await _riskService.GetAllAsync();

                var dashboard = new
                {
                    timestamp = DateTime.UtcNow,
                    summary = new
                    {
                        totalRisks = risks.Count(),
                        critical = risks.Count(r => (r.Probability * r.Impact) >= 25),
                        high = risks.Count(r => (r.Probability * r.Impact) >= 15 && (r.Probability * r.Impact) < 25),
                        medium = risks.Count(r => (r.Probability * r.Impact) >= 10 && (r.Probability * r.Impact) < 15),
                        low = risks.Count(r => (r.Probability * r.Impact) < 10),
                        mitigated = risks.Count(r => r.Status == "Mitigated"),
                        active = risks.Count(r => r.Status == "Active")
                    },
                    distribution = new
                    {
                        byCategory = risks.GroupBy(r => r.Category).Select(g => new { category = g.Key, count = g.Count() }).ToList(),
                        byStatus = risks.GroupBy(r => r.Status).Select(g => new { status = g.Key, count = g.Count() }).ToList()
                    }
                };

                return Ok(ApiResponse<object>.SuccessResponse(dashboard, "Risk dashboard retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get assessment dashboard
        /// Returns assessment progress and completion metrics
        /// </summary>
        [HttpGet("assessments")]
        public async Task<IActionResult> GetAssessmentDashboard()
        {
            try
            {
                var assessments = await _assessmentService.GetAllAsync();

                var dashboard = new
                {
                    timestamp = DateTime.UtcNow,
                    summary = new
                    {
                        total = assessments.Count(),
                        completed = assessments.Count(a => a.Status == "Completed"),
                        inProgress = assessments.Count(a => a.Status == "InProgress"),
                        pending = assessments.Count(a => a.Status == "Pending"),
                        completionPercentage = assessments.Count() > 0 
                            ? (assessments.Count(a => a.Status == "Completed") * 100 / assessments.Count()) 
                            : 0
                    },
                    recentAssessments = assessments
                        .OrderByDescending(a => a.StartDate)
                        .Take(10)
                        .Select(a => new { a.Id, a.Name, a.Status, a.StartDate })
                        .ToList()
                };

                return Ok(ApiResponse<object>.SuccessResponse(dashboard, "Assessment dashboard retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get compliance metrics
        /// Returns detailed compliance metrics and KPIs
        /// </summary>
        [HttpGet("metrics")]
        public async Task<IActionResult> GetComplianceMetrics()
        {
            try
            {
                var assessments = await _assessmentService.GetAllAsync();
                var stats = await _assessmentService.GetStatisticsAsync();

                var metrics = new
                {
                    timestamp = DateTime.UtcNow,
                    assessmentMetrics = new
                    {
                        total = stats.TotalAssessments,
                        completed = stats.CompletedAssessments,
                        inProgress = stats.InProgressAssessments,
                        pending = stats.PendingAssessments,
                        overdue = stats.OverdueAssessments,
                        completionRate = stats.CompletionRate,
                        averageScore = stats.AverageScore
                    },
                    scoreTrend = assessments
                        .Where(a => a.Score > 0)
                        .GroupBy(a => a.StartDate.Month)
                        .Select(g => new { month = g.Key, averageScore = g.Average(a => a.Score) })
                        .ToList()
                };

                return Ok(ApiResponse<object>.SuccessResponse(metrics, "Compliance metrics retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get upcoming assessments
        /// Returns assessments scheduled for the next 30 days
        /// </summary>
        [HttpGet("upcoming-assessments")]
        public async Task<IActionResult> GetUpcomingAssessments([FromQuery] int days = 30)
        {
            try
            {
                var upcomingAssessments = await _assessmentService.GetUpcomingAssessmentsAsync(days);

                var dashboard = new
                {
                    period = $"Next {days} days",
                    timestamp = DateTime.UtcNow,
                    upcomingCount = upcomingAssessments.Count(),
                    assessments = upcomingAssessments
                        .OrderBy(a => a.StartDate)
                        .Select(a => new { a.Id, a.Name, a.Status, a.StartDate, a.AssignedTo })
                        .ToList()
                };

                return Ok(ApiResponse<object>.SuccessResponse(dashboard, "Upcoming assessments retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get control effectiveness
        /// Returns information about control implementation and effectiveness
        /// </summary>
        [HttpGet("control-effectiveness")]
        public async Task<IActionResult> GetControlEffectiveness()
        {
            try
            {
                var controls = await _controlService.GetAllAsync();
                var stats = await _controlService.GetStatisticsAsync();

                var dashboard = new
                {
                    timestamp = DateTime.UtcNow,
                    summary = new
                    {
                        totalControls = stats.TotalControls,
                        effective = stats.EffectiveControls,
                        ineffective = stats.IneffectiveControls,
                        tested = stats.TestedControls,
                        effectivenessRate = stats.EffectivenessRate
                    },
                    distribution = controls
                        .GroupBy(c => c.Status)
                        .Select(g => new { status = g.Key, count = g.Count() })
                        .ToList()
                };

                return Ok(ApiResponse<object>.SuccessResponse(dashboard, "Control effectiveness retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        // Helper methods
        private decimal CalculateComplianceScore(IEnumerable<dynamic> assessments, IEnumerable<dynamic> controls)
        {
            if (!assessments.Any()) return 0;
            return (decimal)assessments.Average(a => a.Score);
        }

        private string CalculateOverallRiskLevel(IEnumerable<dynamic> risks)
        {
            if (!risks.Any()) return "None";
            var criticalCount = risks.Count(r => (r.Probability * r.Impact) >= 25);
            if (criticalCount > 0) return "Critical";
            var highCount = risks.Count(r => (r.Probability * r.Impact) >= 15 && (r.Probability * r.Impact) < 25);
            if (highCount > 3) return "High";
            return "Medium";
        }
    }
}

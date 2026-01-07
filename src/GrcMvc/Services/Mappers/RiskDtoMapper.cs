using GrcMvc.Models.DTOs;
using GrcMvc.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrcMvc.Services.Mappers
{
    /// <summary>
    /// Maps between UI DTOs (Models.Dtos) and Service DTOs (Models.DTOs)
    /// This allows UI to use its own DTO structure while service layer remains unchanged
    /// </summary>
    public static class RiskDtoMapper
    {
        // Service DTO → UI DTO (for display)
        public static RiskListItemDto ToListItemDto(RiskDto serviceDto)
        {
            return new RiskListItemDto
            {
                Id = serviceDto.Id,
                RiskNumber = GenerateRiskNumber(serviceDto.Id), // Generate from ID
                Title = serviceDto.Name, // Map Name → Title
                Category = serviceDto.Category,
                Status = serviceDto.Status,
                InherentScore = serviceDto.InherentRisk, // Map InherentRisk → InherentScore
                ResidualScore = serviceDto.ResidualRisk, // Map ResidualRisk → ResidualScore
                ResidualRating = CalculateRating(serviceDto.ResidualRisk), // Calculate from score
                ResponsibleParty = serviceDto.Owner, // Map Owner → ResponsibleParty
                IdentifiedDate = serviceDto.CreatedDate, // Use CreatedDate as IdentifiedDate
                TargetClosureDate = serviceDto.DueDate,
                MitigationCount = 0 // TODO: Get from mitigations when available
            };
        }

        // UI DTO → Service DTO (for create)
        public static CreateRiskDto ToCreateDto(RiskCreateDto uiDto, string? status = null)
        {
            return new CreateRiskDto
            {
                Name = uiDto.Title, // Map Title → Name
                Description = uiDto.Description,
                Category = uiDto.Category,
                Probability = ParseLikelihood(uiDto.Likelihood), // Convert string → int
                Impact = ParseImpact(uiDto.Impact), // Convert string → int
                InherentRisk = uiDto.InherentScore, // Map InherentScore → InherentRisk
                ResidualRisk = uiDto.ResidualScore, // Map ResidualScore → ResidualRisk
                Status = status ?? "Active",
                Owner = uiDto.ResponsibleParty ?? uiDto.Owner, // Prefer ResponsibleParty
                DueDate = uiDto.TargetClosureDate
            };
        }

        // Service DTO → UI DTO (for edit)
        public static RiskEditDto ToEditDto(RiskDto serviceDto)
        {
            return new RiskEditDto
            {
                Id = serviceDto.Id,
                RiskNumber = GenerateRiskNumber(serviceDto.Id),
                Title = serviceDto.Name,
                Category = serviceDto.Category,
                Description = serviceDto.Description,
                Status = serviceDto.Status,
                InherentScore = serviceDto.InherentRisk,
                ResidualScore = serviceDto.ResidualRisk,
                ResponsibleParty = serviceDto.Owner,
                Owner = serviceDto.Owner,
                IdentifiedDate = serviceDto.CreatedDate,
                TargetClosureDate = serviceDto.DueDate,
                Impact = ConvertImpactToString(serviceDto.Impact),
                Likelihood = ConvertLikelihoodToString(serviceDto.Probability),
                CreatedDate = serviceDto.CreatedDate,
                LastModifiedDate = serviceDto.ModifiedDate ?? serviceDto.CreatedDate,
                MitigationCount = 0
            };
        }

        // UI DTO → Service DTO (for update)
        public static UpdateRiskDto ToUpdateDto(RiskEditDto uiDto)
        {
            return new UpdateRiskDto
            {
                Id = uiDto.Id,
                Name = uiDto.Title,
                Description = uiDto.Description,
                Category = uiDto.Category,
                Probability = ParseLikelihood(uiDto.Likelihood),
                Impact = ParseImpact(uiDto.Impact),
                InherentRisk = uiDto.InherentScore,
                ResidualRisk = uiDto.ResidualScore,
                Status = uiDto.Status,
                Owner = uiDto.ResponsibleParty ?? uiDto.Owner,
                DueDate = uiDto.TargetClosureDate
            };
        }

        // Helper methods
        private static string GenerateRiskNumber(Guid id)
        {
            // Generate RISK-XXX format from first 8 chars of GUID
            var shortId = id.ToString("N").Substring(0, 8).ToUpper();
            return $"RISK-{shortId}";
        }

        private static string CalculateRating(int score)
        {
            return score switch
            {
                <= 5 => "Low",
                <= 12 => "Medium",
                <= 18 => "High",
                _ => "Critical"
            };
        }

        private static int ParseLikelihood(string likelihood)
        {
            return likelihood?.ToLower() switch
            {
                "low" => 1,
                "medium" => 2,
                "high" => 3,
                _ => 2 // Default to medium
            };
        }

        private static int ParseImpact(string impact)
        {
            return impact?.ToLower() switch
            {
                "low" => 1,
                "medium" => 2,
                "high" => 3,
                "critical" => 4,
                _ => 2 // Default to medium
            };
        }

        private static string ConvertImpactToString(int impact)
        {
            return impact switch
            {
                1 => "Low",
                2 => "Medium",
                3 => "High",
                4 => "Critical",
                _ => "Medium"
            };
        }

        private static string ConvertLikelihoodToString(int probability)
        {
            return probability switch
            {
                1 => "Low",
                2 => "Medium",
                3 => "High",
                _ => "Medium"
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GrcMvc.Models.DTOs
{
    public class RiskDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Probability { get; set; }
        public int Impact { get; set; }
        public int InherentRisk { get; set; }
        public int ResidualRisk { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string MitigationStrategy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string DataClassification { get; set; } = string.Empty;
        
        // Alias properties for controller compatibility (not serialized to avoid duplicates)
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public int RiskScore
        {
            get => ResidualRisk;
            set => ResidualRisk = value;
        }

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string TreatmentPlan
        {
            get => MitigationStrategy;
            set => MitigationStrategy = value ?? string.Empty;
        }
    }

    public class CreateRiskDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Probability { get; set; }
        public int Impact { get; set; }
        public int InherentRisk { get; set; }
        public int ResidualRisk { get; set; }
        public string Status { get; set; } = "Active";
        public string Owner { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string MitigationStrategy { get; set; } = string.Empty;
        public string DataClassification { get; set; } = string.Empty;
        
        // Alias properties for controller compatibility (not serialized to avoid duplicates)
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public int RiskScore
        {
            get => ResidualRisk;
            set => ResidualRisk = value;
        }

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string TreatmentPlan
        {
            get => MitigationStrategy;
            set => MitigationStrategy = value ?? string.Empty;
        }
    }

    public class UpdateRiskDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Probability { get; set; }
        public int Impact { get; set; }
        public int InherentRisk { get; set; }
        public int ResidualRisk { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string MitigationStrategy { get; set; } = string.Empty;
        public string DataClassification { get; set; } = string.Empty;
        
        // Alias properties for controller compatibility (not serialized to avoid duplicates)
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public int RiskScore
        {
            get => ResidualRisk;
            set => ResidualRisk = value;
        }

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string TreatmentPlan
        {
            get => MitigationStrategy;
            set => MitigationStrategy = value ?? string.Empty;
        }
    }

    public class RiskStatisticsDto
    {
        public int TotalRisks { get; set; }
        public int ActiveRisks { get; set; }
        public int CriticalRisks { get; set; }
        public int HighRisks { get; set; }
        public int MediumRisks { get; set; }
        public int LowRisks { get; set; }
        public int MitigatedRisks { get; set; }
        public int AcceptedRisks { get; set; }
        public int ClosedRisks { get; set; }
        public int OpenRisks { get; set; }
        public Dictionary<string, int> RisksByCategory { get; set; } = new();
        public double AverageRiskScore { get; set; }
    }
}
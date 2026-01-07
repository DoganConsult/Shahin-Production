using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GrcMvc.Models.DTOs
{
    public class ControlDto
    {
        public Guid Id { get; set; }
        public string ControlId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int EffectivenessScore { get; set; }
        public int Effectiveness { get; set; }
        public DateTime? ImplementationDate { get; set; }
        public DateTime? LastReviewDate { get; set; }
        public DateTime? NextReviewDate { get; set; }
        public DateTime? LastTestDate { get; set; }
        public DateTime? NextTestDate { get; set; }
        public Guid? RiskId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string DataClassification { get; set; } = string.Empty;
        
        // Alias properties for controller compatibility (not serialized to avoid duplicates)
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string ControlNumber 
        { 
            get => ControlId; 
            set => ControlId = value; 
        }
        
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string ControlType 
        { 
            get => Type; 
            set => Type = value; 
        }
        
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string TestingFrequency 
        { 
            get => Frequency; 
            set => Frequency = value; 
        }
        
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public DateTime? LastTestedDate 
        { 
            get => LastTestDate; 
            set => LastTestDate = value; 
        }
    }

    public class CreateControlDto
    {
        public string ControlId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public int EffectivenessScore { get; set; }
        public DateTime? LastTestDate { get; set; }
        public DateTime? NextTestDate { get; set; }
        public Guid? RiskId { get; set; }
        public string DataClassification { get; set; } = string.Empty;
    }

    public class UpdateControlDto
    {
        public Guid Id { get; set; } // Add missing Id property
        
        public string ControlId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int EffectivenessScore { get; set; }
        public int Effectiveness { get; set; }
        public DateTime? ImplementationDate { get; set; }
        public DateTime? LastReviewDate { get; set; }
        public DateTime? NextReviewDate { get; set; }
        public DateTime? LastTestDate { get; set; }
        public DateTime? NextTestDate { get; set; }
        public Guid? RiskId { get; set; }
        public string DataClassification { get; set; } = string.Empty;
        public string TestingFrequency { get; set; } = string.Empty;
        public DateTime? LastTestedDate { get; set; }
        
        // Alias properties for controller compatibility (not serialized to avoid duplicates)
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string ControlNumber 
        { 
            get => ControlId; 
            set => ControlId = value; 
        }
        
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string ControlType 
        { 
            get => Type; 
            set => Type = value; 
        }
    }
}
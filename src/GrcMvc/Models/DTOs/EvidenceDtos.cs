using System;

namespace GrcMvc.Models.DTOs
{
    // EvidenceListItemDto is defined in CommonDtos.cs

    /// <summary>
    /// Evidence detail DTO
    /// </summary>
    public class EvidenceDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LinkedItemId { get; set; } = string.Empty;
        public string LinkedItemType { get; set; } = string.Empty;
        public DateTime UploadedDate { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string FileSize { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime? ReviewedDate { get; set; }
        public string ReviewedBy { get; set; } = string.Empty;
        public string ReviewStatus { get; set; } = string.Empty; // Pending, Approved, Rejected
    }

    /// <summary>
    /// Evidence create DTO
    /// </summary>
    public class EvidenceCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid LinkedItemId { get; set; }
        public string LinkedItemType { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}

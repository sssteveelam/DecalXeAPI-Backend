// DecalXeAPI/DTOs/UpdateCustomServiceRequestDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class UpdateCustomServiceRequestDto
    {
        [Required]
        public string Description { get; set; } = string.Empty;
        public string? ReferenceImageURL { get; set; }
        public DateTime? DesiredCompletionDate { get; set; }
        [Required]
        public string RequestStatus { get; set; } = string.Empty;
        public decimal? EstimatedCost { get; set; }
        public int? EstimatedWorkUnits { get; set; }
        public string? SalesEmployeeID { get; set; }
    }
}
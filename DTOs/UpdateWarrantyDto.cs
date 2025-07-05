// DecalXeAPI/DTOs/UpdateWarrantyDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class UpdateWarrantyDto
    {
        // Không cho phép đổi VehicleID
        public DateTime WarrantyStartDate { get; set; }
        public DateTime WarrantyEndDate { get; set; }
        [Required]
        public string WarrantyType { get; set; } = string.Empty;
        [Required]
        public string WarrantyStatus { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Notes { get; set; }
    }
}
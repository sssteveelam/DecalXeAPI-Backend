// DecalXeAPI/DTOs/CreateWarrantyDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class CreateWarrantyDto
    {
        [Required]
        public string VehicleID { get; set; } = string.Empty;
        public DateTime? WarrantyStartDate { get; set; }
        public DateTime? WarrantyEndDate { get; set; }
        [Required]
        public string WarrantyType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Notes { get; set; }
    }
}
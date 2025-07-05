// DecalXeAPI/DTOs/CreateVehicleModelDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class CreateVehicleModelDto
    {
        [Required]
        public string ModelName { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public string ChassisNumber { get; set; } = string.Empty;
        [Required]
        public string VehicleType { get; set; } = string.Empty;
        [Required]
        public string BrandID { get; set; } = string.Empty;
    }
}
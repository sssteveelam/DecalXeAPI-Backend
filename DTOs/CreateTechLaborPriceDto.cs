// DecalXeAPI/DTOs/CreateTechLaborPriceDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class CreateTechLaborPriceDto
    {
        [Required]
        public string ServiceID { get; set; } = string.Empty;
        [Required]
        public string VehicleModelID { get; set; } = string.Empty;
        [Required]
        public decimal LaborPrice { get; set; }
    }
}
// DecalXeAPI/DTOs/CreateCustomerVehicleDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class CreateCustomerVehicleDto
    {
        [Required]
        [MaxLength(50)]
        public string ChassisNumber { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? LicensePlate { get; set; } // Biển số xe

        [MaxLength(50)]
        public string? Color { get; set; }

        public int? Year { get; set; }

        public decimal? InitialKM { get; set; }

        [Required]
        public string CustomerID { get; set; } = string.Empty;

        [Required]
        public string ModelID { get; set; } = string.Empty;
    }
}

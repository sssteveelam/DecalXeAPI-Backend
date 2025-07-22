// DecalXeAPI/DTOs/UpdateCustomerVehicleDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class UpdateCustomerVehicleDto
    {
        [MaxLength(50)]
        public string? ChassisNumber { get; set; }

        [MaxLength(20)]
        public string? LicensePlate { get; set; } // Biển số xe

        [MaxLength(50)]
        public string? Color { get; set; }

        public int? Year { get; set; }

        public decimal? InitialKM { get; set; }

        public string? ModelID { get; set; }
    }
}

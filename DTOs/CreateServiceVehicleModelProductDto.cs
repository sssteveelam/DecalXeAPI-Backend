// DecalXeAPI/DTOs/CreateServiceVehicleModelProductDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class CreateServiceVehicleModelProductDto
    {
        [Required]
        public string ServiceID { get; set; } = string.Empty;
        [Required]
        public string VehicleModelID { get; set; } = string.Empty;
        [Required]
        public string ProductID { get; set; } = string.Empty;
        [Required]
        public int Quantity { get; set; }
    }
}
// DecalXeAPI/DTOs/UpdateServiceVehicleModelProductDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class UpdateServiceVehicleModelProductDto
    {
        [Required]
        public int Quantity { get; set; }
    }
}
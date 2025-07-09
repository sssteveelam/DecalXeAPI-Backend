// DecalXeAPI/DTOs/UpdateVehicleDecalTypePriceDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class UpdateVehicleDecalTypePriceDto
    {
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá tiền mới phải là một số không âm.")]
        public decimal NewPrice { get; set; }
    }
}
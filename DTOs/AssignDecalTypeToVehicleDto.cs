// DecalXeAPI/DTOs/AssignDecalTypeToVehicleDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class AssignDecalTypeToVehicleDto
    {
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá tiền phải là một số không âm.")]
        public decimal Price { get; set; }
    }
}
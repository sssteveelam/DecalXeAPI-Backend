// DecalXeAPI/DTOs/UpdateTechLaborPriceDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class UpdateTechLaborPriceDto
    {
        [Required]
        public decimal LaborPrice { get; set; }
    }
}
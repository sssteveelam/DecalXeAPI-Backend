// DecalXeAPI/DTOs/UpdateDecalServiceDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class UpdateDecalServiceDto
    {
        [Required]
        public string ServiceName { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        public int StandardWorkUnits { get; set; }
        [Required]
        public string DecalTypeID { get; set; } = string.Empty;
    }
}
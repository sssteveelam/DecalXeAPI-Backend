// DecalXeAPI/DTOs/CreateDecalTypeDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class CreateDecalTypeDto
    {
        [Required]
        [MaxLength(100)]
        public string DecalTypeName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Material { get; set; }

        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
    }
}
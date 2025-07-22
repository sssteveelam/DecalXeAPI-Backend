// DecalXeAPI/DTOs/CreateDesignTemplateItemDto.cs
using System.ComponentModel.DataAnnotations;
using DecalXeAPI.Models;

namespace DecalXeAPI.DTOs
{
    public class CreateDesignTemplateItemDto
    {
        [Required]
        [MaxLength(100)]
        public string ItemName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public VehiclePart PlacementPosition { get; set; }

        [MaxLength(200)]
        public string? ImageUrl { get; set; }

        public decimal? Width { get; set; }

        public decimal? Height { get; set; }

        public int DisplayOrder { get; set; } = 0;

        [Required]
        public string DesignId { get; set; } = string.Empty;
    }
}

// DecalXeAPI/DTOs/UpdateDesignTemplateItemDto.cs
using System.ComponentModel.DataAnnotations;
using DecalXeAPI.Models;

namespace DecalXeAPI.DTOs
{
    public class UpdateDesignTemplateItemDto
    {
        [MaxLength(100)]
        public string? ItemName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public VehiclePart? PlacementPosition { get; set; }

        [MaxLength(200)]
        public string? ImageUrl { get; set; }

        public decimal? Width { get; set; }

        public decimal? Height { get; set; }

        public int? DisplayOrder { get; set; }
    }
}

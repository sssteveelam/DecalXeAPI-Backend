// DecalXeAPI/DTOs/DesignTemplateItemDto.cs
using DecalXeAPI.Models;

namespace DecalXeAPI.DTOs
{
    public class DesignTemplateItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public VehiclePart PlacementPosition { get; set; }
        public string PlacementPositionName { get; set; } = string.Empty; // Tên tiếng Việt của vị trí
        public string? ImageUrl { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public int DisplayOrder { get; set; }
        public string DesignId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

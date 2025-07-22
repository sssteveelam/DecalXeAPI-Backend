// DecalXeAPI/Models/DesignTemplateItem.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DecalXeAPI.Models
{
    /// <summary>
    /// Đại diện cho một item cụ thể trong một design template
    /// Mỗi item có vị trí đặt riêng biệt trên xe
    /// </summary>
    public class DesignTemplateItem
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string ItemName { get; set; } = string.Empty; // Tên của item decal

        [MaxLength(500)]
        public string? Description { get; set; } // Mô tả chi tiết về item

        /// <summary>
        /// Vị trí đặt decal trên xe (sử dụng enum VehiclePart)
        /// </summary>
        [Required]
        public VehiclePart PlacementPosition { get; set; }

        [MaxLength(200)]
        public string? ImageUrl { get; set; } // URL hình ảnh của item

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Width { get; set; } // Chiều rộng của decal (cm)

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Height { get; set; } // Chiều cao của decal (cm)

        public int DisplayOrder { get; set; } = 0; // Thứ tự hiển thị

        // Foreign Key relationship với Design
        [Required]
        [ForeignKey("Design")]
        public string DesignId { get; set; } = string.Empty;

        [JsonIgnore]
        public virtual Design? Design { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

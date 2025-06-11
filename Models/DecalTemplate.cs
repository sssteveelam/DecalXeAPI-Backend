using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization; // Để dùng [JsonIgnore]

namespace DecalXeAPI.Models
{
    public class DecalTemplate
    {
        [Key]
        public string TemplateID { get; set; } = Guid.NewGuid().ToString(); // PK

        [Required]
        [MaxLength(100)]
        public string TemplateName { get; set; } = string.Empty; // Tên mẫu decal

        [MaxLength(500)]
        public string? ImageURL { get; set; } // URL hình ảnh mẫu decal

        // Khóa ngoại (Foreign Key): Một DecalTemplate thuộc về một DecalType
        [ForeignKey("DecalType")]
        public string DecalTypeID { get; set; } = string.Empty; // FK_DecalTypeID
        public DecalType? DecalType { get; set; }

        // --- NAVIGATION PROPERTIES HIỆN CÓ (Giữ nguyên) ---
        [JsonIgnore] // Để tránh lỗi vòng lặp JSON
        public ICollection<ServiceDecalTemplate>? ServiceDecalTemplates { get; set; }

        // --- NAVIGATION PROPERTY MỚI TỪ YÊU CẦU REVIEW ---
        // Mối quan hệ N-N với CarModel thông qua bảng trung gian CarModelDecalTemplate
        [JsonIgnore] // Để tránh lỗi vòng lặp JSON
        public ICollection<CarModelDecalTemplate>? CarModelDecalTemplates { get; set; }
    }
}
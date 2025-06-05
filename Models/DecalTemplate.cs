using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        // [ForeignKey("DecalType")]
        public string DecalTypeID { get; set; } = string.Empty; // FK_DecalTypeID

        // Navigation Property: Một DecalTemplate có một DecalType
        public DecalType? DecalType { get; set; }

        // Navigation Property: Một DecalTemplate có thể được dùng trong nhiều ServiceDecalTemplate (bảng trung gian)
        public ICollection<ServiceDecalTemplate>? ServiceDecalTemplates { get; set; }
    }
}
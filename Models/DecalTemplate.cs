using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System; // For Guid

namespace DecalXeAPI.Models
{
    public class DecalTemplate
    {
        [Key]
        public string TemplateID { get; set; } = Guid.NewGuid().ToString(); // PK

        [Required]
        [MaxLength(100)]
        public string TemplateName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageURL { get; set; }

        [ForeignKey("DecalType")]
        public string DecalTypeID { get; set; } = string.Empty;
        public DecalType? DecalType { get; set; }

        // --- CÁC NAVIGATION PROPERTIES HIỆN CÓ ---
        [JsonIgnore]
        public ICollection<ServiceDecalTemplate>? ServiceDecalTemplates { get; set; } // Giữ lại

        // --- NAVIGATION PROPERTY ĐƯỢC ĐỔI TÊN THEO REVIEW2 ---
        [JsonIgnore]
        public ICollection<VehicleModelDecalTemplate>? VehicleModelDecalTemplates { get; set; } // <-- ĐỔI TÊN
    }
}

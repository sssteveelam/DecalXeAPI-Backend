// DecalXeAPI/Models/Design.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecalXeAPI.Models
{
    public class Design
    {
        [Key]
        public string DesignID { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(500)]
        public string DesignURL { get; set; } = string.Empty;

        [ForeignKey("Employee")]
        public string? DesignerID { get; set; }
        public Employee? Designer { get; set; }

        [Required]
        [MaxLength(50)]
        public string Version { get; set; } = "1.0";

        [Required]
        [MaxLength(50)]
        public string ApprovalStatus { get; set; } = "Pending";

        public bool IsAIGenerated { get; set; } = false;

        [MaxLength(100)]
        public string? AIModelUsed { get; set; }

        // --- THAY ĐỔI THEO YÊU CẦU ---
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DesignPrice { get; set; } // Thêm giá tiền thiết kế

        /// <summary>
        /// Kích thước của bộ decal (ví dụ: "20cm x 50cm", "Bộ tem trùm cho Exciter 150")
        /// </summary>
        [MaxLength(200)]
        public string? Size { get; set; } // Kích thước decal

        // Navigation properties
        public DesignWorkOrder? DesignWorkOrder { get; set; }

        /// <summary>
        /// Collection of template items that make up this design
        /// Each item represents a specific decal piece with its placement position
        /// </summary>
        public virtual ICollection<DesignTemplateItem>? TemplateItems { get; set; }
    }
}



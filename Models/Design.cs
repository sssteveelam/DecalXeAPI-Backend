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

        // OrderID và AIPrompt đã được xóa
        // Mở file Models/Design.cs và thêm dòng này vào
        public DesignWorkOrder? DesignWorkOrder { get; set; }
    }
}



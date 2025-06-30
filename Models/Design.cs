using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System; // For Guid

namespace DecalXeAPI.Models
{
    public class Design
    {
        [Key]
        public string DesignID { get; set; } = Guid.NewGuid().ToString(); // PK

        // --- CỘT ĐÃ BỊ XÓA THEO REVIEW2 ---
        // [ForeignKey("Order")] // <-- ĐÃ XÓA DÒNG NÀY (FK)
        // public string OrderID { get; set; } = string.Empty; // <-- ĐÃ XÓA DÒNG NÀY
        // [JsonIgnore]
        // public Order? Order { get; set; } // <-- ĐÃ XÓA DÒNG NÀY

        [Required]
        [MaxLength(500)]
        public string DesignURL { get; set; } = string.Empty; // URL file thiết kế

        [ForeignKey("Employee")]
        public string? DesignerID { get; set; }
        public Employee? Designer { get; set; }

        [Required]
        [MaxLength(50)]
        public string Version { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ApprovalStatus { get; set; } = string.Empty;

        public bool IsAIGenerated { get; set; } = false;

        [MaxLength(100)]
        public string? AIModelUsed { get; set; }

        // public string? AIPrompt { get; set; } // <-- ĐÃ XÓA DÒNG NÀY (theo yêu cầu "bỏ allPromt")

        // --- CỘT MỚI TỪ YÊU CẦU REVIEW2 ---
        [Required] // Tiền thiết kế là bắt buộc nếu có design
        [Column(TypeName = "decimal(18,2)")]
        public decimal DesignPrice { get; set; } // <-- MỚI: Tiền thiết kế cho bản thiết kế này


        // Navigation Property: Một Design có nhiều DesignComment
        [JsonIgnore]
        public ICollection<DesignComment>? DesignComments { get; set; }

        // Navigation Property: Một Design có thể có một DesignWorkOrder (Order của Designer)
        // Mối quan hệ 1-0..1 Design -> DesignWorkOrder (DesignID vừa là PK vừa là FK trong DesignWorkOrder)
        [JsonIgnore]
        public DesignWorkOrder? DesignWorkOrder { get; set; }
    }
}

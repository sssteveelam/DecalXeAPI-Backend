using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecalXeAPI.Models
{
    public class Design
    {
        [Key]
        public string DesignID { get; set; } = Guid.NewGuid().ToString(); // PK

        // Khóa ngoại (Foreign Key): Bản thiết kế này thuộc về Order nào
        public string OrderID { get; set; } = string.Empty; // FK_OrderID
        // Navigation Property: Một Design có một Order
        public Order? Order { get; set; }

        [Required]
        [MaxLength(500)]
        public string DesignURL { get; set; } = string.Empty; // URL của file thiết kế

        // Khóa ngoại (Foreign Key): Nhân viên nào đã thiết kế bản này
        public string? DesignerID { get; set; } // FK_DesignerID
        // Navigation Property: Một Design có một Designer (Employee)
        public Employee? Designer { get; set; }

        [Required]
        [MaxLength(50)]
        public string Version { get; set; } = "1.0"; // Phiên bản thiết kế

        [Required]
        [MaxLength(50)]
        public string ApprovalStatus { get; set; } = "Pending"; // Trạng thái phê duyệt (ví dụ: "Pending", "Approved", "Rejected")

        public bool IsAIGenerated { get; set; } = false; // Có phải thiết kế được tạo bởi AI không
        [MaxLength(100)]
        public string? AIModelUsed { get; set; } // Tên model AI nếu có
        [MaxLength(1000)]
        public string? AIPrompt { get; set; } // Prompt đã dùng cho AI nếu có
    }
}
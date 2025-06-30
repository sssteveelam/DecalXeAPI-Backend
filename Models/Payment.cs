using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Text.Json.Serialization; // Để dùng [JsonIgnore]

namespace DecalXeAPI.Models
{
    public class Payment
    {
        [Key]
        public string PaymentID { get; set; } = Guid.NewGuid().ToString(); // PK

        [Required]
        [ForeignKey("Order")]
        public string OrderID { get; set; } = string.Empty;
        [JsonIgnore]
        public Order? Order { get; set; }

        // --- CỘT ĐÃ BỊ XÓA THEO REVIEW2 ---
        // [ForeignKey("Promotion")] // <-- ĐÃ XÓA DÒNG NÀY (FK)
        // public string? PromotionID { get; set; } // <-- ĐÃ XÓA DÒNG NÀY
        // [JsonIgnore]
        // public Promotion? Promotion { get; set; } // <-- ĐÃ XÓA DÒNG NÀY

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaymentAmount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        // MỚI: Lưu mã giao dịch sau khi chuyển khoản thành công (đã có từ trước, giữ nguyên)
        [MaxLength(255)]
        public string? TransactionCode { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentStatus { get; set; } = string.Empty; // Ví dụ: "Success", "Pending", "Failed"

        [MaxLength(100)]
        public string? BankName { get; set; }

        [MaxLength(50)]
        public string? AccountNumber { get; set; }

        [MaxLength(100)]
        public string? PayerName { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}

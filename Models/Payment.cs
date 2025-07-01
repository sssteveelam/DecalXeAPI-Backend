// DecalXeAPI/Models/Payment.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace DecalXeAPI.Models
{
    public class Payment
    {
        [Key]
        public string PaymentID { get; set; } = Guid.NewGuid().ToString();

        [ForeignKey("Order")]
        public string OrderID { get; set; } = string.Empty;
        public Order? Order { get; set; }

        // --- CÁC THUỘC TÍNH ĐÃ BỊ XÓA ---
        // public string? PromotionID { get; set; } 
        // public Promotion? Promotion { get; set; }
        // --------------------------------

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaymentAmount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        // ... (các thuộc tính còn lại giữ nguyên)
        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? TransactionCode { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "Pending";

        [MaxLength(100)]
        public string? BankName { get; set; }

        [MaxLength(100)]
        public string? AccountNumber { get; set; }

        [MaxLength(255)]
        public string? PayerName { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
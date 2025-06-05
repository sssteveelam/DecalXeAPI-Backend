using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace DecalXeAPI.Models
{
    public class Payment
    {
        [Key]
        public string PaymentID { get; set; } = Guid.NewGuid().ToString(); // PK

        // Khóa ngoại (Foreign Key): Thanh toán này cho Order nào
        public string OrderID { get; set; } = string.Empty; // FK_OrderID
        // Navigation Property
        public Order? Order { get; set; }

        // Khóa ngoại (Foreign Key): Thanh toán này có áp dụng Promotion nào không (nullable)
        public string? PromotionID { get; set; } // FK_PromotionID (có thể null)
        // Navigation Property
        public Promotion? Promotion { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Số tiền thanh toán
        public decimal PaymentAmount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow; // Ngày thanh toán

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // Phương thức thanh toán (ví dụ: "Cash", "Card", "Momo", "ZaloPay")

        [MaxLength(100)]
        public string? TransactionCode { get; set; } // Mã giao dịch của bên thứ 3 (ví dụ: mã giao dịch Momo)

        [Required]
        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "Pending"; // Trạng thái thanh toán (ví dụ: "Pending", "Success", "Failed")

        [MaxLength(100)]
        public string? BankName { get; set; } // Tên ngân hàng

        [MaxLength(100)]
        public string? AccountNumber { get; set; } // Số tài khoản/Ví điện tử

        [MaxLength(255)]
        public string? PayerName { get; set; } // Tên người thanh toán

        [MaxLength(500)]
        public string? Notes { get; set; } // Ghi chú thêm về thanh toán
    }
}
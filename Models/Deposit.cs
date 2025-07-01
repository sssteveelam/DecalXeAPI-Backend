// DecalXeAPI/Models/Deposit.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecalXeAPI.Models
{
    public class Deposit
    {
        [Key]
        public string DepositID { get; set; } = Guid.NewGuid().ToString();

        [ForeignKey("Order")]
        public string OrderID { get; set; } = string.Empty; // Cọc này cho đơn hàng nào
        public Order? Order { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } // Số tiền cọc

        [Required]
        public DateTime DepositDate { get; set; } = DateTime.UtcNow; // Ngày cọc

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // Phương thức thanh toán cọc

        [MaxLength(500)]
        public string? Notes { get; set; } // Ghi chú
    }
}
// DecalXeAPI/DTOs/PaymentDto.cs
using System;

namespace DecalXeAPI.DTOs
{
    public class PaymentDto
    {
        public string PaymentID { get; set; } = string.Empty;
        public string OrderID { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;

        // --- CÁC THUỘC TÍNH ĐÃ BỊ XÓA ---
        // public string? PromotionID { get; set; }
        // public string? PromotionName { get; set; }
        // --------------------------------

        public decimal PaymentAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? TransactionCode { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? PayerName { get; set; }
        public string? Notes { get; set; }
    }
}
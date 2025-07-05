// DecalXeAPI/DTOs/CreatePaymentDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class CreatePaymentDto
    {
        [Required]
        public string OrderID { get; set; } = string.Empty;
        [Required]
        public decimal PaymentAmount { get; set; }
        [Required]
        public string PaymentMethod { get; set; } = string.Empty;
        public string? TransactionCode { get; set; }
        [Required]
        public string PaymentStatus { get; set; } = string.Empty;
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? PayerName { get; set; }
        public string? Notes { get; set; }
    }
}
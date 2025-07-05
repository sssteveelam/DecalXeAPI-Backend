// DecalXeAPI/DTOs/CreateDepositDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class CreateDepositDto
    {
        [Required]
        public string OrderID { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}
// DecalXeAPI/DTOs/UpdatePaymentDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class UpdatePaymentDto
    {
        [Required]
        public string PaymentStatus { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
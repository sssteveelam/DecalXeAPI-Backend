// DecalXeAPI/DTOs/DepositDto.cs
using System;

namespace DecalXeAPI.DTOs
{
    public class DepositDto
    {
        public string DepositID { get; set; } = string.Empty;
        public string OrderID { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DepositDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
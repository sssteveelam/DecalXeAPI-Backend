// DecalXeAPI/DTOs/CreateOrderDetailDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class CreateOrderDetailDto
    {
        [Required]
        public string OrderID { get; set; } = string.Empty;
        [Required]
        public string ServiceID { get; set; } = string.Empty;
        [Required]
        public int Quantity { get; set; }
        public decimal? ActualAreaUsed { get; set; }
        public decimal? ActualLengthUsed { get; set; }
        public decimal? ActualWidthUsed { get; set; }
    }
}
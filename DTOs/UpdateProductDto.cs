// DecalXeAPI/DTOs/UpdateProductDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class UpdateProductDto
    {
        [Required]
        public string ProductName { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public string Unit { get; set; } = string.Empty;
        [Required]
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }
        [Required]
        public string CategoryID { get; set; } = string.Empty;
    }
}
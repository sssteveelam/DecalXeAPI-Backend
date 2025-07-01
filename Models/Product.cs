using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Để dùng [Column]
using System.Text.Json.Serialization;

namespace DecalXeAPI.Models
{
    public class Product
    {
        [Key]
        public string ProductID { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string ProductName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; } // Mô tả sản phẩm

        [Required]
        [MaxLength(50)]
        public string Unit { get; set; } = string.Empty; // Đơn vị tính (ví dụ: cuộn, chai, mét)

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Định rõ kiểu số thập phân cho DB (18 chữ số tổng cộng, 2 chữ số sau dấu thập phân)
        public decimal UnitPrice { get; set; }

        public int StockQuantity { get; set; } // Số lượng tồn kho

        // Navigation Property: Một Product có thể được dùng trong nhiều ServiceProduct.
        // Khóa ngoại trỏ đến Category
        [ForeignKey("Category")]
        public string CategoryID { get; set; } = string.Empty;
        public Category? Category { get; set; }
    }
}
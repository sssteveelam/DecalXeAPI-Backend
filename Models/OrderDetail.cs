using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecalXeAPI.Models
{
    public class OrderDetail
    {
        [Key]
        public string OrderDetailID { get; set; } = Guid.NewGuid().ToString(); // PK

        // Khóa ngoại (Foreign Key): Chi tiết này thuộc về Order nào
        public string OrderID { get; set; } = string.Empty; // FK_OrderID
        // Navigation Property: Một OrderDetail có một Order
        public Order? Order { get; set; }

        // Khóa ngoại (Foreign Key): Chi tiết này là dịch vụ nào
        public string ServiceID { get; set; } = string.Empty; // FK_ServiceID
        // Navigation Property: Một OrderDetail có một DecalService
        public DecalService? DecalService { get; set; }

        [Required]
        public int Quantity { get; set; } // Số lượng dịch vụ/sản phẩm trong chi tiết này

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Giá của dịch vụ/sản phẩm trong chi tiết này
        public decimal Price { get; set; }
    }
}
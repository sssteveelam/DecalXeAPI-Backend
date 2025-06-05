using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecalXeAPI.Models
{
    public class ServiceProduct
    {
        [Key]
        public string ServiceProductID { get; set; } = Guid.NewGuid().ToString(); // PK

        // Khóa ngoại (Foreign Key): Một ServiceProduct liên kết với một DecalService
        public string ServiceID { get; set; } = string.Empty; // FK_ServiceID

        // Navigation Property
        public DecalService? DecalService { get; set; }

        // Khóa ngoại (Foreign Key): Một ServiceProduct liên kết với một Product
        public string ProductID { get; set; } = string.Empty; // FK_ProductID

        // Navigation Property
        public Product? Product { get; set; }

        [Required]
        public decimal QuantityUsed { get; set; } // Số lượng sản phẩm/vật tư cần dùng cho dịch vụ
    }
}
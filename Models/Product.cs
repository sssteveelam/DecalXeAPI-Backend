using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System; // For Guid

namespace DecalXeAPI.Models
{
    public class Product
    {
        [Key]
        public string ProductID { get; set; } = Guid.NewGuid().ToString(); // PK

        [Required]
        [MaxLength(100)]
        public string ProductName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string Unit { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        // --- NAVIGATION PROPERTIES ĐƯỢC ĐIỀU CHỈNH/THÊM THEO REVIEW2 ---
        // public ICollection<ServiceProduct>? ServiceProducts { get; set; } // <-- ĐÃ XÓA (vì bảng ServiceProduct đã xóa)

        [JsonIgnore]
        public ICollection<ServiceVehicleModelProduct>? ServiceVehicleModelProducts { get; set; } // <-- MỚI: Bảng liên kết 3 chiều mới
    }
}

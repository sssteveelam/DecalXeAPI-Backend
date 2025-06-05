using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace DecalXeAPI.Models
{
    public class Warranty
    {
        [Key]
        public string WarrantyID { get; set; } = Guid.NewGuid().ToString(); // PK

        // Khóa ngoại (Foreign Key): Bảo hành này cho Order nào
        public string OrderID { get; set; } = string.Empty; // FK_OrderID
        // Navigation Property
        public Order? Order { get; set; }

        [Required]
        public DateTime WarrantyStartDate { get; set; } = DateTime.UtcNow; // Ngày bắt đầu bảo hành

        [Required]
        public DateTime WarrantyEndDate { get; set; } = DateTime.UtcNow.AddYears(1); // Ngày kết thúc bảo hành (mặc định 1 năm)

        [Required]
        [MaxLength(100)]
        public string WarrantyType { get; set; } = string.Empty; // Loại bảo hành (ví dụ: "Bảo hành bong tróc", "Bảo hành màu sắc")

        [Required]
        [MaxLength(50)]
        public string WarrantyStatus { get; set; } = "Active"; // Trạng thái bảo hành (Active, Expired, Void)

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; } // Ghi chú thêm
    }
}
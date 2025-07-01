// DecalXeAPI/Models/Warranty.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace DecalXeAPI.Models
{
    public class Warranty
    {
        [Key]
        public string WarrantyID { get; set; } = Guid.NewGuid().ToString();

        // --- THAY ĐỔI LỚN Ở ĐÂY ---
        // Khóa ngoại mới: Bảo hành này cho xe nào
        [ForeignKey("CustomerVehicle")]
        public string VehicleID { get; set; } = string.Empty; // Thêm mới
        public CustomerVehicle? CustomerVehicle { get; set; } // Thêm mới

        // Khóa ngoại cũ đã bị xóa
        // public string OrderID { get; set; } = string.Empty; 
        // public Order? Order { get; set; }
        // ---------------------------

        [Required]
        public DateTime WarrantyStartDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime WarrantyEndDate { get; set; } = DateTime.UtcNow.AddYears(1);

        [Required]
        [MaxLength(100)]
        public string WarrantyType { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string WarrantyStatus { get; set; } = "Active";

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
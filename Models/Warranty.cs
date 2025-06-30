using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Text.Json.Serialization;

namespace DecalXeAPI.Models
{
    public class Warranty
    {
        [Key]
        public string WarrantyID { get; set; } = Guid.NewGuid().ToString(); // PK

        // --- CỘT ĐÃ BỊ XÓA THEO REVIEW2 ---
        // [ForeignKey("Order")] // <-- ĐÃ XÓA DÒNG NÀY (FK)
        // public string OrderID { get; set; } = string.Empty; // <-- ĐÃ XÓA DÒNG NÀY
        // [JsonIgnore]
        // public Order? Order { get; set; } // <-- ĐÃ XÓA DÒNG NÀY

        // --- CỘT MỚI TỪ YÊU CẦU REVIEW2 ---
        [Required] // Bảo hành phải liên kết với một xe cụ thể
        [ForeignKey("CustomerVehicle")]
        public string VehicleID { get; set; } = string.Empty; // <-- MỚI: FK tới xe của khách hàng (theo số khung)
        [JsonIgnore]
        public CustomerVehicle? CustomerVehicle { get; set; } // Navigation Property


        [Required]
        public DateTime WarrantyStartDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime WarrantyEndDate { get; set; }

        [Required]
        [MaxLength(100)]
        public string WarrantyType { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string WarrantyStatus { get; set; } = string.Empty; // Ví dụ: "Active", "Expired", "Voided"

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}

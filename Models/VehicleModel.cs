using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System;
using System.Text.Json.Serialization;

namespace DecalXeAPI.Models
{
    public class VehicleModel // Đổi từ CarModel
    {
        [Key]
        public string ModelID { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string ModelName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        // --- THÊM MỚI THEO YÊU CẦU ---
        [Required]
        [MaxLength(50)]
        public string ChassisNumber { get; set; } = string.Empty; // Số khung xe

        [Required]
        [MaxLength(50)]
        public string VehicleType { get; set; } = string.Empty; // Loại xe (vd: "Xe máy", "Ô tô")
        // -----------------------------

        [ForeignKey("VehicleBrand")] // Đổi từ CarBrand
        public string BrandID { get; set; } = string.Empty;
        public VehicleBrand? VehicleBrand { get; set; } // Đổi từ CarBrand

        [JsonIgnore]
        public ICollection<CustomerVehicle>? CustomerVehicles { get; set; }

        [JsonIgnore]
        public ICollection<VehicleModelDecalTemplate>? VehicleModelDecalTemplates { get; set; } // Đổi từ CarModelDecalTemplate
        [JsonIgnore]
        public ICollection<VehicleModelDecalType>? VehicleModelDecalTypes { get; set; }
    }
}
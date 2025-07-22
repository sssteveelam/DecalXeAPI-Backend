// DecalXeAPI/Models/CustomerVehicle.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DecalXeAPI.Models
{
    public class CustomerVehicle
    {
        [Key]
        public string VehicleID { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(50)] // Tăng độ dài cho số khung
        public string ChassisNumber { get; set; } = string.Empty; // Thay thế LicensePlate

        [MaxLength(20)]
        public string? LicensePlate { get; set; } // Biển số xe

        [MaxLength(50)]
        public string? Color { get; set; }

        public int? Year { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? InitialKM { get; set; }

        [ForeignKey("Customer")]
        public string CustomerID { get; set; } = string.Empty;
        public Customer? Customer { get; set; }

        [ForeignKey("VehicleModel")] // Đổi từ CarModel
        public string ModelID { get; set; } = string.Empty;
        public VehicleModel? VehicleModel { get; set; } // Đổi từ CarModel

        [JsonIgnore]
        public ICollection<Order>? Orders { get; set; }
    }
}
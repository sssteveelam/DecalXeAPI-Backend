using System; // For Guid
using System.Collections.Generic; // For ICollection
using System.ComponentModel.DataAnnotations; // For [Key], [Required], [MaxLength]
using System.ComponentModel.DataAnnotations.Schema; // For [Column]
using System.Text.Json.Serialization; // For [JsonIgnore]

namespace DecalXeAPI.Models
{
    public class Vehicle
    {
        [Key]
        public string VehicleID { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string BrandName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ModelName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? VehicleType { get; set; }

        [MaxLength(50)]
        public string? ChassisNumber { get; set; }


        [MaxLength(500)]
        public string? Description { get; set; }

        [JsonIgnore]
        public ICollection<CustomerVehicle>? CustomerVehicles { get; set; }

        [JsonIgnore]
        public ICollection<VehicleModelDecalTemplate>? VehicleModelDecalTemplates { get; set; }

        [JsonIgnore]
        public ICollection<TechLaborPrice>? TechLaborPrices { get; set; }

        [JsonIgnore]
        public ICollection<ServiceVehicleModelProduct>? ServiceVehicleModelProducts { get; set; } // <-- Đảm bảo tên này đúng (số nhiều)
    }
}
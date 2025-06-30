using System; // For Guid
using System.Collections.Generic; // For ICollection
using System.ComponentModel.DataAnnotations; // For [Key], [Required], [MaxLength]
using System.ComponentModel.DataAnnotations.Schema; // For [ForeignKey]
using System.Text.Json.Serialization; // For [JsonIgnore]

namespace DecalXeAPI.Models
{
    public class VehicleModel
    {
        [Key]
        public string ModelID { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string ModelName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [ForeignKey("VehicleBrand")]
        public string BrandID { get; set; } = string.Empty;
        public VehicleBrand? VehicleBrand { get; set; }

        [JsonIgnore]
        public ICollection<CustomerVehicle>? CustomerVehicles { get; set; }

        [JsonIgnore]
        public ICollection<VehicleModelDecalTemplate>? VehicleModelDecalTemplates { get; set; }

        [JsonIgnore]
        public ICollection<TechLaborPrice>? TechLaborPrices { get; set; }

        [JsonIgnore]
        public ICollection<ServiceVehicleModelProduct>? ServiceCarModelProducts { get; set; } // <-- Đảm bảo tên này đúng (số nhiều)
    }
}
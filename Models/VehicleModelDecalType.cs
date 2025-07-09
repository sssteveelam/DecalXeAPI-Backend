// DecalXeAPI/Models/VehicleModelDecalType.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DecalXeAPI.Models
{
    public class VehicleModelDecalType
    {
        [Key]
        public string VehicleModelDecalTypeID { get; set; } = Guid.NewGuid().ToString();



        // Khóa ngoại trỏ đến hòn đảo "VehicleModel"
        [ForeignKey("VehicleModel")]
        public string ModelID { get; set; } = string.Empty;
        [JsonIgnore]
        public VehicleModel? VehicleModel { get; set; }

        // Khóa ngoại trỏ đến hòn đảo "DecalType"
        [ForeignKey("DecalType")]
        public string DecalTypeID { get; set; } = string.Empty;
        [JsonIgnore]
        public DecalType? DecalType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Giá của loại decal này cho mẫu xe này
    }
}
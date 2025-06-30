using System; // For Guid
using System.Collections.Generic; // For ICollection
using System.ComponentModel.DataAnnotations; // For [Key], [Required], [MaxLength]
using System.Text.Json.Serialization; // For [JsonIgnore]

namespace DecalXeAPI.Models
{
    // ĐỔI TÊN TỪ CarBrand
    public class VehicleBrand // <-- ĐẢM BẢO TÊN LỚP ĐÚNG
    {
        [Key]
        public string BrandID { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string BrandName { get; set; } = string.Empty;

        // Navigation Property: Một hãng xe (VehicleBrand) có thể có nhiều mẫu xe (VehicleModel).
        [JsonIgnore]
        public ICollection<VehicleModel>? VehicleModels { get; set; } // <-- ĐỔI TÊN VÀ TRỎ TỚI VEHICLEMODEL
    }
}

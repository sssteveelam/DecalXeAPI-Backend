using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System; // For Guid
using System.Text.Json.Serialization; // For [JsonIgnore]

namespace DecalXeAPI.Models
{
    // Bảng liên kết nhiều-nhiều giữa VehicleModel và DecalTemplate
    // ĐỔI TÊN TỪ CarModelDecalTemplate SANG VehicleModelDecalTemplate
    public class VehicleModelDecalTemplate // <-- ĐẢM BẢO TÊN LỚP ĐÚNG
    {
        [Key]
        public string VehicleModelDecalTemplateID { get; set; } = Guid.NewGuid().ToString(); // PK riêng cho liên kết

        // Khóa ngoại 1: đến VehicleModel (đã đổi tên từ CarModel)
        [ForeignKey("VehicleModel")] // <-- ĐỔI TÊN FK
        public string ModelID { get; set; } = string.Empty;
        [JsonIgnore]
        public VehicleModel? VehicleModel { get; set; } // <-- ĐỔI TÊN NAVIGATION PROPERTY

        // Khóa ngoại 2: đến DecalTemplate
        [ForeignKey("DecalTemplate")]
        public string TemplateID { get; set; } = string.Empty;
        [JsonIgnore]
        public DecalTemplate? DecalTemplate { get; set; }
    }
}

using System; // For Guid
using System.ComponentModel.DataAnnotations; // For [Key], [Required]
using System.ComponentModel.DataAnnotations.Schema; // For [ForeignKey], [Column]
using System.Text.Json.Serialization; // For [JsonIgnore]

namespace DecalXeAPI.Models
{
    // Bảng liên kết ba chiều: DecalService <-> VehicleModel <-> Product
    public class ServiceVehicleModelProduct
    {
        [Key]
        public string ServiceVehicleModelProductID { get; set; } = Guid.NewGuid().ToString(); // PK riêng cho liên kết

        [ForeignKey("DecalService")]
        public string ServiceID { get; set; } = string.Empty;
        [JsonIgnore] // Tránh lỗi vòng lặp JSON
        public DecalService? DecalService { get; set; }

        [ForeignKey("VehicleModel")] // Khóa ngoại trỏ tới bảng VehicleModel (mẫu xe)
        public string VehicleModelID { get; set; } = string.Empty; // FK trỏ tới VehicleModel
        [JsonIgnore] // Tránh lỗi vòng lặp JSON
        public VehicleModel? VehicleModel { get; set; } // Navigation Property

        [ForeignKey("Product")]
        public string ProductID { get; set; } = string.Empty;
        [JsonIgnore] // Tránh lỗi vòng lặp JSON
        public Product? Product { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal QuantityUsed { get; set; } // Số lượng sản phẩm/vật tư cần dùng cho dịch vụ đó trên mẫu xe đó
    }
}
using System; // For Guid
using System.ComponentModel.DataAnnotations; // For [Key], [Required], [MaxLength]
using System.ComponentModel.DataAnnotations.Schema; // For [ForeignKey], [Column]
using System.Text.Json.Serialization; // For [JsonIgnore]

namespace DecalXeAPI.Models
{
    public class TechLaborPrice
    {
        [Key]
        public string TechLaborPriceID { get; set; } = Guid.NewGuid().ToString(); // PK

        [ForeignKey("DecalService")] // Khóa ngoại trỏ tới bảng DecalService
        public string ServiceID { get; set; } = string.Empty;
        [JsonIgnore]
        public DecalService? DecalService { get; set; } // Navigation Property

        [ForeignKey("VehicleModel")] // Khóa ngoại trỏ tới bảng VehicleModel (mẫu xe, đã đổi tên từ CarModel)
        public string VehicleModelID { get; set; } = string.Empty; // Đổi tên từ ModelID sang VehicleModelID để rõ ràng
        [JsonIgnore]
        public VehicleModel? VehicleModel { get; set; } // Navigation Property (đổi tên từ CarModel sang VehicleModel)

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Kiểu dữ liệu decimal cho giá tiền
        public decimal PricePerUnit { get; set; } // Giá công trên mỗi đơn vị (ví dụ: mỗi giờ, mỗi mét vuông)

        [MaxLength(100)]
        public string? Unit { get; set; } // Đơn vị tính (ví dụ: "giờ", "m2", "chiếc")
    }
}
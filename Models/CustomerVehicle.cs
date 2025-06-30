using System; // For Guid
using System.Collections.Generic; // For ICollection
using System.ComponentModel.DataAnnotations; // For [Key], [Required], [MaxLength]
using System.ComponentModel.DataAnnotations.Schema; // For [ForeignKey], [Column]
using System.Text.Json.Serialization; // For [JsonIgnore]

namespace DecalXeAPI.Models
{
    public class CustomerVehicle
    {
        [Key]
        public string VehicleID { get; set; } = Guid.NewGuid().ToString(); // PK duy nhất cho chiếc xe cụ thể của khách hàng

        [Required]
        [MaxLength(50)] // Số khung xe là định danh duy nhất cho chiếc xe của khách hàng
        public string ChassisNumber { get; set; } = string.Empty; // <-- THAY THẾ LicensePlate bằng ChassisNumber

        [MaxLength(50)]
        public string? LicensePlate { get; set; } // <-- MỚI: Thêm lại LicensePlate nếu vẫn cần lưu biển số xe riêng

        [MaxLength(50)]
        public string? Color { get; set; }

        public int? Year { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? InitialKM { get; set; }

        // Mối quan hệ: Xe của khách hàng thuộc MẪU XE (VehicleModel) nào
        [ForeignKey("VehicleModel")] // <-- ĐỔI TÊN FK
        public string VehicleModelID { get; set; } = string.Empty; // <-- ĐỔI TÊN TỪ ModelID SANG VehicleModelID
        public VehicleModel? VehicleModel { get; set; } // <-- ĐỔI TÊN NAVIGATION PROPERTY TỪ CarModel SANG VehicleModel


        // Khóa ngoại: Xe này thuộc về Customer nào
        [ForeignKey("Customer")]
        public string CustomerID { get; set; } = string.Empty;
        public Customer? Customer { get; set; }


        // Navigation Property: Một chiếc xe cụ thể có thể có nhiều Order
        [JsonIgnore]
        public ICollection<Order>? Orders { get; set; }

        // Navigation Property: Một chiếc xe cụ thể có thể có nhiều Warranty
        [JsonIgnore]
        public ICollection<Warranty>? Warranties { get; set; }
    }
}

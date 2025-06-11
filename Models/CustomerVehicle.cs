using System; // Để sử dụng Guid.NewGuid()
using System.Collections.Generic; // Để sử dụng ICollection cho Navigation Property
using System.ComponentModel.DataAnnotations; // Để sử dụng các thuộc tính như [Key], [Required], [MaxLength]
using System.ComponentModel.DataAnnotations.Schema; // Để sử dụng thuộc tính [ForeignKey]
using System.Text.Json.Serialization; // Để sử dụng [JsonIgnore]

namespace DecalXeAPI.Models
{
    public class CustomerVehicle
    {
        [Key] // Đánh dấu VehicleID là Khóa Chính (Primary Key) của bảng. Mỗi chiếc xe có một ID duy nhất.
        public string VehicleID { get; set; } = Guid.NewGuid().ToString(); // PK, tự động sinh GUID làm ID mặc định.

        [Required] // Đánh dấu LicensePlate là bắt buộc (NOT NULL).
        [MaxLength(20)] // Giới hạn độ dài tối đa của biển số xe.
        public string LicensePlate { get; set; } = string.Empty; // Biển số xe của khách hàng.

        [MaxLength(50)] // Giới hạn độ dài tối đa của chuỗi Color.
        public string? Color { get; set; } // Màu sắc của xe (có thể là NULL).

        public int? Year { get; set; } // Năm sản xuất của xe (có thể là NULL).

        [Column(TypeName = "decimal(18,2)")] // Định rõ kiểu dữ liệu số thập phân cho cột trong DB.
        public decimal? InitialKM { get; set; } // Số km ban đầu của xe khi thực hiện dịch vụ dán (có thể là NULL).

        // Khóa ngoại (Foreign Key): Xe này thuộc về Khách hàng (Customer) nào.
        [ForeignKey("Customer")] // Chỉ rõ rằng CustomerID là khóa ngoại đến bảng "Customer".
        public string CustomerID { get; set; } = string.Empty; // FK, bắt buộc (NOT NULL).
        public Customer? Customer { get; set; } // Navigation Property: Trỏ về đối tượng Customer liên quan.

        // Khóa ngoại (Foreign Key): Xe này thuộc Mẫu xe (CarModel) nào.
        [ForeignKey("CarModel")] // Chỉ rõ rằng ModelID là khóa ngoại đến bảng "CarModel".
        public string ModelID { get; set; } = string.Empty; // FK, bắt buộc (NOT NULL).
        public CarModel? CarModel { get; set; } // Navigation Property: Trỏ về đối tượng CarModel liên quan.

        // Navigation Property (Quan hệ 1-N): Một chiếc xe (CustomerVehicle) có thể được gán cho nhiều Đơn hàng (Order).
        [JsonIgnore] // Tránh lỗi vòng lặp khi serialize JSON.
        public ICollection<Order>? Orders { get; set; }
    }
}
using System; // Để sử dụng Guid.NewGuid()
using System.Collections.Generic; // Để sử dụng ICollection cho Navigation Property
using System.ComponentModel.DataAnnotations; // Để sử dụng các thuộc tính như [Key], [Required], [MaxLength]
using System.Text.Json.Serialization; // Để sử dụng [JsonIgnore]

namespace DecalXeAPI.Models
{
    public class CarBrand
    {
        [Key] // Đánh dấu BrandID là Khóa Chính (Primary Key) của bảng. Mỗi hãng xe có một ID duy nhất.
        public string BrandID { get; set; } = Guid.NewGuid().ToString(); // PK, tự động sinh GUID làm ID mặc định.

        [Required] // Đánh dấu BrandName là thuộc tính bắt buộc (NOT NULL) trong cơ sở dữ liệu.
        [MaxLength(100)] // Giới hạn độ dài tối đa của chuỗi BrandName là 100 ký tự.
        public string BrandName { get; set; } = string.Empty; // Tên của hãng xe (ví dụ: "Honda", "Mercedes-Benz").

        // Navigation Property (Quan hệ 1-N): Một hãng xe (CarBrand) có thể có nhiều mẫu xe (CarModel).
        // [JsonIgnore] được sử dụng để tránh lỗi vòng lặp khi serialize JSON (nếu CarModel có Navigation Property ngược lại về CarBrand,
        // và bạn Include cả hai trong cùng một truy vấn API).
        [JsonIgnore]
        public ICollection<CarModel>? CarModels { get; set; }
    }
}
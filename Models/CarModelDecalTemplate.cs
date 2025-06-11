using System; // Để sử dụng Guid.NewGuid()
using System.ComponentModel.DataAnnotations; // Để sử dụng thuộc tính [Key]
using System.ComponentModel.DataAnnotations.Schema; // Để sử dụng thuộc tính [ForeignKey]
using System.Text.Json.Serialization; // Để sử dụng [JsonIgnore]

namespace DecalXeAPI.Models
{
    // Bảng liên kết nhiều-nhiều giữa CarModel và DecalTemplate.
    // Mặc dù có thể dùng khóa chính composite (ModelID, TemplateID),
    // việc dùng PK riêng (CarModelDecalTemplateID) thường dễ quản lý hơn trong EF Core
    // và cho phép thêm các thuộc tính khác vào mối quan hệ này trong tương lai.
    public class CarModelDecalTemplate
    {
        [Key] // Đánh dấu CarModelDecalTemplateID là Khóa Chính của bảng. Mỗi liên kết có một ID duy nhất.
        public string CarModelDecalTemplateID { get; set; } = Guid.NewGuid().ToString(); // PK, tự động sinh GUID làm ID mặc định.

        // Khóa ngoại 1: Trỏ đến Mẫu xe (CarModel).
        [ForeignKey("CarModel")] // Chỉ rõ rằng ModelID là khóa ngoại đến bảng "CarModel".
        public string ModelID { get; set; } = string.Empty; // FK, bắt buộc (NOT NULL).
        [JsonIgnore] // Tránh lỗi vòng lặp khi serialize JSON (khi CarModel truy vấn CarModelDecalTemplate và ngược lại).
        public CarModel? CarModel { get; set; } // Navigation Property: Trỏ về đối tượng CarModel liên quan.

        // Khóa ngoại 2: Trỏ đến Mẫu decal (DecalTemplate).
        [ForeignKey("DecalTemplate")] // Chỉ rõ rằng TemplateID là khóa ngoại đến bảng "DecalTemplate".
        public string TemplateID { get; set; } = string.Empty; // FK, bắt buộc (NOT NULL).
        [JsonIgnore] // Tránh lỗi vòng lặp khi serialize JSON.
        public DecalTemplate? DecalTemplate { get; set; } // Navigation Property: Trỏ về đối tượng DecalTemplate liên quan.

        // Có thể thêm các thuộc tính khác vào bảng liên kết này nếu cần,
        // ví dụ: IsRecommended (kiểu bool, để đánh dấu mẫu decal này có được đề xuất cho mẫu xe đó không),
        // hoặc DisplayOrder (kiểu int, để sắp xếp thứ tự hiển thị các mẫu decal cho một mẫu xe cụ thể).
    }
}
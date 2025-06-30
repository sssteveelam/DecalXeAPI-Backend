using System; // For Guid
using System.ComponentModel.DataAnnotations; // For [Key], [Required], [MaxLength]
using System.ComponentModel.DataAnnotations.Schema; // For [ForeignKey], [Column]
using System.Text.Json.Serialization; // For [JsonIgnore]

namespace DecalXeAPI.Models
{
    public class AdminDetail
    {
        // EmployeeID là Khóa Chính (PK) và Khóa Ngoại (FK) đồng thời.
        // Điều này thiết lập mối quan hệ 1-1 với Employee:
        // Mỗi AdminDetail PHẢI thuộc về một và chỉ một Employee.
        // Ngược lại, một Employee có thể có 0 hoặc 1 AdminDetail.
        [Key] // Đánh dấu EmployeeID là Khóa Chính của bảng này.
        [ForeignKey("Employee")] // Chỉ rõ rằng EmployeeID là khóa ngoại đến bảng "Employee".
        public string EmployeeID { get; set; } = string.Empty; // PK & FK.

        // Navigation Property: Trỏ ngược về đối tượng Employee liên quan.
        // [JsonIgnore] được thêm vào để tránh lỗi vòng lặp JSON nếu Employee có Navigation Property ngược lại.
        [JsonIgnore]
        public Employee? Employee { get; set; }

        [Required] // Lương cơ bản là bắt buộc.
        [Column(TypeName = "decimal(18,2)")] // Định rõ kiểu dữ liệu số thập phân cho cột trong DB.
        public decimal BaseSalary { get; set; } // Mức lương cơ bản do Admin thiết lập.

        [MaxLength(100)] // Giới hạn độ dài tối đa của chuỗi.
        public string? ManagementLevel { get; set; } // Cấp độ quản lý (ví dụ: "Senior Admin", "Junior Admin").
    }
}
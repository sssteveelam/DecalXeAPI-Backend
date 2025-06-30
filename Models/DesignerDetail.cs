using System; // For Guid
using System.ComponentModel.DataAnnotations; // For [Key], [Required], [MaxLength]
using System.ComponentModel.DataAnnotations.Schema; // For [ForeignKey], [Column]
using System.Text.Json.Serialization; // For [JsonIgnore]

namespace DecalXeAPI.Models
{
    public class DesignerDetail
    {
        // EmployeeID là Khóa Chính (PK) và Khóa Ngoại (FK) đồng thời.
        [Key]
        [ForeignKey("Employee")]
        public string EmployeeID { get; set; } = string.Empty; // PK & FK.

        [JsonIgnore]
        public Employee? Employee { get; set; }

        [MaxLength(100)]
        public string? DesignSpecialty { get; set; } // Chuyên môn thiết kế (ví dụ: "Xe máy", "Ô tô").

        [MaxLength(500)]
        public string? DesignDifficultyDescription { get; set; } // Mô tả độ khó của thiết kế (nếu có).

        public int? DesignHoursWorked { get; set; } // Tổng số giờ làm việc thiết kế (có thể NULL).

        [Required] // Trạng thái làm việc là bắt buộc.
        [MaxLength(50)]
        public string Status { get; set; } = "Available"; // Trạng thái làm việc (ví dụ: "Available", "Busy").
    }
}
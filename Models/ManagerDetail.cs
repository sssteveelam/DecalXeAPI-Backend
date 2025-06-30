using System; // For Guid
using System.ComponentModel.DataAnnotations; // For [Key], [Required], [MaxLength]
using System.ComponentModel.DataAnnotations.Schema; // For [ForeignKey], [Column]
using System.Text.Json.Serialization; // For [JsonIgnore]

namespace DecalXeAPI.Models
{
    public class ManagerDetail
    {
        // EmployeeID là Khóa Chính (PK) và Khóa Ngoại (FK) đồng thời.
        [Key]
        [ForeignKey("Employee")]
        public string EmployeeID { get; set; } = string.Empty; // PK & FK.

        [JsonIgnore]
        public Employee? Employee { get; set; }

        [Required]
        [MaxLength(100)] // Tên phòng ban không quá 100 ký tự.
        public string DepartmentManaged { get; set; } = string.Empty; // Phòng ban mà Manager này quản lý.

        [Required]
        [MaxLength(50)] // Trạng thái không quá 50 ký tự.
        public string Status { get; set; } = "Active"; // Trạng thái làm việc (ví dụ: "Active", "On Leave").
    }
}
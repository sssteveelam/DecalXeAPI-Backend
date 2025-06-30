using System; // For Guid
using System.ComponentModel.DataAnnotations; // For [Key], [Required], [MaxLength]
using System.ComponentModel.DataAnnotations.Schema; // For [ForeignKey], [Column]
using System.Text.Json.Serialization; // For [JsonIgnore]

namespace DecalXeAPI.Models
{
    public class SalesPersonDetail
    {
        // EmployeeID là Khóa Chính (PK) và Khóa Ngoại (FK) đồng thời.
        [Key]
        [ForeignKey("Employee")]
        public string EmployeeID { get; set; } = string.Empty; // PK & FK.

        [JsonIgnore]
        public Employee? Employee { get; set; }

        [Column(TypeName = "decimal(18,2)")] // Kiểu dữ liệu decimal cho chỉ tiêu doanh số.
        public decimal? SalesTarget { get; set; } // Chỉ tiêu doanh số (có thể NULL).

        [Required] // Trạng thái làm việc là bắt buộc.
        [MaxLength(50)]
        public string Status { get; set; } = "Active"; // Trạng thái làm việc (ví dụ: "Active", "On Leave").
    }
}
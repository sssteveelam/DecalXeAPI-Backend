// DecalXeAPI/Models/AdminDetail.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecalXeAPI.Models
{
    public class AdminDetail
    {
        [Key, ForeignKey("Employee")]
        public string EmployeeID { get; set; }
        public virtual Employee? Employee { get; set; }

        // --- CẬP NHẬT THEO YÊU CẦU ---
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSalary { get; set; } // Lương cơ bản

        [MaxLength(100)]
        public string? ManagementLevel { get; set; } // Cấp bậc quản lý
    }
}
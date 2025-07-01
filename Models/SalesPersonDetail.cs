// DecalXeAPI/Models/SalesPersonDetail.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecalXeAPI.Models
{
    public class SalesPersonDetail
    {
        [Key, ForeignKey("Employee")]
        public string EmployeeID { get; set; }
        public virtual Employee? Employee { get; set; }

        // --- CẬP NHẬT THEO YÊU CẦU ---
        [Column(TypeName = "decimal(18,2)")]
        public decimal? SalesTarget { get; set; } // Chỉ tiêu doanh số

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Active"; // Trạng thái
    }
}
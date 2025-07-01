// DecalXeAPI/Models/DesignerDetail.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecalXeAPI.Models
{
    public class DesignerDetail
    {
        [Key, ForeignKey("Employee")]
        public string EmployeeID { get; set; }
        public virtual Employee? Employee { get; set; }

        // --- CẬP NHẬT THEO YÊU CẦU ---
        [MaxLength(200)]
        public string? DesignSpecialty { get; set; } // Chuyên môn thiết kế

        [MaxLength(500)]
        public string? DesignDifficultyDescription { get; set; } // Mô tả độ khó

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DesignHoursWorked { get; set; } // Tổng số giờ làm việc

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Available"; // Trạng thái
    }
}
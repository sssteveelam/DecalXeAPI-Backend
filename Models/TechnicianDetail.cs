// DecalXeAPI/Models/TechnicianDetail.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecalXeAPI.Models
{
    public class TechnicianDetail
    {
        [Key, ForeignKey("Employee")]
        public string EmployeeID { get; set; }
        public virtual Employee? Employee { get; set; }

        // --- CẬP NHẬT THEO YÊU CẦU ---
        [MaxLength(200)]
        public string? SpecialtyArea { get; set; } // Lĩnh vực chuyên môn

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Available"; // Trạng thái: Available, Busy, On Leave
    }
}
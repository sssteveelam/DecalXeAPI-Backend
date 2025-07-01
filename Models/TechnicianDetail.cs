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

        // Các thuộc tính riêng cho Technician
        public int? YearsOfExperience { get; set; } // Số năm kinh nghiệm
        [MaxLength(200)]
        public string? Certifications { get; set; } // Các chứng chỉ
    }
}
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

        // Các thuộc tính riêng cho Designer
        [MaxLength(200)]
        public string? Specialization { get; set; } // Chuyên môn (ví dụ: "3D, Animation, Vector")
        public int? PortfolioUrl { get; set; } // Link đến portfolio
    }
}
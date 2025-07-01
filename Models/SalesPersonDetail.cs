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

        // Các thuộc tính riêng cho Sales Person
        [Column(TypeName = "decimal(5,2)")]
        public decimal? CommissionRate { get; set; } // Tỷ lệ hoa hồng
    }
}
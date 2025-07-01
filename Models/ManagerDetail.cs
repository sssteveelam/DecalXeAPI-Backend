// DecalXeAPI/Models/ManagerDetail.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecalXeAPI.Models
{
    public class ManagerDetail
    {
        [Key, ForeignKey("Employee")]
        public string EmployeeID { get; set; }
        public virtual Employee? Employee { get; set; }

        // Các thuộc tính riêng cho Manager
        [Column(TypeName = "decimal(18,2)")]
        public decimal? BudgetManaged { get; set; } // Ngân sách được giao quản lý
    }
}
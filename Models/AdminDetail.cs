// DecalXeAPI/Models/AdminDetail.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecalXeAPI.Models
{
    public class AdminDetail
    {
        [Key, ForeignKey("Employee")]
        public string EmployeeID { get; set; } // Vừa là Khóa Chính (PK), vừa là Khóa Ngoại (FK)
        public virtual Employee? Employee { get; set; } // Navigation property để trỏ về Employee

        // Thêm các thuộc tính riêng cho Admin ở đây nếu có trong tương lai
        // Ví dụ:
        [MaxLength(100)]
        public string? AccessLevel { get; set; } // Ví dụ: "Full Control", "Limited"
    }
}
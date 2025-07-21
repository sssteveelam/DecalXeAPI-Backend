using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization; // Để dùng [JsonIgnore]

namespace DecalXeAPI.Models
{
    public class Employee
    {
        [Key]
        public string EmployeeID { get; set; } = Guid.NewGuid().ToString(); // PK

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(20)] // Ví dụ: +8490...
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        // Khóa ngoại (Foreign Key): Một Employee thuộc về một Store
        [ForeignKey("Store")]
        public string StoreID { get; set; } = string.Empty; // FK_StoreID
        public Store? Store { get; set; }

        // Khóa ngoại (Foreign Key): Một Employee có thể liên kết với một Account
        [ForeignKey("Account")]
        public string? AccountID { get; set; } // FK_AccountID (có thể null nếu chưa có account đăng nhập)
        public Account? Account { get; set; }

        // --- NAVIGATION PROPERTIES HIỆN CÓ (Giữ nguyên) ---
        [JsonIgnore] // Để tránh lỗi vòng lặp JSON
        public ICollection<Design>? Designs { get; set; } // Nếu Designer là Employee

        // --- NAVIGATION PROPERTY MỚI TỪ YÊU CẦU REVIEW ---
        [JsonIgnore] // Để tránh lỗi vòng lặp JSON
        public ICollection<OrderStageHistory>? OrderStageHistories { get; set; } // Lịch sử chuyển giai đoạn Order mà nhân viên này tham gia


        // Thêm các dòng này vào cuối lớp Employee trong file Employee.cs
        public virtual AdminDetail? AdminDetail { get; set; }
        public virtual ManagerDetail? ManagerDetail { get; set; }
        public virtual SalesPersonDetail? SalesPersonDetail { get; set; }
        public virtual DesignerDetail? DesignerDetail { get; set; }
        public virtual TechnicianDetail? TechnicianDetail { get; set; }
    }
}
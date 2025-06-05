using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Để dùng thuộc tính [ForeignKey]

namespace DecalXeAPI.Models
{
    public class Account
    {
        [Key]
        public string AccountID { get; set; } = Guid.NewGuid().ToString(); // PK

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)] // PasswordHash thường dài để lưu mã hóa
        public string PasswordHash { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true; // Mặc định là Active

        // Khóa ngoại (Foreign Key): Một Account thuộc về một Role
        // [ForeignKey("Role")] // Có thể bỏ qua nếu tên cột RoleID đủ rõ ràng
        public string RoleID { get; set; } = string.Empty; // FK_RoleID

        // Navigation Property (quan hệ một-một hoặc một-nhiều về phía "một")
        // Một Account có thể có một Role. Dấu '?' nghĩa là có thể null nếu RoleID null
        public Role? Role { get; set; }

        // Navigation Properties cho quan hệ 1-1 với Customer và Employee
        // Một Account có thể là Customer hoặc Employee (hoặc cả hai nếu thiết kế cho phép)
        public Customer? Customer { get; set; }
        public Employee? Employee { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization; // Để dùng [JsonIgnore]

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
        [ForeignKey("Role")]
        public string RoleID { get; set; } = string.Empty; // FK_RoleID
        public Role? Role { get; set; }

        // --- NAVIGATION PROPERTIES HIỆN CÓ (Giữ nguyên) ---
        [JsonIgnore] // Để tránh lỗi vòng lặp JSON
        public Customer? Customer { get; set; }
        [JsonIgnore] // Để tránh lỗi vòng lặp JSON
        public Employee? Employee { get; set; }

        // --- NAVIGATION PROPERTY MỚI TỪ YÊU CẦU REVIEW ---
        [JsonIgnore] // Để tránh lỗi vòng lặp JSON
        public ICollection<DesignComment>? DesignComments { get; set; } // Các bình luận thiết kế mà tài khoản này đã gửi
    }
}
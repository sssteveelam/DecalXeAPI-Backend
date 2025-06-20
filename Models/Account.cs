using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System; // Cần cho DateTime
using System.Text.Json.Serialization; // Để dùng [JsonIgnore]

namespace DecalXeAPI.Models
{
    public class Account
    {
        [Key]
        public string AccountID { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Email { get; set; }

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // --- CỘT CŨ CHO TÍNH NĂNG QUÊN MẬT KHẨU (KHÔNG DÙNG EMAIL - SẼ XÓA) ---
        // [MaxLength(500)] // <-- XÓA DÒNG NÀY
        // public string? SecurityQuestion { get; set; } // <-- XÓA DÒNG NÀY

        // [MaxLength(255)] // <-- XÓA DÒNG NÀY
        // public string? SecurityAnswerHash { get; set; } // <-- XÓA DÒNG NÀY


        // Khóa ngoại (Foreign Key): Một Account thuộc về một Role
        [ForeignKey("Role")]
        public string RoleID { get; set; } = string.Empty;
        public Role? Role { get; set; }

        // --- NAVIGATION PROPERTIES HIỆN CÓ ---
        [JsonIgnore]
        public Customer? Customer { get; set; }
        [JsonIgnore]
        public Employee? Employee { get; set; }
        [JsonIgnore]
        public ICollection<DesignComment>? DesignComments { get; set; }
    }
}
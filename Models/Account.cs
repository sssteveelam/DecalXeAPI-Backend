using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System; // For Guid

namespace DecalXeAPI.Models
{
    public class Account
    {
        [Key]
        public string AccountID { get; set; } = Guid.NewGuid().ToString(); // PK

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Email { get; set; } // Email của tài khoản (có thể null)

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // --- CÁC CỘT LIÊN QUAN ĐẾN PASSWORD RESET TOKEN ĐÃ BỊ XÓA THEO REVIEW2 ---
        // public string? PasswordResetToken { get; set; } // ĐÃ XÓA
        // public DateTime? PasswordResetTokenExpiry { get; set; } // ĐÃ XÓA


        [ForeignKey("Role")]
        public string RoleID { get; set; } = string.Empty;
        public Role? Role { get; set; }

        // --- NAVIGATION PROPERTIES HIỆN CÓ (ĐÃ ĐIỀU CHỈNH/XÓA THEO REVIEW2) ---
        [JsonIgnore]
        public Customer? Customer { get; set; } // Giữ lại
        [JsonIgnore]
        public Employee? Employee { get; set; } // Giữ lại
        [JsonIgnore]
        public ICollection<DesignComment>? DesignComments { get; set; } // Giữ lại
    }
}
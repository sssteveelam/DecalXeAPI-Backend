// DecalXeAPI/Models/Account.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;



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

        // PasswordResetToken và PasswordResetTokenExpiry đã được xóa

        [ForeignKey("Role")]
        public string RoleID { get; set; } = string.Empty;
        public Role? Role { get; set; }

        [JsonIgnore]
        public Customer? Customer { get; set; }
        [JsonIgnore]
        public Employee? Employee { get; set; }
        [JsonIgnore]
        public ICollection<DesignComment>? DesignComments { get; set; }
    }
}



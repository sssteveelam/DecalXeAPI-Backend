// DecalXeAPI/DTOs/UpdateAccountDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class UpdateAccountDto
    {
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Email { get; set; }

        public bool IsActive { get; set; }

        [Required]
        public string RoleID { get; set; } = string.Empty;
    }
}
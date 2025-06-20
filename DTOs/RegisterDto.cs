using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Username là bắt buộc.")]
        [MaxLength(100, ErrorMessage = "Username không được vượt quá 100 ký tự.")]
        public string Username { get; set; } = string.Empty;


        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        [MaxLength(50, ErrorMessage = "Mật khẩu không được vượt quá 50 ký tự.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "RoleID là bắt buộc.")]
        public string RoleID { get; set; } = string.Empty;
    }
}
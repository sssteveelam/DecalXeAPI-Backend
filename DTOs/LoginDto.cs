using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Username là bắt buộc.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        public string Password { get; set; } = string.Empty;
    }
}
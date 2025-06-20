using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class ForgotPasswordRequestDto
    {
        [Required(ErrorMessage = "Username hoặc Email là bắt buộc.")]
        public string Identifier { get; set; } = string.Empty; // Có thể là Username hoặc Email
    }
}
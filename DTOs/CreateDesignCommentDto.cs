// DecalXeAPI/DTOs/CreateDesignCommentDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class CreateDesignCommentDto
    {
        [Required]
        [MaxLength(1000)]
        public string CommentText { get; set; } = string.Empty;

        [Required]
        public string DesignID { get; set; } = string.Empty;

        [Required]
        public string SenderAccountID { get; set; } = string.Empty;

        // ParentCommentID có thể null nếu là comment gốc
        public string? ParentCommentID { get; set; }
    }
}
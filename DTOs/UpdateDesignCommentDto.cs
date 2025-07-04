// DecalXeAPI/DTOs/UpdateDesignCommentDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class UpdateDesignCommentDto
    {
        [Required]
        [MaxLength(1000)]
        public string CommentText { get; set; } = string.Empty;
    }
}
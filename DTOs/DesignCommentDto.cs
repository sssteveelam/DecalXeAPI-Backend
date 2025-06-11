using System;

namespace DecalXeAPI.DTOs
{
    public class DesignCommentDto
    {
        public string CommentID { get; set; } = string.Empty;
        public string CommentText { get; set; } = string.Empty;
        public DateTime CommentDate { get; set; }
        public string DesignID { get; set; } = string.Empty;
        public string? SenderAccountID { get; set; }
        public string? SenderUsername { get; set; } // Username của người gửi
        public string? SenderRoleName { get; set; } // Role của người gửi
        public string? ParentCommentID { get; set; } // Để hiển thị comment cha
    }
}
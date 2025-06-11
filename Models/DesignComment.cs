using System; // Để sử dụng Guid.NewGuid(), DateTime
using System.Collections.Generic; // Để sử dụng ICollection cho Navigation Property
using System.ComponentModel.DataAnnotations; // Để sử dụng các thuộc tính như [Key], [Required], [MaxLength]
using System.ComponentModel.DataAnnotations.Schema; // Để sử dụng thuộc tính [ForeignKey]
using System.Text.Json.Serialization; // Để sử dụng [JsonIgnore]

namespace DecalXeAPI.Models
{
    public class DesignComment
    {
        [Key] // Đánh dấu CommentID là Khóa Chính (Primary Key) của bảng. Mỗi bình luận có một ID duy nhất.
        public string CommentID { get; set; } = Guid.NewGuid().ToString(); // PK, tự động sinh GUID làm ID mặc định.

        [Required] // Đánh dấu CommentText là bắt buộc (NOT NULL).
        [MaxLength(1000)] // Giới hạn độ dài tối đa của văn bản bình luận là 1000 ký tự.
        public string CommentText { get; set; } = string.Empty; // Nội dung của bình luận.

        [Required] // Đánh dấu CommentDate là bắt buộc (NOT NULL).
        public DateTime CommentDate { get; set; } = DateTime.UtcNow; // Thời điểm bình luận được gửi.

        // Khóa ngoại (Foreign Key): Bình luận này thuộc về bản Thiết kế (Design) nào.
        [ForeignKey("Design")] // Chỉ rõ rằng DesignID là khóa ngoại đến bảng "Design".
        public string DesignID { get; set; } = string.Empty; // FK, bắt buộc (NOT NULL).
        public Design? Design { get; set; } // Navigation Property: Trỏ về đối tượng Design liên quan.

        // Khóa ngoại (Foreign Key): Ai là người gửi bình luận này. Có thể là tài khoản của Khách hàng hoặc Nhân viên.
        [ForeignKey("Account")] // Chỉ rõ rằng SenderAccountID là khóa ngoại đến bảng "Account".
        public string SenderAccountID { get; set; } = string.Empty; // FK, bắt buộc (NOT NULL).
        public Account? SenderAccount { get; set; } // Navigation Property: Trỏ về đối tượng Account của người gửi.

        // Khóa ngoại (Foreign Key - Tự tham chiếu): Để hỗ trợ bình luận trả lời bình luận khác.
        [ForeignKey("ParentComment")] // Chỉ rõ rằng ParentCommentID là khóa ngoại tự trỏ về chính bảng này.
        public string? ParentCommentID { get; set; } // FK, có thể là NULL nếu đây là bình luận gốc (không phải trả lời).
        [JsonIgnore] // Tránh lỗi vòng lặp khi serialize JSON (khi Comment có ParentComment và ParentComment lại có Replies).
        public DesignComment? ParentComment { get; set; } // Navigation Property: Trỏ về bình luận cha.

        // Navigation Property (Quan hệ 1-N): Một bình luận có thể có nhiều bình luận con (trả lời).
        [JsonIgnore] // Tránh lỗi vòng lặp khi serialize JSON.
        public ICollection<DesignComment>? Replies { get; set; }
    }
}
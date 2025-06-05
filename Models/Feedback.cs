using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace DecalXeAPI.Models
{
    public class Feedback
    {
        [Key]
        public string FeedbackID { get; set; } = Guid.NewGuid().ToString(); // PK

        // Khóa ngoại (Foreign Key): Feedback này cho Order nào
        public string OrderID { get; set; } = string.Empty; // FK_OrderID
        // Navigation Property
        public Order? Order { get; set; }

        // Khóa ngoại (Foreign Key): Feedback này từ Customer nào
        public string CustomerID { get; set; } = string.Empty; // FK_CustomerID
        // Navigation Property
        public Customer? Customer { get; set; }

        [Required]
        public int Rating { get; set; } // Đánh giá (ví dụ: 1-5 sao)

        [MaxLength(1000)]
        public string? Comment { get; set; } // Bình luận chi tiết

        [Required]
        public DateTime FeedbackDate { get; set; } = DateTime.UtcNow; // Ngày gửi feedback
    }
}
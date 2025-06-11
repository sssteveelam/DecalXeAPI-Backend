using System; // Để sử dụng Guid.NewGuid(), DateTime
using System.ComponentModel.DataAnnotations; // Để sử dụng các thuộc tính như [Key], [Required], [MaxLength]
using System.ComponentModel.DataAnnotations.Schema; // Để sử dụng thuộc tính [ForeignKey]

namespace DecalXeAPI.Models
{
    public class OrderCompletionImage
    {
        [Key] // Đánh dấu ImageID là Khóa Chính (Primary Key) của bảng. Mỗi hình ảnh có một ID duy nhất.
        public string ImageID { get; set; } = Guid.NewGuid().ToString(); // PK, tự động sinh GUID làm ID mặc định.

        [Required] // Đánh dấu ImageURL là bắt buộc (NOT NULL).
        [MaxLength(500)] // Giới hạn độ dài tối đa của URL hình ảnh là 500 ký tự.
        public string ImageURL { get; set; } = string.Empty; // URL của hình ảnh đã lưu trữ (ví dụ: trên Cloud Storage).

        [MaxLength(500)] // Giới hạn độ dài tối đa của chuỗi Description.
        public string? Description { get; set; } // Mô tả ngắn gọn về nội dung của hình ảnh (có thể là NULL).

        [Required] // Đánh dấu UploadDate là bắt buộc (NOT NULL).
        public DateTime UploadDate { get; set; } = DateTime.UtcNow; // Thời điểm hình ảnh được tải lên.

        // Khóa ngoại (Foreign Key): Hình ảnh này thuộc về Đơn hàng (Order) nào.
        [ForeignKey("Order")] // Chỉ rõ rằng OrderID là khóa ngoại đến bảng "Order".
        public string OrderID { get; set; } = string.Empty; // FK, bắt buộc (NOT NULL).
        public Order? Order { get; set; } // Navigation Property: Trỏ về đối tượng Order liên quan.
    }
}
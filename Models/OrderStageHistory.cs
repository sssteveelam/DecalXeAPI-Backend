using System; // Để sử dụng Guid.NewGuid(), DateTime
using System.ComponentModel.DataAnnotations; // Để sử dụng các thuộc tính như [Key], [Required], [MaxLength]
using System.ComponentModel.DataAnnotations.Schema; // Để sử dụng thuộc tính [ForeignKey]
using System.Text.Json.Serialization; // Để sử dụng [JsonIgnore]

namespace DecalXeAPI.Models
{
    public class OrderStageHistory
    {
        [Key] // Đánh dấu OrderStageHistoryID là Khóa Chính (Primary Key) của bảng. Mỗi bản ghi lịch sử có một ID duy nhất.
        public string OrderStageHistoryID { get; set; } = Guid.NewGuid().ToString(); // PK, tự động sinh GUID làm ID mặc định.

        [Required] // Đánh dấu StageName là bắt buộc (NOT NULL).
        [MaxLength(100)] // Giới hạn độ dài tối đa của tên giai đoạn.
        public string StageName { get; set; } = string.Empty; // Tên của giai đoạn đơn hàng (ví dụ: "New Profile", "In Design", "Printing").

        [Required] // Đánh dấu ChangeDate là bắt buộc (NOT NULL).
        public DateTime ChangeDate { get; set; } = DateTime.UtcNow; // Thời điểm mà đơn hàng chuyển sang giai đoạn này.

        // Khóa ngoại (Foreign Key): Lịch sử này thuộc về Đơn hàng (Order) nào.
        [ForeignKey("Order")] // Chỉ rõ rằng OrderID là khóa ngoại đến bảng "Order".
        public string OrderID { get; set; } = string.Empty; // FK, bắt buộc (NOT NULL).
        [JsonIgnore] // Tránh lỗi vòng lặp khi serialize JSON (khi Order có History và History lại có Order).
        public Order? Order { get; set; } // Navigation Property: Trỏ về đối tượng Order liên quan.

        // Khóa ngoại (Foreign Key): Ai là Nhân viên (Employee) đã thực hiện việc chuyển giai đoạn này.
        [ForeignKey("Employee")] // Chỉ rõ rằng ChangedByEmployeeID là khóa ngoại đến bảng "Employee".
        public string? ChangedByEmployeeID { get; set; } // FK, có thể là NULL nếu việc chuyển giai đoạn được thực hiện tự động bởi hệ thống.
        public Employee? ChangedByEmployee { get; set; } // Navigation Property: Trỏ về đối tượng Employee liên quan.

        [MaxLength(500)] // Giới hạn độ dài tối đa của ghi chú.
        public string? Notes { get; set; } // Ghi chú thêm về lý do hoặc chi tiết của việc chuyển giai đoạn (có thể là NULL).

        /// <summary>
        /// Trạng thái/Giai đoạn của đơn hàng tại thời điểm ghi nhận lịch sử này
        /// Sử dụng enum OrderStage để chuẩn hóa các giai đoạn
        /// </summary>
        [Required] // Trường bắt buộc để đảm bảo mỗi lịch sử đều có giai đoạn rõ ràng
        public OrderStage Stage { get; set; } // Giai đoạn hiện tại của đơn hàng
    }
}
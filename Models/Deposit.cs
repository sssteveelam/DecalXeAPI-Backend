using System; // For Guid, DateTime
using System.ComponentModel.DataAnnotations; // For [Key], [Required], [MaxLength]
using System.ComponentModel.DataAnnotations.Schema; // For [ForeignKey], [Column]
using System.Text.Json.Serialization; // For [JsonIgnore]

namespace DecalXeAPI.Models
{
    public class Deposit
    {
        [Key]
        public string DepositID { get; set; } = Guid.NewGuid().ToString(); // PK

        [ForeignKey("Order")] // Khóa ngoại trỏ tới bảng Order
        public string OrderID { get; set; } = string.Empty;
        [JsonIgnore] // Tránh lỗi vòng lặp JSON
        public Order? Order { get; set; } // Navigation Property

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Kiểu dữ liệu decimal cho số tiền
        public decimal Amount { get; set; } // Số tiền cọc

        [Required]
        public DateTime DepositDate { get; set; } = DateTime.UtcNow; // Ngày đặt cọc

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // Phương thức thanh toán (ví dụ: "Cash", "Bank Transfer")

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Trạng thái cọc (ví dụ: "Pending", "Confirmed", "Refunded")
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // Để dùng [JsonIgnore]

namespace DecalXeAPI.Models
{
    public class OrderDetail
    {
        [Key]
        public string OrderDetailID { get; set; } = Guid.NewGuid().ToString(); // PK

        // Khóa ngoại (Foreign Key): Chi tiết này thuộc về Order nào
        [ForeignKey("Order")]
        public string OrderID { get; set; } = string.Empty; // FK_OrderID
        public Order? Order { get; set; }

        // Khóa ngoại (Foreign Key): Chi tiết này là dịch vụ nào
        [ForeignKey("DecalService")]
        public string ServiceID { get; set; } = string.Empty; // FK_ServiceID
        public DecalService? DecalService { get; set; }

        [Required]
        public int Quantity { get; set; } // Số lượng dịch vụ/sản phẩm trong chi tiết này

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Giá của dịch vụ/sản phẩm trong chi tiết này (tạm thời)
        public decimal Price { get; set; }

        // --- CỘT MỚI TỪ YÊU CẦU REVIEW ---
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ActualAreaUsed { get; set; } // Diện tích thực tế đã dùng cho item này (Nullable)
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ActualLengthUsed { get; set; } // Chiều dài thực tế đã dùng (Nullable)
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ActualWidthUsed { get; set; } // Chiều rộng thực tế đã dùng (Nullable)

        [Required] // Giá tính toán cuối cùng là bắt buộc
        [Column(TypeName = "decimal(18,2)")]
        public decimal FinalCalculatedPrice { get; set; } // Giá cuối cùng đã tính toán cho item này
    }
}
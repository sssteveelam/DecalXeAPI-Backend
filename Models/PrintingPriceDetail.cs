using System; // Để sử dụng Decimal
using System.ComponentModel.DataAnnotations; // Để sử dụng các thuộc tính như [Key], [Required], [MaxLength]
using System.ComponentModel.DataAnnotations.Schema; // Để sử dụng các thuộc tính như [Column], [ForeignKey]

namespace DecalXeAPI.Models
{
    public class PrintingPriceDetail
    {
        // ServiceID là Khóa Chính (PK) và Khóa Ngoại (FK) đồng thời.
        // Điều này thiết lập mối quan hệ 1-1 với DecalService:
        // Mỗi PrintingPriceDetail PHẢI thuộc về một và chỉ một DecalService.
        // Ngược lại, một DecalService có thể có 0 hoặc 1 PrintingPriceDetail.
        [Key] // Đánh dấu ServiceID là Khóa Chính của bảng này.
        [ForeignKey("DecalService")] // Chỉ rõ rằng ServiceID là Khóa Ngoại đến bảng "DecalService".
        public string ServiceID { get; set; } = string.Empty; // PK & FK.

        // Navigation Property: Trỏ ngược về đối tượng DecalService liên quan.
        // [JsonIgnore] KHÔNG cần ở đây vì mối quan hệ 1-1, và thường chỉ có một chiều Include.
        public DecalService? DecalService { get; set; }

        [Required] // Giá cơ bản/m2 là bắt buộc (NOT NULL).
        [Column(TypeName = "decimal(18,2)")] // Định rõ kiểu dữ liệu số thập phân cho cột trong DB.
        public decimal BasePricePerSqMeter { get; set; } // Giá cơ bản trên mỗi mét vuông.

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinLength { get; set; } // Chiều dài tối thiểu để áp dụng công thức này (có thể là NULL).
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxLength { get; set; } // Chiều dài tối đa (có thể là NULL).

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinArea { get; set; } // Diện tích tối thiểu để áp dụng công thức này (có thể là NULL).
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxArea { get; set; } // Diện tích tối đa (có thể là NULL).

        [Column(TypeName = "decimal(5,2)")] // Ví dụ: 1.0 (không ảnh hưởng), 1.2 (tăng 20%).
        public decimal? ColorPricingFactor { get; set; } // Hệ số giá dựa trên số lượng màu hoặc độ phức tạp màu (có thể là NULL).

        [MaxLength(50)] // Giới hạn độ dài tối đa của chuỗi.
        public string? PrintType { get; set; } // Kiểu in (ví dụ: "In thường", "In UV", "In cán bóng") (có thể là NULL).

        public bool IsActive { get; set; } = true; // Xác định công thức này có đang được áp dụng không (mặc định là TRUE).

        [MaxLength(500)] // Giới hạn độ dài tối đa của chuỗi.
        public string? FormulaDescription { get; set; } // Mô tả chi tiết về công thức hoặc quy tắc tính giá (có thể là NULL).
    }
}
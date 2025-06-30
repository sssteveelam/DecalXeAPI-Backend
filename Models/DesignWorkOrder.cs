using System; // For Guid
using System.ComponentModel.DataAnnotations; // For [Key], [Required], [MaxLength]
using System.ComponentModel.DataAnnotations.Schema; // For [ForeignKey], [Column]
using System.Text.Json.Serialization; // For [JsonIgnore]

namespace DecalXeAPI.Models
{
    public class DesignWorkOrder
    { 
 // DesignID là Khóa Chính (PK) và Khóa Ngoại (FK) đồng thời.
    // Điều này thiết lập mối quan hệ 1-1 với Design:
    // Mỗi DesignWorkOrder PHẢI thuộc về một và chỉ một Design.
    // Ngược lại, một Design có thể có 0 hoặc 1 DesignWorkOrder.
    [Key] // Đánh dấu DesignID là Khóa Chính của bảng này.
    [ForeignKey("Design")] // Chỉ rõ rằng DesignID là khóa ngoại đến bảng "Design".
    public string DesignID { get; set; } = string.Empty; // PK & FK.

    // Navigation Property: Trỏ ngược về đối tượng Design liên quan.
    [JsonIgnore] // Tránh lỗi vòng lặp JSON
    public Design? Design { get; set; }

    [ForeignKey("DecalService")] // Khóa ngoại trỏ tới bảng DecalService (dịch vụ thiết kế chung)
    public string ServiceID { get; set; } = string.Empty;
    [JsonIgnore] // Tránh lỗi vòng lặp JSON
    public DecalService? DecalService { get; set; } // Navigation Property

    [Required]
    public int HoursWorked { get; set; } // Số giờ designer đã làm

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal DesignCost { get; set; } // Chi phí tính toán cho công việc thiết kế

    [MaxLength(500)]
    public string? DesignResult { get; set; } // Kết quả cuối cùng của công việc thiết kế (có thể là mô tả hoặc URL)


    }
   
}
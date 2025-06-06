using System.ComponentModel.DataAnnotations;
using System; // Để dùng DateTime
using System.Text.Json.Serialization;


namespace DecalXeAPI.Models
{
    public class Promotion
    {
        [Key]
        public string PromotionID { get; set; } = Guid.NewGuid().ToString(); // PK

        [Required]
        [MaxLength(100)]
        public string PromotionName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(1); // Mặc định kết thúc sau 1 tháng

        [Required]
        public decimal DiscountPercentage { get; set; } // Phần trăm giảm giá (ví dụ: 0.1 cho 10%)

        // Navigation Property: Một Promotion có thể được áp dụng cho nhiều Payments
        [JsonIgnore]
        public ICollection<Payment>? Payments { get; set; }
    }
}
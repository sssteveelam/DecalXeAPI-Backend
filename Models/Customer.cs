using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DecalXeAPI.Models
{
    public class Customer
    {
        [Key]
        public string CustomerID { get; set; } = Guid.NewGuid().ToString(); // PK

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty; // Số điện thoại bắt buộc

        [MaxLength(100)]
        public string? Email { get; set; } // Email có thể null (khách vãng lai)

        [MaxLength(255)]
        public string? Address { get; set; }

        // Khóa ngoại (Foreign Key): Một Customer có thể liên kết với một Account
        // [ForeignKey("Account")]
        public string? AccountID { get; set; } // FK_AccountID (có thể null nếu khách không đăng ký tài khoản)

        // Navigation Property: Một Customer có thể có một Account
        public Account? Account { get; set; }

        // Navigation Properties cho các mối quan hệ một-nhiều
        [JsonIgnore]
        public ICollection<Order>? Orders { get; set; }
        [JsonIgnore]
        public ICollection<Feedback>? Feedbacks { get; set; }
        [JsonIgnore]
        public ICollection<CustomServiceRequest>? CustomServiceRequests { get; set; }
    }
}
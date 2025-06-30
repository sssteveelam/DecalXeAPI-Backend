using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System; // For Guid

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
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [ForeignKey("Account")]
        public string? AccountID { get; set; }
        public Account? Account { get; set; }

        // --- NAVIGATION PROPERTIES HIỆN CÓ (ĐIỀU CHỈNH THEO REVIEW2) ---
        [JsonIgnore]
        public ICollection<Order>? Orders { get; set; } // Giữ lại
        
        [JsonIgnore]
        public ICollection<Feedback>? Feedbacks { get; set; } // Giữ lại
        
        [JsonIgnore]
        public ICollection<CustomServiceRequest>? CustomServiceRequests { get; set; } // Giữ lại

        // Navigation Property từ Review1 (đã có)
        [JsonIgnore]
        public ICollection<CustomerVehicle>? CustomerVehicles { get; set; } // Giữ lại
        
        // Các Navigation Property cũ đã bị xóa vì bảng liên quan bị xóa:
        // (Không có thêm NP mới cho Deposit vì Deposit liên kết với Order, không phải Customer trực tiếp)
        // (Không có NP nào bị xóa khỏi Customer trong đợt này)
    }
}

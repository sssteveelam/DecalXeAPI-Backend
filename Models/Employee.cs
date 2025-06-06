using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DecalXeAPI.Models
{
    public class Employee
    {
        [Key]
        public string EmployeeID { get; set; } = Guid.NewGuid().ToString(); // PK

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(20)] // Ví dụ: +8490...
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        // Khóa ngoại (Foreign Key): Một Employee thuộc về một Store
        // [ForeignKey("Store")]
        public string StoreID { get; set; } = string.Empty; // FK_StoreID

        // Navigation Property: Một Employee có một Store
        public Store? Store { get; set; }

        // Khóa ngoại (Foreign Key): Một Employee có thể liên kết với một Account
        // [ForeignKey("Account")]
        public string? AccountID { get; set; } // FK_AccountID (có thể null nếu chưa có account đăng nhập)

        // Navigation Property: Một Employee có thể có một Account
        public Account? Account { get; set; }

        // Navigation Properties cho các mối quan hệ một-nhiều
        [JsonIgnore]
        public ICollection<TechnicianDailySchedule>? TechnicianDailySchedules { get; set; }
        [JsonIgnore]
        public ICollection<Design>? Designs { get; set; } // Nếu Designer là Employee
        [JsonIgnore]
        public ICollection<CustomServiceRequest>? SalesRequests { get; set; } // Nếu SalesEmployee là Employee
    }
}
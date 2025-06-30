using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System; // For Guid

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

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [ForeignKey("Store")]
        public string StoreID { get; set; } = string.Empty;
        public Store? Store { get; set; }

        [ForeignKey("Account")]
        public string? AccountID { get; set; }
        public Account? Account { get; set; }

        // --- CÁC NAVIGATION PROPERTIES MỚI CHO CÁC VAI TRÒ NHÂN VIÊN TÁCH RỜI ---
        [JsonIgnore]
        public AdminDetail? AdminDetail { get; set; } // Một Employee có thể có 0 hoặc 1 AdminDetail

        [JsonIgnore]
        public ManagerDetail? ManagerDetail { get; set; } // Một Employee có thể có 0 hoặc 1 ManagerDetail

        [JsonIgnore]
        public SalesPersonDetail? SalesPersonDetail { get; set; } // Một Employee có thể có 0 hoặc 1 SalesPersonDetail

        [JsonIgnore]
        public DesignerDetail? DesignerDetail { get; set; } // Một Employee có thể có 0 hoặc 1 DesignerDetail

        [JsonIgnore]
        public TechnicianDetail? TechnicianDetail { get; set; } // Một Employee có thể có 0 hoặc 1 TechnicianDetail


        // --- NAVIGATION PROPERTIES HIỆN CÓ (ĐÃ ĐIỀU CHỈNH/XÓA THEO REVIEW2) ---
        [JsonIgnore]
        public ICollection<Order>? Orders { get; set; } // Giữ lại: Employee có thể là AssignedEmployee của nhiều Order
        [JsonIgnore]
        public ICollection<CustomServiceRequest>? SalesRequests { get; set; } // Giữ lại: Employee có thể là SalesEmployee của nhiều CustomServiceRequest
        [JsonIgnore]
        public ICollection<Design>? Designs { get; set; } // Giữ lại: Employee có thể là Designer của nhiều Design
        [JsonIgnore]
        public ICollection<OrderStageHistory>? OrderStageHistories { get; set; } // Giữ lại: Employee có thể thay đổi nhiều OrderStageHistory
        // Các Navigation Property cũ đã bị xóa vì bảng liên quan bị xóa:
        // public ICollection<TechnicianDailySchedule>? TechnicianDailySchedules { get; set; } // ĐÃ XÓA
        // public ICollection<ScheduledWorkUnit>? ScheduledWorkUnits { get; set; } // ĐÃ XÓA
    }
}
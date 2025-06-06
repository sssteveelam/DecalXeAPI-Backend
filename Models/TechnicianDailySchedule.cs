using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System; // Để dùng DateTime
using System.Text.Json.Serialization;

namespace DecalXeAPI.Models
{
    public class TechnicianDailySchedule
    {
        [Key]
        public string DailyScheduleID { get; set; } = Guid.NewGuid().ToString(); // PK

        // Khóa ngoại (Foreign Key): Lịch làm việc này của Employee nào
        public string EmployeeID { get; set; } = string.Empty; // FK_EmployeeID

        // Navigation Property: Một TechnicianDailySchedule có một Employee
        public Employee? Employee { get; set; }

        [Required]
        public DateTime ScheduleDate { get; set; } // Ngày của lịch làm việc

        public int TotalAvailableWorkUnits { get; set; } // Tổng số xuất công khả dụng trong ngày

        // Navigation Property: Một TechnicianDailySchedule có nhiều ScheduledWorkUnit
        [JsonIgnore]
        public ICollection<ScheduledWorkUnit>? ScheduledWorkUnits { get; set; }
    }
}
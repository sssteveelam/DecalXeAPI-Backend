using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace DecalXeAPI.Models
{
    public class ScheduledWorkUnit
    {
        [Key]
        public string ScheduledWorkUnitID { get; set; } = Guid.NewGuid().ToString(); // PK

        // Khóa ngoại (Foreign Key): Xuất công này thuộc về lịch làm việc ngày nào của kỹ thuật viên
        public string DailyScheduleID { get; set; } = string.Empty; // FK_DailyScheduleID
        // Navigation Property: Một ScheduledWorkUnit có một TechnicianDailySchedule
        public TechnicianDailySchedule? DailySchedule { get; set; }

        // Khóa ngoại (Foreign Key): Xuất công này ứng với khung giờ nào (ví dụ: 07:30-08:00)
        public string SlotDefID { get; set; } = string.Empty; // FK_SlotDefID
        // Navigation Property: Một ScheduledWorkUnit có một TimeSlotDefinition
        public TimeSlotDefinition? TimeSlotDefinition { get; set; }

        // Khóa ngoại (Foreign Key): Xuất công này được gán cho đơn hàng nào (có thể null nếu chưa gán)
        public string? OrderID { get; set; } // FK_OrderID (cho phép null nếu slot đang Available)
        // Navigation Property: Một ScheduledWorkUnit có thể liên kết với một Order
        public Order? Order { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Available"; // Trạng thái của xuất công (ví dụ: "Available", "Booked", "Completed")
    }
}
namespace DecalXeAPI.DTOs
{
    public class ScheduledWorkUnitDto
    {
        public string ScheduledWorkUnitID { get; set; } = string.Empty;
        public string DailyScheduleID { get; set; } = string.Empty;
        public DateTime ScheduleDate { get; set; } // Ngày của lịch làm việc
        public string SlotDefID { get; set; } = string.Empty;
        public TimeSpan SlotStartTime { get; set; } // Thời gian bắt đầu của slot
        public TimeSpan SlotEndTime { get; set; } // Thời gian kết thúc của slot
        public string? OrderID { get; set; }
        public string? OrderStatus { get; set; } // Trạng thái đơn hàng liên quan
        public string Status { get; set; } = string.Empty;
    }
}
using System;

namespace DecalXeAPI.DTOs
{
    public class TechnicianDailyScheduleDto
    {
        public string DailyScheduleID { get; set; } = string.Empty;
        public string EmployeeID { get; set; } = string.Empty;
        public string EmployeeFullName { get; set; } = string.Empty; // Tên đầy đủ kỹ thuật viên
        public DateTime ScheduleDate { get; set; }
        public int TotalAvailableWorkUnits { get; set; }
    }
}
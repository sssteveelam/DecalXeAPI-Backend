using System;

namespace DecalXeAPI.DTOs
{
    public class TimeSlotDefinitionDto
    {
        public string SlotDefID { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int DurationMinutes { get; set; }
    }
}
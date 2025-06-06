using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System; // Để dùng TimeSpan
using System.Text.Json.Serialization;

namespace DecalXeAPI.Models
{
    public class TimeSlotDefinition
    {
        [Key]
        public string SlotDefID { get; set; } = Guid.NewGuid().ToString();

        // TimeSpan là kiểu dữ liệu phù hợp cho khoảng thời gian trong C#.
        // EF Core sẽ tự động ánh xạ sang kiểu Time trong PostgreSQL.
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public int DurationMinutes { get; set; } // Thời lượng của slot (ví dụ: 30 phút)

        // Navigation Property: Một TimeSlotDefinition có thể liên kết với nhiều ScheduledWorkUnit.
        [JsonIgnore]
        public ICollection<ScheduledWorkUnit>? ScheduledWorkUnits { get; set; }
    }
}
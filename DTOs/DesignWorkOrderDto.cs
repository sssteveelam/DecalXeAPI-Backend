// DecalXeAPI/DTOs/DesignWorkOrderDto.cs
namespace DecalXeAPI.DTOs
{
    public class DesignWorkOrderDto
    {
        public string WorkOrderID { get; set; } = string.Empty;
        public string DesignID { get; set; } = string.Empty;
        public string OrderID { get; set; } = string.Empty;
        public decimal EstimatedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public decimal Cost { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Requirements { get; set; }
    }
}
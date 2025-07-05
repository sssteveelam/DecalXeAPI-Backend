// DecalXeAPI/DTOs/UpdateDesignWorkOrderDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class UpdateDesignWorkOrderDto
    {
        // Khi cập nhật, ta không cho phép đổi DesignID và OrderID
        public decimal EstimatedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public decimal Cost { get; set; }
        [Required]
        public string Status { get; set; } = string.Empty;
        public string? Requirements { get; set; }
    }
}
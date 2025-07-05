// DecalXeAPI/DTOs/CreateDesignWorkOrderDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class CreateDesignWorkOrderDto
    {
        [Required]
        public string DesignID { get; set; } = string.Empty;
        [Required]
        public string OrderID { get; set; } = string.Empty;
        [Required]
        public decimal EstimatedHours { get; set; }
        [Required]
        public decimal Cost { get; set; }
        [Required]
        public string Status { get; set; } = "Pending";
        public string? Requirements { get; set; }
    }
}
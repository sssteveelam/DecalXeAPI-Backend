// DecalXeAPI/DTOs/UpdateOrderDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class UpdateOrderDto
    {
        // Khi cập nhật, không cho phép đổi CustomerID
        public string? AssignedEmployeeID { get; set; }
        public string? VehicleID { get; set; }
        public DateTime? ExpectedArrivalTime { get; set; }
        [Required]
        public string CurrentStage { get; set; } = string.Empty;
        [Required]
        public string OrderStatus { get; set; } = string.Empty;
        public string? Priority { get; set; }
    }
}
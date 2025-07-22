// DecalXeAPI/DTOs/OrderDto.cs
using System;

namespace DecalXeAPI.DTOs
{
    public class OrderDto
    {
        public string OrderID { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string? AssignedEmployeeID { get; set; }
        public string? AssignedEmployeeFullName { get; set; }
        public string? VehicleID { get; set; }

        // --- CẬP NHẬT Ở ĐÂY ---
        public string? ChassisNumber { get; set; } = string.Empty; // Thay cho LicensePlate
        public string? VehicleModelName { get; set; } = string.Empty; // Thay cho CarModelName
        public string? VehicleBrandName { get; set; } = string.Empty; // Thay cho CarBrandName
        // ------------------------

        public DateTime? ExpectedArrivalTime { get; set; }
        public string CurrentStage { get; set; } = string.Empty;
        public string? Priority { get; set; }
        public bool IsCustomDecal { get; set; }
    }
}
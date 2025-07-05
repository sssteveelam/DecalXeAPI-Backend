// DecalXeAPI/DTOs/CreateOrderDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class CreateOrderDto
    {
        [Required]
        public string CustomerID { get; set; } = string.Empty;

        public string? AssignedEmployeeID { get; set; }

        public string? VehicleID { get; set; }

        public DateTime? ExpectedArrivalTime { get; set; }

        public string? Priority { get; set; }

        public bool IsCustomDecal { get; set; } = false;
    }
}
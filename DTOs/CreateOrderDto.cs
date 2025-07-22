// DecalXeAPI/DTOs/CreateOrderDto.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecalXeAPI.DTOs
{
    public class CreateOrderDto
    {
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public string? AssignedEmployeeID { get; set; }
        public string? VehicleID { get; set; }
        public DateTime? ExpectedArrivalTime { get; set; }
        public string? Priority { get; set; }
        public bool IsCustomDecal { get; set; } = false;

        // Thêm mô tả cho đơn hàng, đặc biệt quan trọng với đơn hàng tùy chỉnh
        [MaxLength(1000)]
        public string? Description { get; set; } 
    }
}
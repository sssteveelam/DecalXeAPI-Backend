// DecalXeAPI/DTOs/WarrantyDto.cs
using System;

namespace DecalXeAPI.DTOs
{
    public class WarrantyDto
    {
        public string WarrantyID { get; set; } = string.Empty;

        // --- THAY ĐỔI Ở ĐÂY ---
        public string VehicleID { get; set; } = string.Empty; // Thêm mới
        public string ChassisNumber { get; set; } = string.Empty; // Thêm mới: Hiển thị số khung cho tiện
        public string CustomerFullName { get; set; } = string.Empty; // Thêm mới: Hiển thị tên chủ xe
        // OrderID và OrderStatus đã bị xóa
        // ---------------------------

        public DateTime WarrantyStartDate { get; set; }
        public DateTime WarrantyEndDate { get; set; }
        public string WarrantyType { get; set; } = string.Empty;
        public string WarrantyStatus { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Notes { get; set; }
    }
}
using System;

namespace DecalXeAPI.DTOs
{
    public class WarrantyDto
    {
        public string WarrantyID { get; set; } = string.Empty;
        public string OrderID { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty; // Trạng thái đơn hàng liên quan
        public DateTime WarrantyStartDate { get; set; }
        public DateTime WarrantyEndDate { get; set; }
        public string WarrantyType { get; set; } = string.Empty;
        public string WarrantyStatus { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Notes { get; set; }
    }
}
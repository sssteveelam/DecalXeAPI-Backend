using System;

namespace DecalXeAPI.DTOs
{
    public class OrderDto
    {
        public string OrderID { get; set; } = string.Empty;
        public string CustomerID { get; set; } = string.Empty;
        public string CustomerFullName { get; set; } = string.Empty; // Tên đầy đủ khách hàng
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string? AssignedEmployeeID { get; set; }
        public string? AssignedEmployeeFullName { get; set; } // Tên đầy đủ nhân viên được giao
        public string? CustomServiceRequestID { get; set; } // ID của yêu cầu tùy chỉnh nếu có
        public string? CustomServiceRequestDescription { get; set; } // Mô tả yêu cầu tùy chỉnh
    }
}
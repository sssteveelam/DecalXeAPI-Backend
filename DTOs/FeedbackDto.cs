using System;

namespace DecalXeAPI.DTOs
{
    public class FeedbackDto
    {
        public string FeedbackID { get; set; } = string.Empty;
        public string OrderID { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty; // Trạng thái đơn hàng liên quan
        public string CustomerID { get; set; } = string.Empty;
        public string CustomerFullName { get; set; } = string.Empty; // Tên đầy đủ khách hàng
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime FeedbackDate { get; set; }
    }
}
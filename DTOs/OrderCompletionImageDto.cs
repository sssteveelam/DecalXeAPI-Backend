using System;

namespace DecalXeAPI.DTOs
{
    public class OrderCompletionImageDto
    {
        public string ImageID { get; set; } = string.Empty;
        public string ImageURL { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime UploadDate { get; set; }
        public string OrderID { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty; // Trạng thái đơn hàng liên quan
    }
}
namespace DecalXeAPI.DTOs
{
    public class OrderDetailDto
    {
        public string OrderDetailID { get; set; } = string.Empty;
        public string OrderID { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty; // Trạng thái đơn hàng liên quan
        public string ServiceID { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty; // Tên dịch vụ
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        // --- THUỘC TÍNH MỚI TỪ YÊU CẦU REVIEW ---
        public decimal? ActualAreaUsed { get; set; }
        public decimal? ActualLengthUsed { get; set; }
        public decimal? ActualWidthUsed { get; set; }
        public decimal FinalCalculatedPrice { get; set; } // Giá cuối cùng đã tính toán cho item này
    }
}
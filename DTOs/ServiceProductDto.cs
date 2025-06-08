namespace DecalXeAPI.DTOs
{
    public class ServiceProductDto
    {
        public string ServiceProductID { get; set; } = string.Empty;
        public string ServiceID { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty; // Để hiển thị tên dịch vụ
        public string ProductID { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty; // Để hiển thị tên sản phẩm
        public decimal QuantityUsed { get; set; }
    }
}
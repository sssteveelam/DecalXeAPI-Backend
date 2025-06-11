namespace DecalXeAPI.DTOs
{
    public class CustomerVehicleDto
    {
        public string VehicleID { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string? Color { get; set; }
        public int? Year { get; set; }
        public decimal? InitialKM { get; set; }
        public string CustomerID { get; set; } = string.Empty;
        public string CustomerFullName { get; set; } = string.Empty; // Tên đầy đủ khách hàng
        public string ModelID { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty; // Tên mẫu xe
        public string BrandName { get; set; } = string.Empty; // Tên hãng xe
    }
}
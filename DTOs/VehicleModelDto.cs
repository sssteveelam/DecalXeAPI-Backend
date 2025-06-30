namespace DecalXeAPI.DTOs
{
    // ĐỔI TÊN TỪ CarModelDto
    public class VehicleModelDto
    {
        public string ModelID { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string BrandID { get; set; } = string.Empty; // ID của hãng xe
        public string BrandName { get; set; } = string.Empty; // Tên hãng xe (sẽ được ánh xạ từ VehicleModel.VehicleBrand.BrandName)
    }
}
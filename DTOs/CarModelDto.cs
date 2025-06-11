namespace DecalXeAPI.DTOs
{
    public class CarModelDto
    {
        public string ModelID { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string BrandID { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty; // Tên hãng xe để hiển thị
    }
}
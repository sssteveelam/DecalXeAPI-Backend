// DecalXeAPI/DTOs/VehicleModelDto.cs
namespace DecalXeAPI.DTOs
{
    public class VehicleModelDto // <-- Đổi từ CarModelDto
    {
        public string ModelID { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string BrandID { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
    }
}
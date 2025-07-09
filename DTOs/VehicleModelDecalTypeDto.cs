// DecalXeAPI/DTOs/VehicleModelDecalTypeDto.cs
namespace DecalXeAPI.DTOs
{
    public class VehicleModelDecalTypeDto
    {
        public string VehicleModelDecalTypeID { get; set; } = string.Empty;

        public string ModelID { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty; // Tên xe để hiển thị

        public string DecalTypeID { get; set; } = string.Empty;
        public string DecalTypeName { get; set; } = string.Empty; // Tên loại decal để hiển thị

        public decimal Price { get; set; } // Giá tiền cho sự kết hợp này
    }
}
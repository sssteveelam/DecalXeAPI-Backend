// DecalXeAPI/DTOs/TechLaborPriceDto.cs
namespace DecalXeAPI.DTOs
{
    public class TechLaborPriceDto
    {
        public string ServiceID { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty; // Thêm tên để hiển thị cho dễ
        public string VehicleModelID { get; set; } = string.Empty;
        public string VehicleModelName { get; set; } = string.Empty; // Thêm tên để hiển thị cho dễ
        public decimal LaborPrice { get; set; }
    }
}
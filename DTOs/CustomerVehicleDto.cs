// DecalXeAPI/DTOs/CustomerVehicleDto.cs
namespace DecalXeAPI.DTOs
{
    public class CustomerVehicleDto
    {
        public string VehicleID { get; set; } = string.Empty;
        // Đổi LicensePlate thành ChassisNumber để đồng bộ với Model
        public string ChassisNumber { get; set; } = string.Empty;
        public string? LicensePlate { get; set; } // Biển số xe 
        public string? Color { get; set; }
        public int? Year { get; set; }
        public decimal? InitialKM { get; set; }
        public string CustomerID { get; set; } = string.Empty;
        public string CustomerFullName { get; set; } = string.Empty; 
        public string ModelID { get; set; } = string.Empty;
        // Đổi BrandName và ModelName để nhất quán (mặc dù không bắt buộc)
        public string VehicleModelName { get; set; } = string.Empty; 
        public string VehicleBrandName { get; set; } = string.Empty; 
    }
}
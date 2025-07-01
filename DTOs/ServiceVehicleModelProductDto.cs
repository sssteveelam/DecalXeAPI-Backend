// DecalXeAPI/DTOs/ServiceVehicleModelProductDto.cs
namespace DecalXeAPI.DTOs
{
    public class ServiceVehicleModelProductDto
    {
        public string ServiceID { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;

        public string VehicleModelID { get; set; } = string.Empty;
        public string VehicleModelName { get; set; } = string.Empty;

        public string ProductID { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }
    }
}
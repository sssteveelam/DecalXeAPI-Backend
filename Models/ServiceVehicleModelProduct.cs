// DecalXeAPI/Models/ServiceVehicleModelProduct.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecalXeAPI.Models
{
    public class ServiceVehicleModelProduct
    {
        // Đây là khóa phức hợp, sẽ được định nghĩa trong ApplicationDbContext

        [ForeignKey("DecalService")]
        public string ServiceID { get; set; } = string.Empty; // Dịch vụ nào
        public DecalService? DecalService { get; set; }

        [ForeignKey("VehicleModel")]
        public string VehicleModelID { get; set; } = string.Empty; // Mẫu xe nào
        public VehicleModel? VehicleModel { get; set; }

        [ForeignKey("Product")]
        public string ProductID { get; set; } = string.Empty; // Cần sản phẩm (vật tư) nào
        public Product? Product { get; set; }

        [Required]
        public int Quantity { get; set; } // Số lượng vật tư cần
    }
}
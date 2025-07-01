// DecalXeAPI/Models/TechLaborPrice.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecalXeAPI.Models
{
    public class TechLaborPrice
    {
        // Đây là khóa phức hợp, sẽ được định nghĩa trong ApplicationDbContext

        [ForeignKey("DecalService")]
        public string ServiceID { get; set; } = string.Empty; // Dịch vụ nào
        public DecalService? DecalService { get; set; }

        [ForeignKey("VehicleModel")]
        public string VehicleModelID { get; set; } = string.Empty; // Mẫu xe nào
        public VehicleModel? VehicleModel { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal LaborPrice { get; set; } // Giá công là bao nhiêu
    }
}
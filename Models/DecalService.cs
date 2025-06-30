using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System; // For Guid

namespace DecalXeAPI.Models
{
    public class DecalService
    {
        [Key]
        public string ServiceID { get; set; } = Guid.NewGuid().ToString(); // PK

        [Required]
        [MaxLength(100)]
        public string ServiceName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int StandardWorkUnits { get; set; } // Số lượng xuất công tiêu chuẩn cho dịch vụ này

        [ForeignKey("DecalType")]
        public string DecalTypeID { get; set; } = string.Empty;
        public DecalType? DecalType { get; set; }

        // Mối quan hệ 1-0..1 với PrintingPriceDetail
        [JsonIgnore]
        public PrintingPriceDetail? PrintingPriceDetail { get; set; }

        // --- CÁC NAVIGATION PROPERTIES ĐƯỢC ĐIỀU CHỈNH/THÊM THEO REVIEW2 ---
        [JsonIgnore]
        public ICollection<OrderDetail>? OrderDetails { get; set; } // Giữ lại
        
        [JsonIgnore]
        public ICollection<ServiceDecalTemplate>? ServiceDecalTemplates { get; set; } // Giữ lại

        // public ICollection<ServiceProduct>? ServiceProducts { get; set; } // <-- ĐÃ XÓA (thay bằng ServiceCarModelProduct)

        [JsonIgnore]
        public ICollection<ServiceVehicleModelProduct>? ServiceVehicleModelProducts { get; set; } // <-- MỚI: Bảng liên kết 3 chiều

        [JsonIgnore]
        public ICollection<TechLaborPrice>? TechLaborPrices { get; set; } // <-- MỚI: Bảng giá công kỹ thuật

        [JsonIgnore]
        public ICollection<DesignWorkOrder>? DesignWorkOrders { get; set; } // <-- MỚI: Bảng đơn hàng công việc thiết kế
    }
}

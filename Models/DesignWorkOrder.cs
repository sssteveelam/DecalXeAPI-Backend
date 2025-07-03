// DecalXeAPI/Models/DesignWorkOrder.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecalXeAPI.Models
{
    public class DesignWorkOrder
    {
        [Key]
        public string WorkOrderID { get; set; } = Guid.NewGuid().ToString();

        // Liên kết 1-1 với Design
        [ForeignKey("Design")]
        public string DesignID { get; set; } = string.Empty; // Kết quả của công việc này là thiết kế nào
        public Design? Design { get; set; }

        [ForeignKey("Order")]
        public string OrderID { get; set; } = string.Empty; // Công việc này thuộc đơn hàng nào
        public Order? Order { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedHours { get; set; } // Số giờ dự kiến

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ActualHours { get; set; } // Số giờ thực tế

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; } // Chi phí của công việc thiết kế

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Trạng thái công việc

        [MaxLength(1000)]
        public string? Requirements { get; set; } // Yêu cầu chi tiết
    }
}


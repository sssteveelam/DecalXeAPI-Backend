// DecalXeAPI/DTOs/CreateOrderStageHistoryDto.cs
using System.ComponentModel.DataAnnotations;
using DecalXeAPI.Models;

namespace DecalXeAPI.DTOs
{
    public class CreateOrderStageHistoryDto
    {
        [Required]
        [MaxLength(100)]
        public string StageName { get; set; } = string.Empty;

        [Required]
        public string OrderID { get; set; } = string.Empty;

        public string? ChangedByEmployeeID { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Giai đoạn của đơn hàng (bắt buộc)
        /// </summary>
        [Required]
        public OrderStage Stage { get; set; }
    }
}

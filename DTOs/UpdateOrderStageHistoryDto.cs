// DecalXeAPI/DTOs/UpdateOrderStageHistoryDto.cs
using System.ComponentModel.DataAnnotations;
using DecalXeAPI.Models;

namespace DecalXeAPI.DTOs
{
    public class UpdateOrderStageHistoryDto
    {
        [MaxLength(100)]
        public string? StageName { get; set; }

        public string? ChangedByEmployeeID { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Giai đoạn của đơn hàng
        /// </summary>
        public OrderStage? Stage { get; set; }
    }
}

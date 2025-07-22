using System;
using DecalXeAPI.Models;

namespace DecalXeAPI.DTOs
{
    public class OrderStageHistoryDto
    {
        public string OrderStageHistoryID { get; set; } = string.Empty;
        public string StageName { get; set; } = string.Empty;
        public DateTime ChangeDate { get; set; }
        public string OrderID { get; set; } = string.Empty;
        public string? ChangedByEmployeeID { get; set; }
        public string? ChangedByEmployeeFullName { get; set; } // Tên đầy đủ nhân viên thay đổi
        public string? Notes { get; set; }
        
        /// <summary>
        /// Giai đoạn của đơn hàng (sử dụng enum)
        /// </summary>
        public OrderStage Stage { get; set; }
        
        /// <summary>
        /// Tên giai đoạn bằng tiếng Việt
        /// </summary>
        public string StageDescription { get; set; } = string.Empty;
        
        /// <summary>
        /// Phần trăm hoàn thành dựa trên giai đoạn hiện tại
        /// </summary>
        public int CompletionPercentage { get; set; }
    }
}
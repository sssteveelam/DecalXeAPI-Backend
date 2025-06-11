using System;

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
    }
}
using System;

namespace DecalXeAPI.DTOs
{
    public class SalesStatisticsDto
    {
        public DateTime Date { get; set; } // Ngày hoặc tháng của thống kê
        public decimal TotalSalesAmount { get; set; } // Tổng doanh thu
        public int TotalOrders { get; set; } // Tổng số đơn hàng
    }
}
namespace DecalXeAPI.DTOs
{
    public class EmployeePerformanceDto
    {
        public string EmployeeID { get; set; } = string.Empty;
        public string EmployeeFullName { get; set; } = string.Empty;
        public int CompletedWorkUnits { get; set; } // Số lượng xuất công hoàn thành
        public int TotalAssignedWorkUnits { get; set; } // Tổng số xuất công được giao
        public decimal CompletionRate { get; set; } // Tỷ lệ hoàn thành
    }
}
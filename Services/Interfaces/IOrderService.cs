using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.QueryParams;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IOrderService
    {
        // Phương thức lấy danh sách đơn hàng có tìm kiếm/lọc/sắp xếp/phân trang
        // Sẽ trả về một tuple hoặc một đối tượng tùy chỉnh để bao gồm cả totalCount và danh sách DTOs
        Task<(IEnumerable<OrderDto> Orders, int TotalCount)> GetOrdersAsync(OrderQueryParams queryParams);

        // Phương thức lấy đơn hàng theo ID
        Task<OrderDto?> GetOrderByIdAsync(string id); // Dấu ? cho biết có thể trả về null

        // Phương thức tạo đơn hàng mới
        Task<OrderDto> CreateOrderAsync(Order order);

        // Phương thức cập nhật đơn hàng
        Task<bool> UpdateOrderAsync(string id, Order order);

        // Phương thức xóa đơn hàng
        Task<bool> DeleteOrderAsync(string id);

        // Phương thức cập nhật trạng thái đơn hàng (đã tách ra làm API riêng)
        Task<bool> UpdateOrderStatusAsync(string id, string newStatus);

        // Phương thức lấy thống kê doanh thu
        Task<IEnumerable<SalesStatisticsDto>> GetSalesStatisticsAsync(DateTime? startDate, DateTime? endDate);

        // Phương thức kiểm tra sự tồn tại của Order (có thể dùng nội bộ trong Service)
        Task<bool> OrderExistsAsync(string id);

        Task<(bool Success, string? ErrorMessage)> AssignEmployeeToOrderAsync(string orderId, string employeeId);
        Task<(bool Success, string? ErrorMessage)> UnassignEmployeeFromOrderAsync(string orderId);
        Task<EmployeeDto?> GetAssignedEmployeeForOrderAsync(string orderId);
    }
}
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IOrderDetailService
    {
        // Phương thức lấy danh sách chi tiết đơn hàng
        Task<IEnumerable<OrderDetailDto>> GetOrderDetailsAsync();

        // Phương thức lấy chi tiết đơn hàng theo ID
        Task<OrderDetailDto?> GetOrderDetailByIdAsync(string id);

        // Phương thức tạo chi tiết đơn hàng mới (có logic tồn kho và cập nhật tổng tiền)
        Task<(OrderDetailDto? OrderDetail, string? ErrorMessage)> CreateOrderDetailAsync(OrderDetail orderDetail);

        // Phương thức cập nhật chi tiết đơn hàng (có logic tồn kho và cập nhật tổng tiền)
        Task<(bool Success, string? ErrorMessage)> UpdateOrderDetailAsync(string id, OrderDetail orderDetail);

        // Phương thức xóa chi tiết đơn hàng (có logic tồn kho và cập nhật tổng tiền)
        Task<(bool Success, string? ErrorMessage)> DeleteOrderDetailAsync(string id);

        // Phương thức kiểm tra sự tồn tại của OrderDetail (có thể dùng nội bộ trong Service)
        Task<bool> OrderDetailExistsAsync(string id);
    }
}
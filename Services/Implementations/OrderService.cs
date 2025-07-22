// DecalXeAPI/Services/Implementations/OrderService.cs
using DecalXeAPI.Data;
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.QueryParams;
using DecalXeAPI.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(ApplicationDbContext context, IMapper mapper, ILogger<OrderService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<(IEnumerable<OrderDto> Orders, int TotalCount)> GetOrdersAsync(OrderQueryParams queryParams)
        {
            _logger.LogInformation("Lấy danh sách đơn hàng với các tham số: {SearchTerm}, {Status}, {SortBy}, {SortOrder}, Page {PageNumber} Size {PageSize}",
                                    queryParams.SearchTerm, queryParams.Status, queryParams.SortBy, queryParams.SortOrder, queryParams.PageNumber, queryParams.PageSize);

            var query = _context.Orders
                                .Include(o => o.AssignedEmployee)
                                .Include(o => o.CustomerVehicle) // <-- BƯỚC 1: NẠP DỮ LIỆU XE
                                .AsQueryable();

            if (!string.IsNullOrEmpty(queryParams.Status))
            {
                query = query.Where(o => o.OrderStatus.ToLower() == queryParams.Status.ToLower());
            }

            if (!string.IsNullOrEmpty(queryParams.SearchTerm))
            {
                var searchTermLower = queryParams.SearchTerm.ToLower();
                // BƯỚC 2: CẬP NHẬT LẠI TOÀN BỘ LOGIC TÌM KIẾM (REMOVED CUSTOMER SEARCH)
                query = query.Where(o =>
                    (o.AssignedEmployee != null && (o.AssignedEmployee.FirstName + " " + o.AssignedEmployee.LastName).ToLower().Contains(searchTermLower)) ||
                    (o.CustomerVehicle != null && o.CustomerVehicle.ChassisNumber.ToLower().Contains(searchTermLower)) // <-- Sửa từ LicensePlate thành ChassisNumber
                );
            }

            if (!string.IsNullOrEmpty(queryParams.SortBy))
            {
                switch (queryParams.SortBy.ToLower())
                {
                    case "orderdate":
                        query = queryParams.SortOrder.ToLower() == "desc" ? query.OrderByDescending(o => o.OrderDate) : query.OrderBy(o => o.OrderDate);
                        break;
                    case "totalamount":
                        query = queryParams.SortOrder.ToLower() == "desc" ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount);
                        break;
                    // Removed customername sorting as Customer is disconnected
                    case "orderstatus":
                        query = queryParams.SortOrder.ToLower() == "desc" ? query.OrderByDescending(o => o.OrderStatus) : query.OrderBy(o => o.OrderStatus);
                        break;
                    default:
                        query = query.OrderBy(o => o.OrderDate);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(o => o.OrderDate);
            }

            var totalCount = await query.CountAsync();
            var orders = await query
                                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                                .Take(queryParams.PageSize)
                                .ToListAsync();

            var orderDtos = _mapper.Map<List<OrderDto>>(orders);

            _logger.LogInformation("Đã trả về {Count} đơn hàng (tổng cộng {TotalCount}).", orderDtos.Count, totalCount);
            return (orderDtos, totalCount);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy thông tin đơn hàng với ID: {OrderID}", id);
            var order = await _context.Orders
                                    .Include(o => o.AssignedEmployee)
                                    .Include(o => o.CustomerVehicle) // <-- Thêm Include ở đây nữa cho chắc ăn
                                        .ThenInclude(cv => cv.VehicleModel)
                                            .ThenInclude(vm => vm.VehicleBrand)
                                    .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null)
            {
                _logger.LogWarning("Không tìm thấy đơn hàng với ID: {OrderID}", id);
                return null;
            }

            var orderDto = _mapper.Map<OrderDto>(order);
            return orderDto;
        }

        public async Task<OrderDto> CreateOrderAsync(Order order)
        {
            _logger.LogInformation("Yêu cầu tạo đơn hàng mới");




           

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã tạo Order mới với ID: {OrderID}", order.OrderID);

            // Tải lại các thực thể liên quan để AutoMapper có thể ánh xạ đầy đủ
            await _context.Entry(order).Reference(o => o.AssignedEmployee).LoadAsync();
            await _context.Entry(order).Reference(o => o.CustomerVehicle).LoadAsync();

            var orderDto = _mapper.Map<OrderDto>(order);
            return orderDto;
        }

        // ... (Các phương thức còn lại không thay đổi)
        public async Task<bool> UpdateOrderAsync(string id, Order order)
        {
            _logger.LogInformation("Yêu cầu cập nhật đơn hàng với ID: {OrderID}", id);
            if (id != order.OrderID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với OrderID trong body ({OrderIDBody})", id, order.OrderID);
                return false;
            }

            if (!await OrderExistsAsync(id))
            {
                _logger.LogWarning("Không tìm thấy đơn hàng để cập nhật với ID: {OrderID}", id);
                return false;
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật đơn hàng với ID: {OrderID}", order.OrderID);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật đơn hàng với ID: {OrderID}", id);
                throw;
            }
        }

        public async Task<bool> DeleteOrderAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa đơn hàng với ID: {OrderID}", id);
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Không tìm thấy đơn hàng để xóa với ID: {OrderID}", id);
                return false;
            }
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa đơn hàng với ID: {OrderID}", id);
            return true;
        }
        public async Task<bool> UpdateOrderStatusAsync(string id, string newStatus)
        {
            _logger.LogInformation("Yêu cầu cập nhật trạng thái đơn hàng {OrderID} thành {NewStatus}", id, newStatus);
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Không tìm thấy đơn hàng để cập nhật trạng thái với ID: {OrderID}", id);
                return false;
            }

            if (string.IsNullOrEmpty(newStatus))
            {
                _logger.LogWarning("Trạng thái mới rỗng cho OrderID: {OrderID}", id);
                return false;
            }

            order.OrderStatus = newStatus;
            _context.Orders.Update(order);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật trạng thái đơn hàng {OrderID} thành {NewStatus} thành công.", id, newStatus);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật trạng thái đơn hàng {OrderID}", id);
                throw;
            }
        }

        public async Task<IEnumerable<SalesStatisticsDto>> GetSalesStatisticsAsync(DateTime? startDate, DateTime? endDate)
        {
            _logger.LogInformation("Yêu cầu thống kê doanh thu từ {StartDate} đến {EndDate}", startDate, endDate);
            var query = _context.Orders.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(o => o.OrderDate < endDate.Value.AddDays(1));
            }

            var dailySales = await query
              .GroupBy(o => o.OrderDate.Date)
              .Select(g => new SalesStatisticsDto
              {
                  Date = g.Key,
                  TotalSalesAmount = g.Sum(o => o.TotalAmount),
                  TotalOrders = g.Count()
              })
              .OrderBy(s => s.Date)
              .ToListAsync();

            _logger.LogInformation("Đã trả về {Count} bản ghi thống kê doanh thu.", dailySales.Count);
            return dailySales;
        }

        public async Task<bool> OrderExistsAsync(string id)
        {
            return await _context.Orders.AnyAsync(e => e.OrderID == id);
        }

        public async Task<(bool Success, string? ErrorMessage)> AssignEmployeeToOrderAsync(string orderId, string employeeId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return (false, "Không tìm thấy đơn hàng.");
            }

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                return (false, "Không tìm thấy nhân viên.");
            }

            order.AssignedEmployeeID = employeeId;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã gán nhân viên {EmployeeId} cho đơn hàng {OrderId}", employeeId, orderId);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UnassignEmployeeFromOrderAsync(string orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return (false, "Không tìm thấy đơn hàng.");
            }

            if (string.IsNullOrEmpty(order.AssignedEmployeeID))
            {
                return (false, "Đơn hàng này chưa được gán cho nhân viên nào.");
            }

            order.AssignedEmployeeID = null;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã hủy gán nhân viên khỏi đơn hàng {OrderId}", orderId);
            return (true, null);
        }

        public async Task<EmployeeDto?> GetAssignedEmployeeForOrderAsync(string orderId)
        {
            var order = await _context.Orders
                                    .Include(o => o.AssignedEmployee)
                                        .ThenInclude(e => e.Account)
                                            .ThenInclude(a => a.Role) // Lấy luôn thông tin Role
                                    .Include(o => o.AssignedEmployee)
                                        .ThenInclude(e => e.Store) // Lấy thông tin Store
                                    .FirstOrDefaultAsync(o => o.OrderID == orderId);

            if (order == null || order.AssignedEmployee == null)
            {
                return null;
            }

            return _mapper.Map<EmployeeDto>(order.AssignedEmployee);
        }
    }
    

}
using DecalXeAPI.Data;
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.QueryParams;
using DecalXeAPI.Services.Interfaces; // Để sử dụng IOrderService
using AutoMapper; // Để ánh xạ DTOs
using Microsoft.EntityFrameworkCore; // Để tương tác với DB
using Microsoft.Extensions.Logging; // Để ghi log
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Implementations
{
    public class OrderService : IOrderService // <-- Kế thừa từ IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger; // Logger cho Service

        public OrderService(ApplicationDbContext context, IMapper mapper, ILogger<OrderService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // Lấy danh sách Order có tìm kiếm, lọc, sắp xếp, phân trang
        public async Task<(IEnumerable<OrderDto> Orders, int TotalCount)> GetOrdersAsync(OrderQueryParams queryParams)
        {
            _logger.LogInformation("Lấy danh sách đơn hàng với các tham số: {SearchTerm}, {Status}, {SortBy}, {SortOrder}, Page {PageNumber} Size {PageSize}",
                                    queryParams.SearchTerm, queryParams.Status, queryParams.SortBy, queryParams.SortOrder, queryParams.PageNumber, queryParams.PageSize);

            var query = _context.Orders
                                .Include(o => o.Customer)
                                .Include(o => o.AssignedEmployee)
                                .Include(o => o.CustomServiceRequest)
                                .AsQueryable();

            // Lọc (Filtering)
            if (!string.IsNullOrEmpty(queryParams.Status))
            {
                query = query.Where(o => o.OrderStatus.ToLower() == queryParams.Status.ToLower());
            }

            // Tìm kiếm (Searching)
            if (!string.IsNullOrEmpty(queryParams.SearchTerm))
            {
                query = query.Where(o =>
                    o.Customer.FirstName.ToLower().Contains(queryParams.SearchTerm.ToLower()) ||
                    o.Customer.LastName.ToLower().Contains(queryParams.SearchTerm.ToLower()) ||
                   (o.AssignedEmployee != null && (o.AssignedEmployee.FirstName.ToLower().Contains(queryParams.SearchTerm.ToLower()) ||
o.AssignedEmployee.LastName.ToLower().Contains(queryParams.SearchTerm.ToLower())))||
                    (o.CustomServiceRequest != null && o.CustomServiceRequest.Description.ToLower().Contains(queryParams.SearchTerm.ToLower()))
                );
            }

            // Sắp xếp (Sorting)
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
                    case "customername":
                        query = queryParams.SortOrder.ToLower() == "desc" ? query.OrderByDescending(o => o.Customer.LastName).ThenByDescending(o => o.Customer.FirstName) : query.OrderBy(o => o.Customer.LastName).ThenBy(o => o.Customer.FirstName);
                        break;
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
                query = query.OrderBy(o => o.OrderDate); // Sắp xếp mặc định
            }

            var totalCount = await query.CountAsync(); // Tổng số bản ghi trước khi phân trang
            var orders = await query
                                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                                .Take(queryParams.PageSize)
                                .ToListAsync();

            var orderDtos = _mapper.Map<List<OrderDto>>(orders);

            _logger.LogInformation("Đã trả về {Count} đơn hàng (tổng cộng {TotalCount}).", orderDtos.Count, totalCount);
            return (orderDtos, totalCount); // Trả về tuple
        }

        // Lấy đơn hàng theo ID
        public async Task<OrderDto?> GetOrderByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy thông tin đơn hàng với ID: {OrderID}", id);
            var order = await _context.Orders
                                    .Include(o => o.Customer)
                                    .Include(o => o.AssignedEmployee)
                                    .Include(o => o.CustomServiceRequest)
                                    .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null)
            {
                _logger.LogWarning("Không tìm thấy đơn hàng với ID: {OrderID}", id);
                return null; // Trả về null nếu không tìm thấy
            }

            var orderDto = _mapper.Map<OrderDto>(order);
            _logger.LogInformation("Đã trả về đơn hàng với ID: {OrderID}", id);
            return orderDto;
        }

        // Tạo đơn hàng mới
        public async Task<OrderDto> CreateOrderAsync(Order order)
        {
            _logger.LogInformation("Yêu cầu tạo đơn hàng mới cho CustomerID: {CustomerID}", order.CustomerID);

            // Kiểm tra tồn tại FKs (có thể để Controller kiểm tra hoặc Service kiểm tra)
            // Hiện tại, Controller sẽ vẫn kiểm tra một số FKs chính
            // Nhưng Service cũng có thể kiểm tra lại nếu muốn độc lập hoàn toàn

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã tạo Order mới với ID: {OrderID}", order.OrderID);

            // Cập nhật OrderID cho CustomServiceRequest nếu có
            if (!string.IsNullOrEmpty(order.CustomServiceRequest?.CustomRequestID))
            {
                var csr = await _context.CustomServiceRequests.FindAsync(order.CustomServiceRequest.CustomRequestID);
                if (csr != null)
                {
                    csr.OrderID = order.OrderID;
                    _context.CustomServiceRequests.Update(csr);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Đã liên kết CustomServiceRequest {CustomRequestID} với Order {OrderID}", csr.CustomRequestID, order.OrderID);
                }
            }


            // Tự động gán IsCustomDecal nếu Order được liên kết với CustomServiceRequest
            if (order.CustomServiceRequest != null)
            {
                order.IsCustomDecal = true;
            }
            else
            {
                order.IsCustomDecal = false; // Mặc định là false nếu không có CSR
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
                        _logger.LogInformation("Đã tạo Order mới với ID: {OrderID}", order.OrderID);

            await _context.Entry(order).Reference(o => o.Customer).LoadAsync();
            await _context.Entry(order).Reference(o => o.AssignedEmployee).LoadAsync();
            await _context.Entry(order).Reference(o => o.CustomServiceRequest).LoadAsync();

            var orderDto = _mapper.Map<OrderDto>(order);
            return orderDto;
        }

        // Cập nhật đơn hàng
        public async Task<bool> UpdateOrderAsync(string id, Order order)
        {
            _logger.LogInformation("Yêu cầu cập nhật đơn hàng với ID: {OrderID}", id);
            if (id != order.OrderID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với OrderID trong body ({OrderIDBody})", id, order.OrderID);
                return false; // Trả về false nếu ID không khớp
            }

            // Kiểm tra tồn tại Order trước
            if (!await OrderExistsAsync(id))
            {
                _logger.LogWarning("Không tìm thấy đơn hàng để cập nhật với ID: {OrderID}", id);
                return false; // Trả về false nếu Order không tồn tại
            }

            // Kiểm tra FKs (có thể để Controller kiểm tra hoặc Service kiểm tra)

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật đơn hàng với ID: {OrderID}", order.OrderID);

                if (!string.IsNullOrEmpty(order.CustomServiceRequest?.CustomRequestID))
                {
                    var csr = await _context.CustomServiceRequests.FindAsync(order.CustomServiceRequest.CustomRequestID);
                    if (csr != null && csr.OrderID != order.OrderID)
                    {
                        csr.OrderID = order.OrderID;
                        _context.CustomServiceRequests.Update(csr);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Đã cập nhật liên kết CustomServiceRequest {CustomRequestID} với Order {OrderID}", csr.CustomRequestID, order.OrderID);
                    }
                }
                return true; // Cập nhật thành công
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật đơn hàng với ID: {OrderID}", id);
                // Lỗi xung đột thường được xử lý ở Controller để trả về đúng loại lỗi HTTP
                throw; // Ném lại để Controller bắt và xử lý
            }
        }

        // Xóa đơn hàng
        public async Task<bool> DeleteOrderAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa đơn hàng với ID: {OrderID}", id);
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Không tìm thấy đơn hàng để xóa với ID: {OrderID}", id);
                return false; // Trả về false nếu không tìm thấy
            }

            // Ngắt liên kết với CustomServiceRequest trước khi xóa Order
            var csr = await _context.CustomServiceRequests.FirstOrDefaultAsync(c => c.OrderID == id);
            if (csr != null)
            {
                csr.OrderID = null;
                _context.CustomServiceRequests.Update(csr);
                _logger.LogInformation("Đã ngắt liên kết CustomServiceRequest {CustomRequestID} khỏi Order {OrderID}", csr.CustomRequestID, id);
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa đơn hàng với ID: {OrderID}", id);
            return true; // Xóa thành công
        }

        // Cập nhật trạng thái đơn hàng (tách ra API riêng trong Controller)
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
                // Không ném exception, trả về false để Controller báo BadRequest
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
                throw; // Ném lại để Controller xử lý
            }
        }

        // Lấy thống kê doanh thu
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

        // Hàm hỗ trợ: Kiểm tra sự tồn tại của Order (Dùng nội bộ trong Service)
        public async Task<bool> OrderExistsAsync(string id)
        {
            return await _context.Orders.AnyAsync(e => e.OrderID == id);
        }
    }
}
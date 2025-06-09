using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using AutoMapper;
using System.Collections.Generic;
using System;
using System.Linq;
using DecalXeAPI.QueryParams;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging; // <-- THÊM DÒNG NÀY

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<OrdersController> _logger; // <-- KHAI BÁO BIẾN ILogger

        public OrdersController(ApplicationDbContext context, IMapper mapper, ILogger<OrdersController> logger) // <-- TIÊM ILogger VÀO CONSTRUCTOR
        {
            _context = context;
            _mapper = mapper;
            _logger = logger; // <-- GÁN GIÁ TRỊ
        }

        // API: GET api/Orders
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Sales,Technician,Customer")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders([FromQuery] OrderQueryParams queryParams)
        {
            _logger.LogInformation("Lấy danh sách đơn hàng với các tham số: {SearchTerm}, {Status}, {SortBy}, {SortOrder}, Page {PageNumber} Size {PageSize}",
                                    queryParams.SearchTerm, queryParams.Status, queryParams.SortBy, queryParams.SortOrder, queryParams.PageNumber, queryParams.PageSize);

            var query = _context.Orders
                                .Include(o => o.Customer)
                                .Include(o => o.AssignedEmployee)
                                .Include(o => o.CustomServiceRequest)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(queryParams.Status))
            {
                query = query.Where(o => o.OrderStatus.ToLower() == queryParams.Status.ToLower());
            }

            if (!string.IsNullOrEmpty(queryParams.SearchTerm))
            {
                query = query.Where(o =>
                    o.Customer.FirstName.ToLower().Contains(queryParams.SearchTerm.ToLower()) ||
                    o.Customer.LastName.ToLower().Contains(queryParams.SearchTerm.ToLower()) ||
                    (o.AssignedEmployee != null && (o.AssignedEmployee.FirstName.ToLower().Contains(queryParams.SearchTerm.ToLower()) ||
                    o.AssignedEmployee.LastName.ToLower().Contains(queryParams.SearchTerm.ToLower()))) ||
                    (o.CustomServiceRequest != null && o.CustomServiceRequest.Description.ToLower().Contains(queryParams.SearchTerm.ToLower()))
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
                query = query.OrderBy(o => o.OrderDate);
            }

            var totalCount = await query.CountAsync();
            var orders = await query
                                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                                .Take(queryParams.PageSize)
                                .ToListAsync();

            var orderDtos = _mapper.Map<List<OrderDto>>(orders);

            Response.Headers.Append("X-Total-Count", totalCount.ToString());
            Response.Headers.Append("X-Page-Number", queryParams.PageNumber.ToString());
            Response.Headers.Append("X-Page-Size", queryParams.PageSize.ToString());
            Response.Headers.Append("X-Total-Pages", ((int)Math.Ceiling((double)totalCount / queryParams.PageSize)).ToString());

            _logger.LogInformation("Đã trả về {Count} đơn hàng (tổng cộng {TotalCount}).", orderDtos.Count, totalCount);
            return Ok(orderDtos);
        }

        // API: GET api/Orders/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Sales,Technician,Customer")]
        public async Task<ActionResult<OrderDto>> GetOrder(string id)
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
                return NotFound();
            }

            var orderDto = _mapper.Map<OrderDto>(order);
            _logger.LogInformation("Đã trả về đơn hàng với ID: {OrderID}", id);
            return Ok(orderDto);
        }

        // API: POST api/Orders
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Sales")]
        public async Task<ActionResult<OrderDto>> PostOrder(Order order)
        {
            _logger.LogInformation("Yêu cầu tạo đơn hàng mới cho CustomerID: {CustomerID}", order.CustomerID);

            if (!string.IsNullOrEmpty(order.CustomerID) && !CustomerExists(order.CustomerID))
            {
                _logger.LogWarning("CustomerID không tồn tại: {CustomerID}", order.CustomerID);
                return BadRequest("CustomerID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(order.AssignedEmployeeID) && !EmployeeExists(order.AssignedEmployeeID))
            {
                _logger.LogWarning("AssignedEmployeeID không tồn tại: {EmployeeID}", order.AssignedEmployeeID);
                return BadRequest("AssignedEmployeeID không tồn tại.");
            }

            if (!string.IsNullOrEmpty(order.CustomServiceRequest?.CustomRequestID) && !CustomServiceRequestExists(order.CustomServiceRequest.CustomRequestID))
            {
                _logger.LogWarning("CustomServiceRequestID không tồn tại: {CustomRequestID}", order.CustomServiceRequest.CustomRequestID);
                return BadRequest("CustomServiceRequestID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(order.CustomServiceRequest?.CustomRequestID))
            {
                var existingCsr = await _context.CustomServiceRequests
                                            .Where(csr => csr.CustomRequestID == order.CustomServiceRequest.CustomRequestID && csr.OrderID != null)
                                            .FirstOrDefaultAsync();
                if (existingCsr != null)
                {
                    _logger.LogWarning("CustomServiceRequest đã được liên kết với Order khác: {CustomRequestID}", order.CustomServiceRequest.CustomRequestID);
                    return BadRequest("CustomServiceRequest này đã được liên kết với một Order khác.");
                }
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã tạo Order mới với ID: {OrderID}", order.OrderID);

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

            await _context.Entry(order).Reference(o => o.Customer).LoadAsync();
            await _context.Entry(order).Reference(o => o.AssignedEmployee).LoadAsync();
            await _context.Entry(order).Reference(o => o.CustomServiceRequest).LoadAsync();

            var orderDto = _mapper.Map<OrderDto>(order);
            return CreatedAtAction(nameof(GetOrder), new { id = orderDto.OrderID }, orderDto);
        }

        // API: PUT api/Orders/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Sales")]
        public async Task<IActionResult> PutOrder(string id, Order order)
        {
            _logger.LogInformation("Yêu cầu cập nhật đơn hàng với ID: {OrderID}", id);
            if (id != order.OrderID)
            {
                _logger.LogWarning("ID trong URL ({Id}) không khớp với OrderID trong body ({OrderIDBody})", id, order.OrderID);
                return BadRequest();
            }

            if (!string.IsNullOrEmpty(order.CustomerID) && !CustomerExists(order.CustomerID))
            {
                _logger.LogWarning("CustomerID không tồn tại: {CustomerID}", order.CustomerID);
                return BadRequest("CustomerID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(order.AssignedEmployeeID) && !EmployeeExists(order.AssignedEmployeeID))
            {
                _logger.LogWarning("AssignedEmployeeID không tồn tại: {EmployeeID}", order.AssignedEmployeeID);
                return BadRequest("AssignedEmployeeID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(order.CustomServiceRequest?.CustomRequestID) && !CustomServiceRequestExists(order.CustomServiceRequest.CustomRequestID))
            {
                _logger.LogWarning("CustomServiceRequestID không tồn tại: {CustomRequestID}", order.CustomServiceRequest.CustomRequestID);
                return BadRequest("CustomServiceRequestID không tồn tại.");
            }

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

            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật đơn hàng với ID: {OrderID}", id);
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // API: DELETE api/Orders/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteOrder(string id)
        {
            _logger.LogInformation("Yêu cầu xóa đơn hàng với ID: {OrderID}", id);
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Không tìm thấy đơn hàng để xóa với ID: {OrderID}", id);
                return NotFound();
            }

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

            return NoContent();
        }

        // API: PUT api/Orders/{id}/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] string newStatus)
        {
            _logger.LogInformation("Yêu cầu cập nhật trạng thái đơn hàng {OrderID} thành {NewStatus}", id, newStatus);
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Không tìm thấy đơn hàng để cập nhật trạng thái với ID: {OrderID}", id);
                return NotFound();
            }

            if (string.IsNullOrEmpty(newStatus))
            {
                _logger.LogWarning("Trạng thái mới rỗng cho OrderID: {OrderID}", id);
                return BadRequest("Trạng thái mới không được rỗng.");
            }

            order.OrderStatus = newStatus;
            _context.Orders.Update(order);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật trạng thái đơn hàng {OrderID} thành {NewStatus} thành công.", id, newStatus);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật trạng thái đơn hàng {OrderID}", id);
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // API Thống kê Doanh thu
        [HttpGet("statistics/sales")]
        [Authorize(Roles = "Admin,Manager,Sales")]
        public async Task<ActionResult<IEnumerable<SalesStatisticsDto>>> GetSalesStatistics(
            [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
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
            return Ok(dailySales);
        }

        // --- HÀM HỖ TRỢ: KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG ---
        private bool OrderExists(string id) { return _context.Orders.Any(e => e.OrderID == id); }
        private bool CustomerExists(string id) { return _context.Customers.Any(e => e.CustomerID == id); }
        private bool EmployeeExists(string id) { return _context.Employees.Any(e => e.EmployeeID == id); }
        private bool CustomServiceRequestExists(string id) { return _context.CustomServiceRequests.Any(e => e.CustomRequestID == id); }
    }
}
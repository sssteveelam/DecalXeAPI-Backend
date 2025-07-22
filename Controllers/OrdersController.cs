using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data; // Vẫn cần DbContext để dùng các hàm hỗ trợ Exists
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.QueryParams;
using DecalXeAPI.Services.Interfaces; // <-- THÊM DÒNG NÀY (Để sử dụng IOrderService)
using AutoMapper; // Vẫn cần AutoMapper để ánh xạ các DTO input/output nếu có
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists đơn giản
        private readonly IOrderService _orderService; // <-- KHAI BÁO BIẾN CHO SERVICE
        private readonly IMapper _mapper; // Vẫn giữ để ánh xạ DTOs nếu có

        public OrdersController(ApplicationDbContext context, IOrderService orderService, IMapper mapper) // <-- TIÊM IOrderService
        {
            _context = context; // Để dùng các hàm hỗ trợ
            _orderService = orderService; // Gán Service
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Sales,Technician,Customer")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders([FromQuery] OrderQueryParams queryParams)
        {
            var (orders, totalCount) = await _orderService.GetOrdersAsync(queryParams);

            Response.Headers.Append("X-Total-Count", totalCount.ToString());
            Response.Headers.Append("X-Page-Number", queryParams.PageNumber.ToString());
            Response.Headers.Append("X-Page-Size", queryParams.PageSize.ToString());
            Response.Headers.Append("X-Total-Pages", ((int)Math.Ceiling((double)totalCount / queryParams.PageSize)).ToString());

            return Ok(orders);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Sales,Technician,Customer")]
        public async Task<ActionResult<OrderDto>> GetOrder(string id)
        {
            var orderDto = await _orderService.GetOrderByIdAsync(id);
            if (orderDto == null) return NotFound();
            return Ok(orderDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Sales")]
        public async Task<ActionResult<OrderDto>> PostOrder(CreateOrderDto createDto)
        {
            if (!string.IsNullOrEmpty(createDto.VehicleID) && !VehicleExists(createDto.VehicleID))
            {
                return BadRequest("VehicleID không tồn tại.");
            }

            var order = _mapper.Map<Order>(createDto);
            order.OrderStatus = "New";
            order.CurrentStage = "New Profile";

            var orderDto = await _orderService.CreateOrderAsync(order);
            return CreatedAtAction(nameof(GetOrder), new { id = orderDto.OrderID }, orderDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Sales")]
        public async Task<IActionResult> PutOrder(string id, UpdateOrderDto updateDto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            _mapper.Map(updateDto, order);

            var success = await _orderService.UpdateOrderAsync(id, order);
            if (!success) return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteOrder(string id)
        {
            var success = await _orderService.DeleteOrderAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] string newStatus)
        {
            var success = await _orderService.UpdateOrderStatusAsync(id, newStatus);
            if (!success && !await _orderService.OrderExistsAsync(id)) return NotFound();
            if (!success) return BadRequest("Trạng thái mới không được rỗng.");
            return NoContent();
        }

        // API Thống kê Doanh thu
        [HttpGet("statistics/sales")]
        [Authorize(Roles = "Admin,Manager,Sales")]
        public async Task<ActionResult<IEnumerable<SalesStatisticsDto>>> GetSalesStatistics(
            [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            // Ủy quyền logic thống kê cho Service Layer
            var salesData = await _orderService.GetSalesStatisticsAsync(startDate, endDate);
            return Ok(salesData);
        }

        /// <summary>
        /// API để gán một nhân viên vào đơn hàng.
        /// </summary>
        /// <param name="orderId">ID của đơn hàng.</param>
        /// <param name="employeeId">ID của nhân viên được gán.</param>
        [HttpPut("{orderId}/assign/{employeeId}")]
        [Authorize(Roles = "Admin,Manager,Sales")] // Chỉ định vai trò được phép
        public async Task<IActionResult> AssignEmployee(string orderId, string employeeId)
        {
            var (success, errorMessage) = await _orderService.AssignEmployeeToOrderAsync(orderId, employeeId);
            if (!success)
            {
                // Trả về lỗi 400 Bad Request kèm thông báo lỗi cụ thể từ Service
                return BadRequest(new { message = errorMessage });
            }
            return NoContent(); // Thành công, trả về 204 No Content
        }

        /// <summary>
        /// API để hủy gán nhân viên khỏi đơn hàng.
        /// </summary>
        /// <param name="orderId">ID của đơn hàng.</param>
        [HttpDelete("{orderId}/assign")]
        [Authorize(Roles = "Admin,Manager,Sales")] // Chỉ định vai trò được phép
        public async Task<IActionResult> UnassignEmployee(string orderId)
        {
            var (success, errorMessage) = await _orderService.UnassignEmployeeFromOrderAsync(orderId);
            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }
            return NoContent();
        }

        /// <summary>
        /// API để xem thông tin nhân viên đã được gán cho một đơn hàng.
        /// </summary>
        /// <param name="orderId">ID của đơn hàng.</param>
        [HttpGet("{orderId}/assigned-employee")]
        public async Task<ActionResult<EmployeeDto>> GetAssignedEmployee(string orderId)
        {
            var employee = await _orderService.GetAssignedEmployeeForOrderAsync(orderId);
            if (employee == null)
            {
                return NotFound(new { message = "Đơn hàng này không tồn tại hoặc chưa được gán cho nhân viên nào." });
            }
            return Ok(employee);
        }


        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG ---
        // Các hàm này vẫn được giữ ở Controller để kiểm tra FKs trước khi gọi Service

        // Removed CustomerExists method as Customer table is disconnected
        private bool VehicleExists(string id) { return _context.CustomerVehicles.Any(e => e.VehicleID == id); }

    }
}
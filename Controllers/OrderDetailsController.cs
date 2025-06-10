using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Vẫn cần để dùng các hàm Exists đơn giản
using DecalXeAPI.Data; // Vẫn cần ApplicationDbContext cho các hàm Exists
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // <-- THÊM DÒNG NÀY (Để sử dụng IOrderDetailService)
using AutoMapper;
using System.Collections.Generic;
using System.Linq; // Để dùng Any() trong hàm Exists
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging; // Vẫn cần để ghi log Controller

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderDetailsController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists
        private readonly IOrderDetailService _orderDetailService; // <-- KHAI BÁO BIẾN CHO SERVICE
        private readonly IMapper _mapper; // Vẫn giữ để ánh xạ DTOs nếu có
        private readonly ILogger<OrderDetailsController> _logger; // Logger cho Controller

        public OrderDetailsController(ApplicationDbContext context, IOrderDetailService orderDetailService, IMapper mapper, ILogger<OrderDetailsController> logger) // <-- TIÊM IOrderDetailService
        {
            _context = context; // Để dùng các hàm hỗ trợ
            _orderDetailService = orderDetailService; // Gán Service
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/OrderDetails
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Sales,Technician")]
        public async Task<ActionResult<IEnumerable<OrderDetailDto>>> GetOrderDetails()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách chi tiết đơn hàng.");
            // Ủy quyền logic cho Service Layer
            var orderDetails = await _orderDetailService.GetOrderDetailsAsync();
            return Ok(orderDetails);
        }

        // API: GET api/OrderDetails/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Sales,Technician")]
        public async Task<ActionResult<OrderDetailDto>> GetOrderDetail(string id)
        {
            _logger.LogInformation("Yêu cầu lấy chi tiết đơn hàng với ID: {OrderDetailID}", id);
            // Ủy quyền logic cho Service Layer
            var orderDetailDto = await _orderDetailService.GetOrderDetailByIdAsync(id);

            if (orderDetailDto == null)
            {
                _logger.LogWarning("Không tìm thấy chi tiết đơn hàng với ID: {OrderDetailID}", id);
                return NotFound();
            }

            return Ok(orderDetailDto);
        }

        // API: POST api/OrderDetails
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Sales")]
        public async Task<ActionResult<OrderDetailDto>> PostOrderDetail(OrderDetail orderDetail) // Vẫn nhận OrderDetail Model
        {
            _logger.LogInformation("Yêu cầu tạo chi tiết đơn hàng mới cho OrderID: {OrderID}, ServiceID: {ServiceID}", orderDetail.OrderID, orderDetail.ServiceID);

            // --- KIỂM TRA FKs CHÍNH TRƯỚC KHI GỬI VÀO SERVICE ---
            // Controller sẽ chịu trách nhiệm validate các FKs chính
            if (!string.IsNullOrEmpty(orderDetail.OrderID) && !OrderExists(orderDetail.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(orderDetail.ServiceID) && !DecalServiceExists(orderDetail.ServiceID))
            {
                return BadRequest("ServiceID không tồn tại.");
            }

            // Ủy quyền logic tạo OrderDetail cho Service Layer
            var (createdOrderDetailDto, errorMessage) = await _orderDetailService.CreateOrderDetailAsync(orderDetail);

            if (createdOrderDetailDto == null)
            {
                _logger.LogError("Lỗi khi tạo chi tiết đơn hàng: {ErrorMessage}", errorMessage);
                return BadRequest(errorMessage); // Trả về lỗi từ Service
            }

            _logger.LogInformation("Đã tạo chi tiết đơn hàng mới với ID: {OrderDetailID}", createdOrderDetailDto.OrderDetailID);
            return CreatedAtAction(nameof(GetOrderDetail), new { id = createdOrderDetailDto.OrderDetailID }, createdOrderDetailDto);
        }

        // API: PUT api/OrderDetails/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Sales")]
        public async Task<IActionResult> PutOrderDetail(string id, OrderDetail orderDetail)
        {
            _logger.LogInformation("Yêu cầu cập nhật chi tiết đơn hàng với ID: {OrderDetailID}", id);

            // Kiểm tra FKs chính
            if (!string.IsNullOrEmpty(orderDetail.OrderID) && !OrderExists(orderDetail.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(orderDetail.ServiceID) && !DecalServiceExists(orderDetail.ServiceID))
            {
                return BadRequest("ServiceID không tồn tại.");
            }

            // Ủy quyền logic cập nhật OrderDetail cho Service Layer
            var (success, errorMessage) = await _orderDetailService.UpdateOrderDetailAsync(id, orderDetail);

            if (!success)
            {
                if (errorMessage == "Chi tiết đơn hàng không tồn tại.")
                {
                    _logger.LogWarning("Không tìm thấy chi tiết đơn hàng để cập nhật với ID: {OrderDetailID}", id);
                    return NotFound(errorMessage);
                }
                else
                {
                    _logger.LogError("Lỗi khi cập nhật chi tiết đơn hàng: {ErrorMessage}", errorMessage);
                    return BadRequest(errorMessage); // Trả về lỗi từ Service
                }
            }

            _logger.LogInformation("Đã cập nhật chi tiết đơn hàng với ID: {OrderDetailID}", id);
            return NoContent();
        }

        // API: DELETE api/OrderDetails/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager,Sales")]
        public async Task<IActionResult> DeleteOrderDetail(string id)
        {
            _logger.LogInformation("Yêu cầu xóa chi tiết đơn hàng với ID: {OrderDetailID}", id);

            // Ủy quyền logic xóa OrderDetail cho Service Layer
            var (success, errorMessage) = await _orderDetailService.DeleteOrderDetailAsync(id);

            if (!success)
            {
                _logger.LogWarning("Không tìm thấy chi tiết đơn hàng để xóa với ID: {OrderDetailID}", id);
                return NotFound(errorMessage); // Service trả về false nếu không tìm thấy
            }

            _logger.LogInformation("Đã xóa chi tiết đơn hàng với ID: {OrderDetailID}", id);
            return NoContent();
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG ---
        // Các hàm này vẫn được giữ ở Controller để kiểm tra FKs cơ bản trước khi gọi Service
        private bool OrderExists(string id) { return _context.Orders.Any(e => e.OrderID == id); }
        private bool DecalServiceExists(string id) { return _context.DecalServices.Any(e => e.ServiceID == id); }
    }
}
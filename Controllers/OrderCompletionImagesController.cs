using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Vẫn cần DbContext cho các hàm Exists cơ bản
using DecalXeAPI.Data; // Vẫn cần ApplicationDbContext cho các hàm Exists
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // Để sử dụng IOrderCompletionImageService
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System; // Để sử dụng ArgumentException

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Technician")] // Chỉ Admin, Manager, Technician mới có quyền quản lý ảnh hoàn tất
    public class OrderCompletionImagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists cơ bản
        private readonly IOrderCompletionImageService _orderCompletionImageService; // Khai báo biến cho Service
        private readonly IMapper _mapper;
        private readonly ILogger<OrderCompletionImagesController> _logger;

        public OrderCompletionImagesController(ApplicationDbContext context, IOrderCompletionImageService orderCompletionImageService, IMapper mapper, ILogger<OrderCompletionImagesController> logger)
        {
            _context = context;
            _orderCompletionImageService = orderCompletionImageService;
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/OrderCompletionImages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderCompletionImageDto>>> GetOrderCompletionImages()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách hình ảnh hoàn tất đơn hàng.");
            var images = await _orderCompletionImageService.GetOrderCompletionImagesAsync();
            return Ok(images);
        }

        // API: GET api/OrderCompletionImages/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderCompletionImageDto>> GetOrderCompletionImage(string id)
        {
            _logger.LogInformation("Yêu cầu lấy hình ảnh hoàn tất đơn hàng với ID: {ImageID}", id);
            var imageDto = await _orderCompletionImageService.GetOrderCompletionImageByIdAsync(id);

            if (imageDto == null)
            {
                _logger.LogWarning("Không tìm thấy hình ảnh hoàn tất đơn hàng với ID: {ImageID}", id);
                return NotFound();
            }

            return Ok(imageDto);
        }

        // API: POST api/OrderCompletionImages
        [HttpPost]
        public async Task<ActionResult<OrderCompletionImageDto>> PostOrderCompletionImage(OrderCompletionImage image) // Vẫn nhận Model
        {
            _logger.LogInformation("Yêu cầu tạo hình ảnh hoàn tất đơn hàng mới cho OrderID: {OrderID}", image.OrderID);

            // Kiểm tra OrderID (FK) trước khi gọi Service
            if (!string.IsNullOrEmpty(image.OrderID) && !OrderExists(image.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }

            try
            {
                var createdDto = await _orderCompletionImageService.CreateOrderCompletionImageAsync(image);
                _logger.LogInformation("Đã tạo hình ảnh hoàn tất đơn hàng mới với ID: {ImageID}", createdDto.ImageID);
                return CreatedAtAction(nameof(GetOrderCompletionImage), new { id = createdDto.ImageID }, createdDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi tạo hình ảnh hoàn tất đơn hàng: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/OrderCompletionImages/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrderCompletionImage(string id, OrderCompletionImage image)
        {
            _logger.LogInformation("Yêu cầu cập nhật hình ảnh hoàn tất đơn hàng với ID: {ImageID}", id);
            if (id != image.ImageID)
            {
                return BadRequest();
            }

            // Kiểm tra OrderID (FK) trước khi gọi Service
            if (!string.IsNullOrEmpty(image.OrderID) && !OrderExists(image.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }

            try
            {
                var success = await _orderCompletionImageService.UpdateOrderCompletionImageAsync(id, image);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy hình ảnh hoàn tất đơn hàng để cập nhật với ID: {ImageID}", id);
                    return NotFound();
                }

                _logger.LogInformation("Đã cập nhật hình ảnh hoàn tất đơn hàng với ID: {ImageID}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi cập nhật hình ảnh hoàn tất đơn hàng: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderCompletionImageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // API: DELETE api/OrderCompletionImages/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderCompletionImage(string id)
        {
            _logger.LogInformation("Yêu cầu xóa hình ảnh hoàn tất đơn hàng với ID: {ImageID}", id);
            var success = await _orderCompletionImageService.DeleteOrderCompletionImageAsync(id);

            if (!success)
            {
                _logger.LogWarning("Không tìm thấy hình ảnh hoàn tất đơn hàng để xóa với ID: {ImageID}", id);
                return NotFound();
            }

            return NoContent();
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool OrderCompletionImageExists(string id) { return _context.OrderCompletionImages.Any(e => e.ImageID == id); }
        private bool OrderExists(string id) { return _context.Orders.Any(e => e.OrderID == id); }
    }
}
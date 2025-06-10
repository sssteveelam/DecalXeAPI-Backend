using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // <-- THÊM DÒNG NÀY (Để sử dụng IFeedbackService)
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System; // Để sử dụng ArgumentException

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Quyền cho FeedbacksController
    public class FeedbacksController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists cơ bản
        private readonly IFeedbackService _feedbackService; // <-- KHAI BÁO BIẾN CHO SERVICE
        private readonly IMapper _mapper;
        private readonly ILogger<FeedbacksController> _logger;

        public FeedbacksController(ApplicationDbContext context, IFeedbackService feedbackService, IMapper mapper, ILogger<FeedbacksController> logger) // <-- TIÊM IFeedbackService
        {
            _context = context;
            _feedbackService = feedbackService;
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/Feedbacks
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Sales,Customer")] // Admin, Manager, Sales xem tất cả. Customer xem feedback của mình.
        public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetFeedbacks()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách phản hồi.");
            var feedbacks = await _feedbackService.GetFeedbacksAsync();
            return Ok(feedbacks);
        }

        // API: GET api/Feedbacks/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Sales,Customer")] // Admin, Manager, Sales xem tất cả. Customer xem feedback của mình.
        public async Task<ActionResult<FeedbackDto>> GetFeedback(string id)
        {
            _logger.LogInformation("Yêu cầu lấy phản hồi với ID: {FeedbackID}", id);
            var feedbackDto = await _feedbackService.GetFeedbackByIdAsync(id);

            if (feedbackDto == null)
            {
                _logger.LogWarning("Không tìm thấy phản hồi với ID: {FeedbackID}", id);
                return NotFound();
            }

            // Logic: Customer chỉ xem được feedback của chính mình (nếu muốn)
            // var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // if (User.IsInRole("Customer") && feedbackDto.CustomerID != currentUserId)
            // {
            //     return Forbid(); // Trả về 403 Forbidden nếu Customer cố gắng xem feedback của người khác
            // }

            return Ok(feedbackDto);
        }

        // API: POST api/Feedbacks
        [HttpPost]
        [Authorize(Roles = "Customer")] // Chỉ Customer được phép gửi feedback
        public async Task<ActionResult<FeedbackDto>> PostFeedback(Feedback feedback) // Vẫn nhận Model
        {
            _logger.LogInformation("Yêu cầu tạo phản hồi mới cho OrderID: {OrderID}, CustomerID: {CustomerID}", feedback.OrderID, feedback.CustomerID);

            // --- KIỂM TRA FKs CHÍNH TRƯỚC KHI GỬI VÀO SERVICE ---
            if (!string.IsNullOrEmpty(feedback.OrderID) && !OrderExists(feedback.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(feedback.CustomerID) && !CustomerExists(feedback.CustomerID))
            {
                return BadRequest("CustomerID không tồn tại.");
            }

            // Logic: Đảm bảo Customer chỉ gửi feedback cho chính mình (nếu muốn)
            // var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // if (User.IsInRole("Customer") && feedback.CustomerID != currentUserId)
            // {
            //     return Forbid("Bạn chỉ có thể gửi phản hồi cho chính mình.");
            // }


            try
            {
                var createdFeedbackDto = await _feedbackService.CreateFeedbackAsync(feedback);
                _logger.LogInformation("Đã tạo phản hồi mới với ID: {FeedbackID}", createdFeedbackDto.FeedbackID);
                return CreatedAtAction(nameof(GetFeedback), new { id = createdFeedbackDto.FeedbackID }, createdFeedbackDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi tạo phản hồi: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/Feedbacks/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")] // Chỉ Admin, Manager có thể cập nhật
        public async Task<IActionResult> PutFeedback(string id, Feedback feedback)
        {
            _logger.LogInformation("Yêu cầu cập nhật phản hồi với ID: {FeedbackID}", id);
            if (id != feedback.FeedbackID)
            {
                return BadRequest();
            }

            // Kiểm tra FKs chính
            if (!string.IsNullOrEmpty(feedback.OrderID) && !OrderExists(feedback.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(feedback.CustomerID) && !CustomerExists(feedback.CustomerID))
            {
                return BadRequest("CustomerID không tồn tại.");
            }

            try
            {
                var success = await _feedbackService.UpdateFeedbackAsync(id, feedback);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy phản hồi để cập nhật với ID: {FeedbackID}", id);
                    return NotFound();
                }

                _logger.LogInformation("Đã cập nhật phản hồi với ID: {FeedbackID}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi cập nhật phản hồi: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FeedbackExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // API: DELETE api/Feedbacks/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")] // Chỉ Admin, Manager có thể xóa
        public async Task<IActionResult> DeleteFeedback(string id)
        {
            _logger.LogInformation("Yêu cầu xóa phản hồi với ID: {FeedbackID}", id);
            var success = await _feedbackService.DeleteFeedbackAsync(id);

            if (!success)
            {
                _logger.LogWarning("Không tìm thấy phản hồi để xóa với ID: {FeedbackID}", id);
                return NotFound();
            }

            return NoContent();
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool FeedbackExists(string id) { return _context.Feedbacks.Any(e => e.FeedbackID == id); }
        private bool OrderExists(string id) { return _context.Orders.Any(e => e.OrderID == id); }
        private bool CustomerExists(string id) { return _context.Customers.Any(e => e.CustomerID == id); }
    }
}
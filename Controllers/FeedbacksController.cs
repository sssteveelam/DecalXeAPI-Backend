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

        // API: POST api/Feedbacks (ĐÃ NÂNG CẤP)
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<FeedbackDto>> PostFeedback(CreateFeedbackDto createDto)
        {
            _logger.LogInformation("Yêu cầu tạo phản hồi mới cho OrderID: {OrderID}, CustomerID: {CustomerID}", createDto.OrderID, createDto.CustomerID);

            // Logic: Đảm bảo Customer chỉ gửi feedback cho chính mình (nếu muốn)
            // var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountID == currentUserId);
            // if (customer == null || customer.CustomerID != createDto.CustomerID)
            // {
            //     return Forbid("Bạn chỉ có thể gửi phản hồi cho chính mình.");
            // }
            
            var feedback = _mapper.Map<Feedback>(createDto);

            try
            {
                var createdFeedbackDto = await _feedbackService.CreateFeedbackAsync(feedback);
                return CreatedAtAction(nameof(GetFeedback), new { id = createdFeedbackDto.FeedbackID }, createdFeedbackDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/Feedbacks/{id} (ĐÃ NÂNG CẤP)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> PutFeedback(string id, UpdateFeedbackDto updateDto)
        {
            _logger.LogInformation("Yêu cầu cập nhật phản hồi với ID: {FeedbackID}", id);
            
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }
            
            _mapper.Map(updateDto, feedback);

            try
            {
                var success = await _feedbackService.UpdateFeedbackAsync(id, feedback);
                if (!success) return NotFound();
                
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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
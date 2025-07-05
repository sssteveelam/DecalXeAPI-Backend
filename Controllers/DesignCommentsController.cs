using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Vẫn cần DbContext cho các hàm Exists cơ bản
using DecalXeAPI.Data; // Vẫn cần ApplicationDbContext cho các hàm Exists
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // Để sử dụng IDesignCommentService
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System; // Để sử dụng ArgumentException hoặc InvalidOperationException
using System.Security.Claims; // Để lấy thông tin User từ JWT

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Mặc định tất cả API trong Controller này đều cần xác thực
    public class DesignCommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists cơ bản
        private readonly IDesignCommentService _designCommentService; // Khai báo biến cho Service
        private readonly IMapper _mapper;
        private readonly ILogger<DesignCommentsController> _logger;

        public DesignCommentsController(ApplicationDbContext context, IDesignCommentService designCommentService, IMapper mapper, ILogger<DesignCommentsController> logger) // Tiêm IDesignCommentService
        {
            _context = context;
            _designCommentService = designCommentService;
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/DesignComments
        // Cho phép Admin, Manager, Designer, Sales xem các bình luận
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Designer,Sales")]
        public async Task<ActionResult<IEnumerable<DesignCommentDto>>> GetDesignComments()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách bình luận thiết kế.");
            var designComments = await _designCommentService.GetDesignCommentsAsync();
            return Ok(designComments);
        }

        // API: GET api/DesignComments/{id}
        // Cho phép Admin, Manager, Designer, Sales xem chi tiết bình luận
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Designer,Sales")]
        public async Task<ActionResult<DesignCommentDto>> GetDesignComment(string id)
        {
            _logger.LogInformation("Yêu cầu lấy bình luận thiết kế với ID: {CommentID}", id);
            var designCommentDto = await _designCommentService.GetDesignCommentByIdAsync(id);

            if (designCommentDto == null)
            {
                _logger.LogWarning("Không tìm thấy bình luận thiết kế với ID: {CommentID}", id);
                return NotFound();
            }

            return Ok(designCommentDto);
        }

        // API: POST /api/DesignComments (ĐÃ CHUẨN HÓA)
        [HttpPost]
        [Authorize(Roles = "Customer,Designer,Sales,Admin,Manager")]
        public async Task<ActionResult<DesignCommentDto>> PostDesignComment(CreateDesignCommentDto createDto)
        {
            var designComment = _mapper.Map<DesignComment>(createDto);

            _logger.LogInformation("Yêu cầu tạo bình luận mới cho DesignID: {DesignID}", designComment.DesignID);

            var currentAccountID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (User.IsInRole("Customer") && designComment.SenderAccountID != currentAccountID)
            {
                return Forbid("Bạn chỉ có thể gửi bình luận bằng tài khoản của chính mình.");
            }

            try
            {
                var createdDto = await _designCommentService.CreateDesignCommentAsync(designComment);
                return CreatedAtAction(nameof(GetDesignComment), new { id = createdDto.CommentID }, createdDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // API: PUT /api/DesignComments/{id} (ĐÃ CHUẨN HÓA)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Designer,Sales")]
        public async Task<IActionResult> PutDesignComment(string id, UpdateDesignCommentDto updateDto)
        {
            _logger.LogInformation("Yêu cầu cập nhật bình luận với ID: {CommentID}", id);

            var existingComment = await _context.DesignComments.FindAsync(id);
            if (existingComment == null)
            {
                return NotFound();
            }

            var currentAccountID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (existingComment.SenderAccountID != currentAccountID && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return Forbid("Bạn không có quyền sửa bình luận này.");
            }

            _mapper.Map(updateDto, existingComment);

            try
            {
                var success = await _designCommentService.UpdateDesignCommentAsync(id, existingComment);
                if (!success) return NotFound();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật bình luận {CommentID}", id);
                return StatusCode(500, "Đã xảy ra lỗi nội bộ.");
            }
        }

        // API: DELETE api/DesignComments/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")] // Chỉ Admin, Manager có thể xóa
        public async Task<IActionResult> DeleteDesignComment(string id)
        {
            _logger.LogInformation("Yêu cầu xóa bình luận thiết kế với ID: {CommentID}", id);
            try
            {
                var success = await _designCommentService.DeleteDesignCommentAsync(id);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy bình luận thiết kế để xóa với ID: {CommentID}", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex) // Bắt lỗi nghiệp vụ nếu có comment con
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi xóa bình luận thiết kế: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool DesignCommentExists(string id) { return _context.DesignComments.Any(e => e.CommentID == id); }
        private bool DesignExists(string id) { return _context.Designs.Any(e => e.DesignID == id); }
        private bool AccountExists(string id) { return _context.Accounts.Any(e => e.AccountID == id); }
    }
}
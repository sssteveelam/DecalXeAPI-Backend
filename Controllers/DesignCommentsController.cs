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

        // API: POST api/DesignComments
        // Customer, Designer, Sales (người liên quan đến Order/Design) có thể gửi comment
        [HttpPost]
        [Authorize(Roles = "Customer,Designer,Sales,Admin,Manager")] // Cho phép các role liên quan gửi comment
        public async Task<ActionResult<DesignCommentDto>> PostDesignComment(CreateDesignCommentDto designComment) // Vẫn nhận Model
        {
            _logger.LogInformation("Yêu cầu tạo bình luận thiết kế mới cho DesignID: {DesignID}, SenderAccountID: {SenderAccountID}",
                                    designComment.DesignID, designComment.SenderAccountID);

            // --- KIỂM TRA FKs CHÍNH TRƯỚC KHI GỬI VÀO SERVICE ---
            if (!string.IsNullOrEmpty(designComment.DesignID) && !DesignExists(designComment.DesignID))
            {
                return BadRequest("DesignID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(designComment.SenderAccountID) && !AccountExists(designComment.SenderAccountID))
            {
                return BadRequest("SenderAccountID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(designComment.ParentCommentID) && !DesignCommentExists(designComment.ParentCommentID))
            {
                return BadRequest("ParentCommentID không tồn tại.");
            }

            // Logic kiểm tra người gửi comment có phải chính tài khoản của họ không (nếu là Customer)
            var currentAccountID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (User.IsInRole("Customer") && designComment.SenderAccountID != currentAccountID)
            {
                _logger.LogWarning("Tài khoản Customer {CurrentAccountID} cố gắng gửi bình luận bằng tài khoản của người khác: {SenderAccountID}.", currentAccountID, designComment.SenderAccountID);
                return Forbid("Bạn chỉ có thể gửi bình luận bằng tài khoản của chính mình.");
            }

            try
            {
                var createdDesignCommentDto = await _designCommentService.CreateDesignCommentAsync(designComment);
                _logger.LogInformation("Đã tạo bình luận thiết kế mới với ID: {CommentID}", createdDesignCommentDto.CommentID);
                return CreatedAtAction(nameof(GetDesignComment), new { id = createdDesignCommentDto.CommentID }, createdDesignCommentDto);
            }
            catch (ArgumentException ex) // Bắt lỗi từ Service nếu FK không hợp lệ
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi tạo bình luận thiết kế: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/DesignComments/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Designer,Sales")] // Admin, Manager, Designer, Sales có thể cập nhật
        public async Task<IActionResult> PutDesignComment(string id, DesignComment designComment)
        {
            _logger.LogInformation("Yêu cầu cập nhật bình luận thiết kế với ID: {CommentID}", id);
            if (id != designComment.CommentID)
            {
                return BadRequest();
            }

            // Kiểm tra FKs chính
            if (!string.IsNullOrEmpty(designComment.DesignID) && !DesignExists(designComment.DesignID))
            {
                return BadRequest("DesignID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(designComment.SenderAccountID) && !AccountExists(designComment.SenderAccountID))
            {
                return BadRequest("SenderAccountID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(designComment.ParentCommentID) && !DesignCommentExists(designComment.ParentCommentID))
            {
                return BadRequest("ParentCommentID không tồn tại.");
            }

            // Logic kiểm tra người cập nhật có phải người tạo comment không (nếu muốn hạn chế)
            // var currentAccountID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // var existingCommentDto = await _designCommentService.GetDesignCommentByIdAsync(id);
            // if (User.IsInRole("Customer") && existingCommentDto != null && existingCommentDto.SenderAccountID != currentAccountID)
            // {
            //     return Forbid("Bạn chỉ có thể cập nhật bình luận của chính mình.");
            // }

            try
            {
                var success = await _designCommentService.UpdateDesignCommentAsync(id, designComment);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy bình luận thiết kế để cập nhật với ID: {CommentID}", id);
                    return NotFound();
                }

                _logger.LogInformation("Đã cập nhật bình luận thiết kế với ID: {CommentID}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi cập nhật bình luận thiết kế: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DesignCommentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
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
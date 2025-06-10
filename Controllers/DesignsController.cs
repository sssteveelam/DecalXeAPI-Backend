using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Vẫn cần cho các hàm Exists cơ bản nếu giữ lại ở Controller
using DecalXeAPI.Data; // Vẫn cần ApplicationDbContext cho các hàm Exists
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // <-- THÊM DÒNG NÀY (Để sử dụng IDesignService)
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System; // Để sử dụng ArgumentException

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Designer")]
    public class DesignsController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists cơ bản
        private readonly IDesignService _designService; // <-- KHAI BÁO BIẾN CHO SERVICE
        private readonly IMapper _mapper;
        private readonly ILogger<DesignsController> _logger;

        public DesignsController(ApplicationDbContext context, IDesignService designService, IMapper mapper, ILogger<DesignsController> logger) // <-- TIÊM IDesignService
        {
            _context = context;
            _designService = designService;
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/Designs
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Designer,Sales")]
        public async Task<ActionResult<IEnumerable<DesignDto>>> GetDesigns()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách thiết kế.");
            var designs = await _designService.GetDesignsAsync();
            return Ok(designs);
        }

        // API: GET api/Designs/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Designer,Sales")]
        public async Task<ActionResult<DesignDto>> GetDesign(string id)
        {
            _logger.LogInformation("Yêu cầu lấy thiết kế với ID: {DesignID}", id);
            var designDto = await _designService.GetDesignByIdAsync(id);

            if (designDto == null)
            {
                _logger.LogWarning("Không tìm thấy thiết kế với ID: {DesignID}", id);
                return NotFound();
            }

            return Ok(designDto);
        }

        // API: POST api/Designs
        [HttpPost]
        public async Task<ActionResult<DesignDto>> PostDesign(Design design) // Vẫn nhận Design Model
        {
            _logger.LogInformation("Yêu cầu tạo thiết kế mới cho OrderID: {OrderID}", design.OrderID);

            // --- KIỂM TRA FKs CHÍNH TRƯỚC KHI GỬI VÀO SERVICE ---
            // Controller sẽ chịu trách nhiệm validate các FKs chính
            if (!string.IsNullOrEmpty(design.OrderID) && !OrderExists(design.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(design.DesignerID) && !EmployeeExists(design.DesignerID))
            {
                return BadRequest("DesignerID không tồn tại.");
            }

            try
            {
                var createdDesignDto = await _designService.CreateDesignAsync(design);
                _logger.LogInformation("Đã tạo thiết kế mới với ID: {DesignID}", createdDesignDto.DesignID);
                return CreatedAtAction(nameof(GetDesign), new { id = createdDesignDto.DesignID }, createdDesignDto);
            }
            catch (ArgumentException ex) // Bắt lỗi từ Service nếu FK không hợp lệ
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi tạo thiết kế: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/Designs/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDesign(string id, Design design)
        {
            _logger.LogInformation("Yêu cầu cập nhật thiết kế với ID: {DesignID}", id);
            if (id != design.DesignID)
            {
                return BadRequest();
            }

            // Kiểm tra FKs chính
            if (!string.IsNullOrEmpty(design.OrderID) && !OrderExists(design.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(design.DesignerID) && !EmployeeExists(design.DesignerID))
            {
                return BadRequest("DesignerID không tồn tại.");
            }

            try
            {
                var success = await _designService.UpdateDesignAsync(id, design);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy thiết kế để cập nhật với ID: {DesignID}", id);
                    return NotFound();
                }

                _logger.LogInformation("Đã cập nhật thiết kế với ID: {DesignID}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi cập nhật thiết kế: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (DbUpdateConcurrencyException) // Vẫn bắt riêng lỗi này ở Controller
            {
                if (!DesignExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // API: DELETE api/Designs/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDesign(string id)
        {
            _logger.LogInformation("Yêu cầu xóa thiết kế với ID: {DesignID}", id);
            var success = await _designService.DeleteDesignAsync(id);

            if (!success)
            {
                _logger.LogWarning("Không tìm thấy thiết kế để xóa với ID: {DesignID}", id);
                return NotFound();
            }

            return NoContent();
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool DesignExists(string id) { return _context.Designs.Any(e => e.DesignID == id); }
        private bool OrderExists(string id) { return _context.Orders.Any(e => e.OrderID == id); }
        private bool EmployeeExists(string id) { return _context.Employees.Any(e => e.EmployeeID == id); }
    }
}
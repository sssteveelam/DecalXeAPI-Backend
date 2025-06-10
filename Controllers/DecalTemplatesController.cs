using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // <-- THÊM DÒNG NÀY (Để sử dụng IDecalTemplateService)
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System; // Để sử dụng ArgumentException

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")] // Quyền cho DecalTemplatesController
    public class DecalTemplatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists cơ bản
        private readonly IDecalTemplateService _decalTemplateService; // <-- KHAI BÁO BIẾN CHO SERVICE
        private readonly IMapper _mapper;
        private readonly ILogger<DecalTemplatesController> _logger;

        public DecalTemplatesController(ApplicationDbContext context, IDecalTemplateService decalTemplateService, IMapper mapper, ILogger<DecalTemplatesController> logger) // <-- TIÊM IDecalTemplateService
        {
            _context = context;
            _decalTemplateService = decalTemplateService;
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/DecalTemplates
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DecalTemplateDto>>> GetDecalTemplates()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách mẫu decal.");
            var decalTemplates = await _decalTemplateService.GetDecalTemplatesAsync();
            return Ok(decalTemplates);
        }

        // API: GET api/DecalTemplates/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<DecalTemplateDto>> GetDecalTemplate(string id)
        {
            _logger.LogInformation("Yêu cầu lấy mẫu decal với ID: {TemplateID}", id);
            var decalTemplateDto = await _decalTemplateService.GetDecalTemplateByIdAsync(id);

            if (decalTemplateDto == null)
            {
                _logger.LogWarning("Không tìm thấy mẫu decal với ID: {TemplateID}", id);
                return NotFound();
            }

            return Ok(decalTemplateDto);
        }

        // API: POST api/DecalTemplates
        [HttpPost]
        public async Task<ActionResult<DecalTemplateDto>> PostDecalTemplate(DecalTemplate decalTemplate) // Vẫn nhận Model
        {
            _logger.LogInformation("Yêu cầu tạo mẫu decal mới: {TemplateName}", decalTemplate.TemplateName);

            // --- KIỂM TRA FKs CHÍNH TRƯỚC KHI GỬI VÀO SERVICE ---
            if (!string.IsNullOrEmpty(decalTemplate.DecalTypeID) && !DecalTypeExists(decalTemplate.DecalTypeID))
            {
                return BadRequest("DecalTypeID không tồn tại.");
            }

            try
            {
                var createdDecalTemplateDto = await _decalTemplateService.CreateDecalTemplateAsync(decalTemplate);
                _logger.LogInformation("Đã tạo mẫu decal mới với ID: {TemplateID}", createdDecalTemplateDto.TemplateID);
                return CreatedAtAction(nameof(GetDecalTemplate), new { id = createdDecalTemplateDto.TemplateID }, createdDecalTemplateDto);
            }
            catch (ArgumentException ex) // Bắt lỗi từ Service nếu có (ví dụ: duplicate name)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi tạo mẫu decal: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/DecalTemplates/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDecalTemplate(string id, DecalTemplate decalTemplate)
        {
            _logger.LogInformation("Yêu cầu cập nhật mẫu decal với ID: {TemplateID}", id);
            if (id != decalTemplate.TemplateID)
            {
                return BadRequest();
            }

            // Kiểm tra FKs chính
            if (!string.IsNullOrEmpty(decalTemplate.DecalTypeID) && !DecalTypeExists(decalTemplate.DecalTypeID))
            {
                return BadRequest("DecalTypeID không tồn tại.");
            }

            try
            {
                var success = await _decalTemplateService.UpdateDecalTemplateAsync(id, decalTemplate);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy mẫu decal để cập nhật với ID: {TemplateID}", id);
                    return NotFound();
                }

                _logger.LogInformation("Đã cập nhật mẫu decal với ID: {TemplateID}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi cập nhật mẫu decal: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DecalTemplateExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // API: DELETE api/DecalTemplates/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDecalTemplate(string id)
        {
            _logger.LogInformation("Yêu cầu xóa mẫu decal với ID: {TemplateID}", id);
            var success = await _decalTemplateService.DeleteDecalTemplateAsync(id);

            if (!success)
            {
                _logger.LogWarning("Không tìm thấy mẫu decal để xóa với ID: {TemplateID}", id);
                return NotFound();
            }

            return NoContent();
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool DecalTemplateExists(string id) { return _context.DecalTemplates.Any(e => e.TemplateID == id); }
        private bool DecalTypeExists(string id) { return _context.DecalTypes.Any(e => e.DecalTypeID == id); }
    }
}
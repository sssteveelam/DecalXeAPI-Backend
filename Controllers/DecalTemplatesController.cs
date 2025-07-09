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

        // API: POST api/DecalTemplates (ĐÃ NÂNG CẤP)
        [HttpPost]
        public async Task<ActionResult<DecalTemplateDto>> PostDecalTemplate(CreateDecalTemplateDto createDto)
        {
            _logger.LogInformation("Yêu cầu tạo mẫu decal mới: {TemplateName}", createDto.TemplateName);

            var decalTemplate = _mapper.Map<DecalTemplate>(createDto);

            try
            {
                var createdDecalTemplateDto = await _decalTemplateService.CreateDecalTemplateAsync(decalTemplate);
                _logger.LogInformation("Đã tạo mẫu decal mới với ID: {TemplateID}", createdDecalTemplateDto.TemplateID);
                return CreatedAtAction(nameof(GetDecalTemplate), new { id = createdDecalTemplateDto.TemplateID }, createdDecalTemplateDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi tạo mẫu decal: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/DecalTemplates/{id} (ĐÃ NÂNG CẤP)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDecalTemplate(string id, UpdateDecalTemplateDto updateDto)
        {
            _logger.LogInformation("Yêu cầu cập nhật mẫu decal với ID: {TemplateID}", id);

            var decalTemplate = await _context.DecalTemplates.FindAsync(id);
            if (decalTemplate == null)
            {
                return NotFound();
            }
            
            _mapper.Map(updateDto, decalTemplate);

            try
            {
                var success = await _decalTemplateService.UpdateDecalTemplateAsync(id, decalTemplate);
                if (!success) return NotFound();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        // --- API MỚI ĐỂ GÁN TEMPLATE VÀO XE ---
        [HttpPost("{templateId}/vehicles/{modelId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AssignTemplateToVehicle(string templateId, string modelId)
        {
            var (success, errorMessage) = await _decalTemplateService.AssignTemplateToVehicleAsync(templateId, modelId);
            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }
            return Ok(new { message = "Gán mẫu decal cho xe thành công." });
        }

        // --- API MỚI ĐỂ GỠ TEMPLATE KHỎI XE ---
        [HttpDelete("{templateId}/vehicles/{modelId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UnassignTemplateFromVehicle(string templateId, string modelId)
        {
            var (success, errorMessage) = await _decalTemplateService.UnassignTemplateFromVehicleAsync(templateId, modelId);
            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }
            return NoContent();
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
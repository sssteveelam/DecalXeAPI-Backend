using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // <-- THÊM DÒNG NÀY (Để sử dụng IServiceDecalTemplateService)
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System; // Để sử dụng ArgumentException

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")] // Quyền cho ServiceDecalTemplatesController (Chỉ Admin/Manager có thể quản lý các liên kết này)
    public class ServiceDecalTemplatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists cơ bản
        private readonly IServiceDecalTemplateService _serviceDecalTemplateService; // <-- KHAI BÁO BIẾN CHO SERVICE
        private readonly IMapper _mapper;
        private readonly ILogger<ServiceDecalTemplatesController> _logger;

        public ServiceDecalTemplatesController(ApplicationDbContext context, IServiceDecalTemplateService serviceDecalTemplateService, IMapper mapper, ILogger<ServiceDecalTemplatesController> logger) // <-- TIÊM IServiceDecalTemplateService
        {
            _context = context;
            _serviceDecalTemplateService = serviceDecalTemplateService;
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/ServiceDecalTemplates
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceDecalTemplateDto>>> GetServiceDecalTemplates()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách mẫu decal dịch vụ.");
            var serviceDecalTemplates = await _serviceDecalTemplateService.GetServiceDecalTemplatesAsync();
            return Ok(serviceDecalTemplates);
        }

        // API: GET api/ServiceDecalTemplates/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceDecalTemplateDto>> GetServiceDecalTemplate(string id)
        {
            _logger.LogInformation("Yêu cầu lấy mẫu decal dịch vụ với ID: {ServiceDecalTemplateID}", id);
            var serviceDecalTemplateDto = await _serviceDecalTemplateService.GetServiceDecalTemplateByIdAsync(id);

            if (serviceDecalTemplateDto == null)
            {
                _logger.LogWarning("Không tìm thấy mẫu decal dịch vụ với ID: {ServiceDecalTemplateID}", id);
                return NotFound();
            }

            return Ok(serviceDecalTemplateDto);
        }

        // API: POST api/ServiceDecalTemplates
        [HttpPost]
        public async Task<ActionResult<ServiceDecalTemplateDto>> PostServiceDecalTemplate(ServiceDecalTemplate serviceDecalTemplate) // Vẫn nhận Model
        {
            _logger.LogInformation("Yêu cầu tạo mẫu decal dịch vụ mới cho ServiceID: {ServiceID}, TemplateID: {TemplateID}", serviceDecalTemplate.ServiceID, serviceDecalTemplate.TemplateID);

            // --- KIỂM TRA FKs CHÍNH TRƯỚC KHI GỬI VÀO SERVICE ---
            if (!string.IsNullOrEmpty(serviceDecalTemplate.ServiceID) && !DecalServiceExists(serviceDecalTemplate.ServiceID))
            {
                return BadRequest("ServiceID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(serviceDecalTemplate.TemplateID) && !DecalTemplateExists(serviceDecalTemplate.TemplateID))
            {
                return BadRequest("TemplateID không tồn tại.");
            }

            try
            {
                var createdServiceDecalTemplateDto = await _serviceDecalTemplateService.CreateServiceDecalTemplateAsync(serviceDecalTemplate);
                _logger.LogInformation("Đã tạo mẫu decal dịch vụ mới với ID: {ServiceDecalTemplateID}", createdServiceDecalTemplateDto.ServiceDecalTemplateID);
                return CreatedAtAction(nameof(GetServiceDecalTemplate), new { id = createdServiceDecalTemplateDto.ServiceDecalTemplateID }, createdServiceDecalTemplateDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi tạo mẫu decal dịch vụ: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/ServiceDecalTemplates/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServiceDecalTemplate(string id, ServiceDecalTemplate serviceDecalTemplate)
        {
            _logger.LogInformation("Yêu cầu cập nhật mẫu decal dịch vụ với ID: {ServiceDecalTemplateID}", id);
            if (id != serviceDecalTemplate.ServiceDecalTemplateID)
            {
                return BadRequest();
            }

            // Kiểm tra FKs chính
            if (!string.IsNullOrEmpty(serviceDecalTemplate.ServiceID) && !DecalServiceExists(serviceDecalTemplate.ServiceID))
            {
                return BadRequest("ServiceID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(serviceDecalTemplate.TemplateID) && !DecalTemplateExists(serviceDecalTemplate.TemplateID))
            {
                return BadRequest("TemplateID không tồn tại.");
            }

            try
            {
                var success = await _serviceDecalTemplateService.UpdateServiceDecalTemplateAsync(id, serviceDecalTemplate);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy mẫu decal dịch vụ để cập nhật với ID: {ServiceDecalTemplateID}", id);
                    return NotFound();
                }

                _logger.LogInformation("Đã cập nhật mẫu decal dịch vụ với ID: {ServiceDecalTemplateID}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi cập nhật mẫu decal dịch vụ: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceDecalTemplateExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // API: DELETE api/ServiceDecalTemplates/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceDecalTemplate(string id)
        {
            _logger.LogInformation("Yêu cầu xóa mẫu decal dịch vụ với ID: {ServiceDecalTemplateID}", id);
            var success = await _serviceDecalTemplateService.DeleteServiceDecalTemplateAsync(id);

            if (!success)
            {
                _logger.LogWarning("Không tìm thấy mẫu decal dịch vụ để xóa với ID: {ServiceDecalTemplateID}", id);
                return NotFound();
            }

            return NoContent();
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool ServiceDecalTemplateExists(string id) { return _context.ServiceDecalTemplates.Any(e => e.ServiceDecalTemplateID == id); }
        private bool DecalServiceExists(string id) { return _context.DecalServices.Any(e => e.ServiceID == id); }
        private bool DecalTemplateExists(string id) { return _context.DecalTemplates.Any(e => e.TemplateID == id); }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs; // Để sử dụng ServiceDecalTemplateDto
using AutoMapper; // Để sử dụng AutoMapper
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization; // Để sử dụng IEnumerable

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Designer")]
    public class ServiceDecalTemplatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Khai báo biến IMapper

        public ServiceDecalTemplatesController(ApplicationDbContext context, IMapper mapper) // Tiêm IMapper
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/ServiceDecalTemplates
        // Lấy tất cả các ServiceDecalTemplate, bao gồm thông tin DecalService và DecalTemplate liên quan, trả về DTO
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceDecalTemplateDto>>> GetServiceDecalTemplates() // Kiểu trả về là ServiceDecalTemplateDto
        {
            var serviceDecalTemplates = await _context.ServiceDecalTemplates
                                                        .Include(sdt => sdt.DecalService)
                                                        .Include(sdt => sdt.DecalTemplate)
                                                        .ToListAsync();
            // Sử dụng AutoMapper để ánh xạ từ List<ServiceDecalTemplate> sang List<ServiceDecalTemplateDto>
            var serviceDecalTemplateDtos = _mapper.Map<List<ServiceDecalTemplateDto>>(serviceDecalTemplates);
            return Ok(serviceDecalTemplateDtos);
        }

        // API: GET api/ServiceDecalTemplates/{id}
        // Lấy thông tin một ServiceDecalTemplate theo ServiceDecalTemplateID, bao gồm các thông tin liên quan, trả về DTO
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceDecalTemplateDto>> GetServiceDecalTemplate(string id) // Kiểu trả về là ServiceDecalTemplateDto
        {
            var serviceDecalTemplate = await _context.ServiceDecalTemplates
                                                            .Include(sdt => sdt.DecalService)
                                                            .Include(sdt => sdt.DecalTemplate)
                                                            .FirstOrDefaultAsync(sdt => sdt.ServiceDecalTemplateID == id);

            if (serviceDecalTemplate == null)
            {
                return NotFound();
            }

            // Sử dụng AutoMapper để ánh xạ từ ServiceDecalTemplate Model sang ServiceDecalTemplateDto
            var serviceDecalTemplateDto = _mapper.Map<ServiceDecalTemplateDto>(serviceDecalTemplate);
            return Ok(serviceDecalTemplateDto);
        }

        // API: POST api/ServiceDecalTemplates
        // Tạo một ServiceDecalTemplate mới, nhận vào ServiceDecalTemplate Model, trả về ServiceDecalTemplateDto sau khi tạo
        [HttpPost]
        public async Task<ActionResult<ServiceDecalTemplateDto>> PostServiceDecalTemplate(ServiceDecalTemplate serviceDecalTemplate) // Kiểu trả về là ServiceDecalTemplateDto
        {
            // Kiểm tra xem ServiceID và TemplateID có tồn tại không
            if (!string.IsNullOrEmpty(serviceDecalTemplate.ServiceID) && !DecalServiceExists(serviceDecalTemplate.ServiceID))
            {
                return BadRequest("ServiceID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(serviceDecalTemplate.TemplateID) && !DecalTemplateExists(serviceDecalTemplate.TemplateID))
            {
                return BadRequest("TemplateID không tồn tại.");
            }

            _context.ServiceDecalTemplates.Add(serviceDecalTemplate);
            await _context.SaveChangesAsync();

            // Tải lại thông tin liên quan để AutoMapper có thể ánh xạ đầy đủ
            await _context.Entry(serviceDecalTemplate).Reference(sdt => sdt.DecalService).LoadAsync();
            await _context.Entry(serviceDecalTemplate).Reference(sdt => sdt.DecalTemplate).LoadAsync();

            // Ánh xạ ServiceDecalTemplate Model vừa tạo sang ServiceDecalTemplateDto để trả về
            var serviceDecalTemplateDto = _mapper.Map<ServiceDecalTemplateDto>(serviceDecalTemplate);
            return CreatedAtAction(nameof(GetServiceDecalTemplate), new { id = serviceDecalTemplateDto.ServiceDecalTemplateID }, serviceDecalTemplateDto);
        }

        // API: PUT api/ServiceDecalTemplates/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServiceDecalTemplate(string id, ServiceDecalTemplate serviceDecalTemplate)
        {
            if (id != serviceDecalTemplate.ServiceDecalTemplateID)
            {
                return BadRequest();
            }

            if (!string.IsNullOrEmpty(serviceDecalTemplate.ServiceID) && !DecalServiceExists(serviceDecalTemplate.ServiceID))
            {
                return BadRequest("ServiceID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(serviceDecalTemplate.TemplateID) && !DecalTemplateExists(serviceDecalTemplate.TemplateID))
            {
                return BadRequest("TemplateID không tồn tại.");
            }

            _context.Entry(serviceDecalTemplate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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

            return NoContent();
        }

        // API: DELETE api/ServiceDecalTemplates/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceDecalTemplate(string id)
        {
            var serviceDecalTemplate = await _context.ServiceDecalTemplates.FindAsync(id);
            if (serviceDecalTemplate == null)
            {
                return NotFound();
            }

            _context.ServiceDecalTemplates.Remove(serviceDecalTemplate);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ServiceDecalTemplateExists(string id)
        {
            return _context.ServiceDecalTemplates.Any(e => e.ServiceDecalTemplateID == id);
        }

        private bool DecalServiceExists(string id)
        {
            return _context.DecalServices.Any(e => e.ServiceID == id);
        }

        private bool DecalTemplateExists(string id)
        {
            return _context.DecalTemplates.Any(e => e.TemplateID == id);
        }
    }
}
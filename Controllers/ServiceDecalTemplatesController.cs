using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceDecalTemplatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceDecalTemplatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: GET api/ServiceDecalTemplates
        // Lấy tất cả các ServiceDecalTemplate, bao gồm thông tin DecalService và DecalTemplate liên quan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceDecalTemplate>>> GetServiceDecalTemplates()
        {
            // .Include() để tải dữ liệu của cả DecalService và DecalTemplate liên quan
            return await _context.ServiceDecalTemplates
                                .Include(sdt => sdt.DecalService)
                                .Include(sdt => sdt.DecalTemplate)
                                .ToListAsync();
        }

        // API: GET api/ServiceDecalTemplates/{id}
        // Lấy thông tin một ServiceDecalTemplate theo ServiceDecalTemplateID, bao gồm các thông tin liên quan
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceDecalTemplate>> GetServiceDecalTemplate(string id)
        {
            var serviceDecalTemplate = await _context.ServiceDecalTemplates
                                                        .Include(sdt => sdt.DecalService)
                                                        .Include(sdt => sdt.DecalTemplate)
                                                        .FirstOrDefaultAsync(sdt => sdt.ServiceDecalTemplateID == id);

            if (serviceDecalTemplate == null)
            {
                return NotFound();
            }

            return serviceDecalTemplate;
        }

        // API: POST api/ServiceDecalTemplates
        // Tạo một ServiceDecalTemplate mới
        [HttpPost]
        public async Task<ActionResult<ServiceDecalTemplate>> PostServiceDecalTemplate(ServiceDecalTemplate serviceDecalTemplate)
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

            // Tải lại thông tin liên quan để trả về đầy đủ
            await _context.Entry(serviceDecalTemplate).Reference(sdt => sdt.DecalService).LoadAsync();
            await _context.Entry(serviceDecalTemplate).Reference(sdt => sdt.DecalTemplate).LoadAsync();

            return CreatedAtAction(nameof(GetServiceDecalTemplate), new { id = serviceDecalTemplate.ServiceDecalTemplateID }, serviceDecalTemplate);
        }

        // API: PUT api/ServiceDecalTemplates/{id}
        // Cập nhật thông tin một ServiceDecalTemplate hiện có
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServiceDecalTemplate(string id, ServiceDecalTemplate serviceDecalTemplate)
        {
            if (id != serviceDecalTemplate.ServiceDecalTemplateID)
            {
                return BadRequest();
            }

            // Kiểm tra xem ServiceID và TemplateID có tồn tại không trước khi cập nhật
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
        // Xóa một ServiceDecalTemplate
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

        // Hàm hỗ trợ: Kiểm tra xem ServiceDecalTemplate có tồn tại không
        private bool ServiceDecalTemplateExists(string id)
        {
            return _context.ServiceDecalTemplates.Any(e => e.ServiceDecalTemplateID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem DecalService có tồn tại không (copy từ DecalServicesController)
        private bool DecalServiceExists(string id)
        {
            return _context.DecalServices.Any(e => e.ServiceID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem DecalTemplate có tồn tại không (copy từ DecalTemplatesController)
        private bool DecalTemplateExists(string id)
        {
            return _context.DecalTemplates.Any(e => e.TemplateID == id);
        }
    }
}
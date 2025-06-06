using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DecalTemplatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DecalTemplatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: GET api/DecalTemplates
        // Lấy tất cả các DecalTemplate, bao gồm thông tin DecalType liên quan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DecalTemplate>>> GetDecalTemplates()
        {
            // .Include(dt => dt.DecalType) sẽ tải thông tin DecalType liên kết.
            return await _context.DecalTemplates.Include(dt => dt.DecalType).ToListAsync();
        }

        // API: GET api/DecalTemplates/{id}
        // Lấy thông tin một DecalTemplate theo TemplateID, bao gồm DecalType liên quan
        [HttpGet("{id}")]
        public async Task<ActionResult<DecalTemplate>> GetDecalTemplate(string id)
        {
            var decalTemplate = await _context.DecalTemplates.Include(dt => dt.DecalType).FirstOrDefaultAsync(dt => dt.TemplateID == id);

            if (decalTemplate == null)
            {
                return NotFound();
            }

            return decalTemplate;
        }

        // API: POST api/DecalTemplates
        // Tạo một DecalTemplate mới
        [HttpPost]
        public async Task<ActionResult<DecalTemplate>> PostDecalTemplate(DecalTemplate decalTemplate)
        {
            // Kiểm tra xem DecalTypeID có tồn tại không
            if (!string.IsNullOrEmpty(decalTemplate.DecalTypeID) && !DecalTypeExists(decalTemplate.DecalTypeID))
            {
                return BadRequest("DecalTypeID không tồn tại.");
            }

            _context.DecalTemplates.Add(decalTemplate);
            await _context.SaveChangesAsync();

            // Tải lại thông tin DecalType để trả về đầy đủ cho client
            await _context.Entry(decalTemplate).Reference(dt => dt.DecalType).LoadAsync();

            return CreatedAtAction(nameof(GetDecalTemplate), new { id = decalTemplate.TemplateID }, decalTemplate);
        }

        // API: PUT api/DecalTemplates/{id}
        // Cập nhật thông tin một DecalTemplate hiện có
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDecalTemplate(string id, DecalTemplate decalTemplate)
        {
            if (id != decalTemplate.TemplateID)
            {
                return BadRequest();
            }

            // Kiểm tra xem DecalTypeID có tồn tại không trước khi cập nhật
            if (!string.IsNullOrEmpty(decalTemplate.DecalTypeID) && !DecalTypeExists(decalTemplate.DecalTypeID))
            {
                return BadRequest("DecalTypeID không tồn tại.");
            }

            _context.Entry(decalTemplate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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

            return NoContent();
        }

        // API: DELETE api/DecalTemplates/{id}
        // Xóa một DecalTemplate
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDecalTemplate(string id)
        {
            var decalTemplate = await _context.DecalTemplates.FindAsync(id);
            if (decalTemplate == null)
            {
                return NotFound();
            }

            _context.DecalTemplates.Remove(decalTemplate);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Hàm hỗ trợ: Kiểm tra xem DecalTemplate có tồn tại không
        private bool DecalTemplateExists(string id)
        {
            return _context.DecalTemplates.Any(e => e.TemplateID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem DecalType có tồn tại không (copy từ DecalTypesController)
        private bool DecalTypeExists(string id)
        {
            return _context.DecalTypes.Any(e => e.DecalTypeID == id);
        }
    }
}
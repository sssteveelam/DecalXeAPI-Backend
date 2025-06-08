using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs; // Để sử dụng DecalTemplateDto
using AutoMapper; // Để sử dụng AutoMapper
using System.Collections.Generic; // Để sử dụng IEnumerable

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DecalTemplatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Khai báo biến IMapper

        public DecalTemplatesController(ApplicationDbContext context, IMapper mapper) // Tiêm IMapper
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/DecalTemplates
        // Lấy tất cả các DecalTemplate, bao gồm thông tin DecalType liên quan, trả về DTO
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DecalTemplateDto>>> GetDecalTemplates() // Kiểu trả về là DecalTemplateDto
        {
            var decalTemplates = await _context.DecalTemplates.Include(dt => dt.DecalType).ToListAsync();
            // Sử dụng AutoMapper để ánh xạ từ List<DecalTemplate> sang List<DecalTemplateDto>
            var decalTemplateDtos = _mapper.Map<List<DecalTemplateDto>>(decalTemplates);
            return Ok(decalTemplateDtos);
        }

        // API: GET api/DecalTemplates/{id}
        // Lấy thông tin một DecalTemplate theo TemplateID, bao gồm DecalType liên quan, trả về DTO
        [HttpGet("{id}")]
        public async Task<ActionResult<DecalTemplateDto>> GetDecalTemplate(string id) // Kiểu trả về là DecalTemplateDto
        {
            var decalTemplate = await _context.DecalTemplates.Include(dt => dt.DecalType).FirstOrDefaultAsync(dt => dt.TemplateID == id);

            if (decalTemplate == null)
            {
                return NotFound();
            }

            // Sử dụng AutoMapper để ánh xạ từ DecalTemplate Model sang DecalTemplateDto
            var decalTemplateDto = _mapper.Map<DecalTemplateDto>(decalTemplate);
            return Ok(decalTemplateDto);
        }

        // API: POST api/DecalTemplates
        // Tạo một DecalTemplate mới, nhận vào DecalTemplate Model, trả về DecalTemplateDto sau khi tạo
        [HttpPost]
        public async Task<ActionResult<DecalTemplateDto>> PostDecalTemplate(DecalTemplate decalTemplate) // Kiểu trả về là DecalTemplateDto
        {
            // Kiểm tra xem DecalTypeID có tồn tại không
            if (!string.IsNullOrEmpty(decalTemplate.DecalTypeID) && !DecalTypeExists(decalTemplate.DecalTypeID))
            {
                return BadRequest("DecalTypeID không tồn tại.");
            }

            _context.DecalTemplates.Add(decalTemplate);
            await _context.SaveChangesAsync();

            // Tải lại thông tin DecalType để AutoMapper có thể ánh xạ DecalTypeName
            await _context.Entry(decalTemplate).Reference(dt => dt.DecalType).LoadAsync();

            // Ánh xạ DecalTemplate Model vừa tạo sang DecalTemplateDto để trả về
            var decalTemplateDto = _mapper.Map<DecalTemplateDto>(decalTemplate);
            return CreatedAtAction(nameof(GetDecalTemplate), new { id = decalTemplateDto.TemplateID }, decalTemplateDto);
        }

        // API: PUT api/DecalTemplates/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDecalTemplate(string id, DecalTemplate decalTemplate)
        {
            if (id != decalTemplate.TemplateID)
            {
                return BadRequest();
            }

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

        private bool DecalTemplateExists(string id)
        {
            return _context.DecalTemplates.Any(e => e.TemplateID == id);
        }

        private bool DecalTypeExists(string id)
        {
            return _context.DecalTypes.Any(e => e.DecalTypeID == id);
        }
    }
}
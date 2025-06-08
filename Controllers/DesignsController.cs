using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs; // Để sử dụng DesignDto
using AutoMapper; // Để sử dụng AutoMapper
using System.Collections.Generic; // Để sử dụng IEnumerable

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DesignsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Khai báo biến IMapper

        public DesignsController(ApplicationDbContext context, IMapper mapper) // Tiêm IMapper
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/Designs
        // Lấy tất cả các Design, bao gồm thông tin Order và Designer (Employee) liên quan, trả về DTO
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DesignDto>>> GetDesigns() // Kiểu trả về là DesignDto
        {
            var designs = await _context.Designs
                                    .Include(d => d.Order)
                                    .Include(d => d.Designer)
                                    .ToListAsync();
            // Sử dụng AutoMapper để ánh xạ từ List<Design> sang List<DesignDto>
            var designDtos = _mapper.Map<List<DesignDto>>(designs);
            return Ok(designDtos);
        }

        // API: GET api/Designs/{id}
        // Lấy thông tin một Design theo DesignID, bao gồm các thông tin liên quan, trả về DTO
        [HttpGet("{id}")]
        public async Task<ActionResult<DesignDto>> GetDesign(string id) // Kiểu trả về là DesignDto
        {
            var design = await _context.Designs
                                        .Include(d => d.Order)
                                        .Include(d => d.Designer)
                                        .FirstOrDefaultAsync(d => d.DesignID == id);

            if (design == null)
            {
                return NotFound();
            }

            // Sử dụng AutoMapper để ánh xạ từ Design Model sang DesignDto
            var designDto = _mapper.Map<DesignDto>(design);
            return Ok(designDto);
        }

        // API: POST api/Designs
        // Tạo một Design mới, nhận vào Design Model, trả về DesignDto sau khi tạo
        [HttpPost]
        public async Task<ActionResult<DesignDto>> PostDesign(Design design) // Kiểu trả về là DesignDto
        {
            // Kiểm tra xem OrderID và DesignerID có tồn tại không
            if (!string.IsNullOrEmpty(design.OrderID) && !OrderExists(design.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(design.DesignerID) && !EmployeeExists(design.DesignerID))
            {
                return BadRequest("DesignerID không tồn tại.");
            }

            _context.Designs.Add(design);
            await _context.SaveChangesAsync();

            // Tải lại thông tin liên quan để AutoMapper có thể ánh xạ đầy đủ
            await _context.Entry(design).Reference(d => d.Order).LoadAsync();
            await _context.Entry(design).Reference(d => d.Designer).LoadAsync();

            // Ánh xạ Design Model vừa tạo sang DesignDto để trả về
            var designDto = _mapper.Map<DesignDto>(design);
            return CreatedAtAction(nameof(GetDesign), new { id = designDto.DesignID }, designDto);
        }

        // API: PUT api/Designs/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDesign(string id, Design design)
        {
            if (id != design.DesignID)
            {
                return BadRequest();
            }

            if (!string.IsNullOrEmpty(design.OrderID) && !OrderExists(design.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(design.DesignerID) && !EmployeeExists(design.DesignerID))
            {
                return BadRequest("DesignerID không tồn tại.");
            }

            _context.Entry(design).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
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

            return NoContent();
        }

        // API: DELETE api/Designs/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDesign(string id)
        {
            var design = await _context.Designs.FindAsync(id);
            if (design == null)
            {
                return NotFound();
            }

            _context.Designs.Remove(design);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DesignExists(string id)
        {
            return _context.Designs.Any(e => e.DesignID == id);
        }

        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }

        private bool EmployeeExists(string id)
        {
            return _context.Employees.Any(e => e.EmployeeID == id);
        }
    }
}
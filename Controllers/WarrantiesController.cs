using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs; // Để sử dụng WarrantyDto
using AutoMapper; // Để sử dụng AutoMapper
using System.Collections.Generic; // Để sử dụng IEnumerable
using System; // Để sử dụng DateTime

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarrantiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Khai báo biến IMapper

        public WarrantiesController(ApplicationDbContext context, IMapper mapper) // Tiêm IMapper
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/Warranties
        // Lấy tất cả các Warranty, bao gồm thông tin Order liên quan, trả về DTO
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WarrantyDto>>> GetWarranties() // Kiểu trả về là WarrantyDto
        {
            var warranties = await _context.Warranties.Include(w => w.Order).ToListAsync();
            // Sử dụng AutoMapper để ánh xạ từ List<Warranty> sang List<WarrantyDto>
            var warrantyDtos = _mapper.Map<List<WarrantyDto>>(warranties);
            return Ok(warrantyDtos);
        }

        // API: GET api/Warranties/{id}
        // Lấy thông tin một Warranty theo WarrantyID, bao gồm Order liên quan, trả về DTO
        [HttpGet("{id}")]
        public async Task<ActionResult<WarrantyDto>> GetWarranty(string id) // Kiểu trả về là WarrantyDto
        {
            var warranty = await _context.Warranties.Include(w => w.Order).FirstOrDefaultAsync(w => w.WarrantyID == id);

            if (warranty == null)
            {
                return NotFound();
            }

            // Sử dụng AutoMapper để ánh xạ từ Warranty Model sang WarrantyDto
            var warrantyDto = _mapper.Map<WarrantyDto>(warranty);
            return Ok(warrantyDto);
        }

        // API: POST api/Warranties
        // Tạo một Warranty mới, nhận vào Warranty Model, trả về WarrantyDto sau khi tạo
        [HttpPost]
        public async Task<ActionResult<WarrantyDto>> PostWarranty(Warranty warranty) // Kiểu trả về là WarrantyDto
        {
            // Kiểm tra OrderID có tồn tại không
            if (!string.IsNullOrEmpty(warranty.OrderID) && !OrderExists(warranty.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }

            _context.Warranties.Add(warranty);
            await _context.SaveChangesAsync();

            // Tải lại thông tin Order để AutoMapper có thể ánh xạ OrderStatus
            await _context.Entry(warranty).Reference(w => w.Order).LoadAsync();

            // Ánh xạ Warranty Model vừa tạo sang WarrantyDto để trả về
            var warrantyDto = _mapper.Map<WarrantyDto>(warranty);
            return CreatedAtAction(nameof(GetWarranty), new { id = warrantyDto.WarrantyID }, warrantyDto);
        }

        // API: PUT api/Warranties/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWarranty(string id, Warranty warranty)
        {
            if (id != warranty.WarrantyID)
            {
                return BadRequest();
            }

            if (!string.IsNullOrEmpty(warranty.OrderID) && !OrderExists(warranty.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }

            _context.Entry(warranty).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WarrantyExists(id))
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

        // API: DELETE api/Warranties/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWarranty(string id)
        {
            var warranty = await _context.Warranties.FindAsync(id);
            if (warranty == null)
            {
                return NotFound();
            }

            _context.Warranties.Remove(warranty);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool WarrantyExists(string id)
        {
            return _context.Warranties.Any(e => e.WarrantyID == id);
        }

        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }
    }
}
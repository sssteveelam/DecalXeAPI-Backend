using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs; // Để sử dụng DecalServiceDto
using AutoMapper; // Để sử dụng AutoMapper
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization; // Để sử dụng IEnumerable

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
    public class DecalServicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Khai báo biến IMapper

        public DecalServicesController(ApplicationDbContext context, IMapper mapper) // Tiêm IMapper
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/DecalServices
        // Lấy tất cả các DecalService, bao gồm thông tin DecalType liên quan, trả về DTO
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DecalServiceDto>>> GetDecalServices() // Kiểu trả về là DecalServiceDto
        {
            var decalServices = await _context.DecalServices.Include(ds => ds.DecalType).ToListAsync();
            // Sử dụng AutoMapper để ánh xạ từ List<DecalService> sang List<DecalServiceDto>
            var decalServiceDtos = _mapper.Map<List<DecalServiceDto>>(decalServices);
            return Ok(decalServiceDtos);
        }

        // API: GET api/DecalServices/{id}
        // Lấy thông tin một DecalService theo ServiceID, bao gồm DecalType liên quan, trả về DTO
        [HttpGet("{id}")]
        public async Task<ActionResult<DecalServiceDto>> GetDecalService(string id) // Kiểu trả về là DecalServiceDto
        {
            var decalService = await _context.DecalServices.Include(ds => ds.DecalType).FirstOrDefaultAsync(ds => ds.ServiceID == id);

            if (decalService == null)
            {
                return NotFound();
            }

            // Sử dụng AutoMapper để ánh xạ từ DecalService Model sang DecalServiceDto
            var decalServiceDto = _mapper.Map<DecalServiceDto>(decalService);
            return Ok(decalServiceDto);
        }

        // API: POST api/DecalServices
        // Tạo một DecalService mới, nhận vào DecalService Model, trả về DecalServiceDto sau khi tạo
        [HttpPost]
        public async Task<ActionResult<DecalServiceDto>> PostDecalService(DecalService decalService) // Kiểu trả về là DecalServiceDto
        {
            // Kiểm tra xem DecalTypeID có tồn tại không
            if (!string.IsNullOrEmpty(decalService.DecalTypeID) && !DecalTypeExists(decalService.DecalTypeID))
            {
                return BadRequest("DecalTypeID không tồn tại.");
            }

            _context.DecalServices.Add(decalService);
            await _context.SaveChangesAsync();

            // Tải lại thông tin DecalType để AutoMapper có thể ánh xạ DecalTypeName
            await _context.Entry(decalService).Reference(ds => ds.DecalType).LoadAsync();

            // Ánh xạ DecalService Model vừa tạo sang DecalServiceDto để trả về
            var decalServiceDto = _mapper.Map<DecalServiceDto>(decalService);
            return CreatedAtAction(nameof(GetDecalService), new { id = decalServiceDto.ServiceID }, decalServiceDto);
        }

        // API: PUT api/DecalServices/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDecalService(string id, DecalService decalService)
        {
            if (id != decalService.ServiceID)
            {
                return BadRequest();
            }

            if (!string.IsNullOrEmpty(decalService.DecalTypeID) && !DecalTypeExists(decalService.DecalTypeID))
            {
                return BadRequest("DecalTypeID không tồn tại.");
            }

            _context.Entry(decalService).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DecalServiceExists(id))
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

        // API: DELETE api/DecalServices/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDecalService(string id)
        {
            var decalService = await _context.DecalServices.FindAsync(id);
            if (decalService == null)
            {
                return NotFound();
            }

            _context.DecalServices.Remove(decalService);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DecalServiceExists(string id)
        {
            return _context.DecalServices.Any(e => e.ServiceID == id);
        }

        private bool DecalTypeExists(string id)
        {
            return _context.DecalTypes.Any(e => e.DecalTypeID == id);
        }
    }
}
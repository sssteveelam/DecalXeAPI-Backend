using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs; // Để sử dụng DecalTypeDto
using AutoMapper; // Để sử dụng AutoMapper
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization; // Để sử dụng IEnumerable

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
    public class DecalTypesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Khai báo biến IMapper

        public DecalTypesController(ApplicationDbContext context, IMapper mapper) // Tiêm IMapper
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/DecalTypes
        // Lấy tất cả các DecalType, trả về dưới dạng DecalTypeDto
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DecalTypeDto>>> GetDecalTypes() // Kiểu trả về là DecalTypeDto
        {
            var decalTypes = await _context.DecalTypes.ToListAsync();
            // Sử dụng AutoMapper để ánh xạ từ List<DecalType> sang List<DecalTypeDto>
            var decalTypeDtos = _mapper.Map<List<DecalTypeDto>>(decalTypes);
            return Ok(decalTypeDtos);
        }

        // API: GET api/DecalTypes/{id}
        // Lấy thông tin một DecalType theo DecalTypeID, trả về dưới dạng DecalTypeDto
        [HttpGet("{id}")]
        public async Task<ActionResult<DecalTypeDto>> GetDecalType(string id) // Kiểu trả về là DecalTypeDto
        {
            var decalType = await _context.DecalTypes.FindAsync(id);

            if (decalType == null)
            {
                return NotFound();
            }

            // Sử dụng AutoMapper để ánh xạ từ DecalType Model sang DecalTypeDto
            var decalTypeDto = _mapper.Map<DecalTypeDto>(decalType);
            return Ok(decalTypeDto);
        }

        // API: POST api/DecalTypes
        // Tạo một DecalType mới, nhận vào DecalType Model, trả về DecalTypeDto sau khi tạo
        [HttpPost]
        public async Task<ActionResult<DecalTypeDto>> PostDecalType(DecalType decalType) // Kiểu trả về là DecalTypeDto
        {
            _context.DecalTypes.Add(decalType);
            await _context.SaveChangesAsync();

            // Không cần LoadAsync() vì DecalTypeDto không có Navigation Property cần tải
            // (DecalTypeName được ánh xạ trực tiếp từ DecalType Model mà không cần Include)

            // Ánh xạ DecalType Model vừa tạo sang DecalTypeDto để trả về
            var decalTypeDto = _mapper.Map<DecalTypeDto>(decalType);
            return CreatedAtAction(nameof(GetDecalType), new { id = decalTypeDto.DecalTypeID }, decalTypeDto);
        }

        // API: PUT api/DecalTypes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDecalType(string id, DecalType decalType)
        {
            if (id != decalType.DecalTypeID)
            {
                return BadRequest();
            }

            _context.Entry(decalType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DecalTypeExists(id))
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

        // API: DELETE api/DecalTypes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDecalType(string id)
        {
            var decalType = await _context.DecalTypes.FindAsync(id);
            if (decalType == null)
            {
                return NotFound();
            }

            _context.DecalTypes.Remove(decalType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DecalTypeExists(string id)
        {
            return _context.DecalTypes.Any(e => e.DecalTypeID == id);
        }
    }
}
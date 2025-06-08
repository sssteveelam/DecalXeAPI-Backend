using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs; // Để sử dụng TimeSlotDefinitionDto
using AutoMapper; // Để sử dụng AutoMapper
using System.Collections.Generic; // Để sử dụng IEnumerable
using System; // Để sử dụng TimeSpan

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeSlotDefinitionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Khai báo biến IMapper

        public TimeSlotDefinitionsController(ApplicationDbContext context, IMapper mapper) // Tiêm IMapper
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/TimeSlotDefinitions
        // Lấy tất cả các TimeSlotDefinition, trả về dưới dạng TimeSlotDefinitionDto
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TimeSlotDefinitionDto>>> GetTimeSlotDefinitions() // Kiểu trả về là TimeSlotDefinitionDto
        {
            var timeSlotDefinitions = await _context.TimeSlotDefinitions.ToListAsync();
            // Sử dụng AutoMapper để ánh xạ từ List<TimeSlotDefinition> sang List<TimeSlotDefinitionDto>
            var timeSlotDefinitionDtos = _mapper.Map<List<TimeSlotDefinitionDto>>(timeSlotDefinitions);
            return Ok(timeSlotDefinitionDtos);
        }

        // API: GET api/TimeSlotDefinitions/{id}
        // Lấy thông tin một TimeSlotDefinition theo SlotDefID, trả về dưới dạng TimeSlotDefinitionDto
        [HttpGet("{id}")]
        public async Task<ActionResult<TimeSlotDefinitionDto>> GetTimeSlotDefinition(string id) // Kiểu trả về là TimeSlotDefinitionDto
        {
            var timeSlotDefinition = await _context.TimeSlotDefinitions.FindAsync(id);

            if (timeSlotDefinition == null)
            {
                return NotFound();
            }

            // Sử dụng AutoMapper để ánh xạ từ TimeSlotDefinition Model sang TimeSlotDefinitionDto
            var timeSlotDefinitionDto = _mapper.Map<TimeSlotDefinitionDto>(timeSlotDefinition);
            return Ok(timeSlotDefinitionDto);
        }

        // API: POST api/TimeSlotDefinitions
        // Tạo một TimeSlotDefinition mới, nhận vào TimeSlotDefinition Model, trả về TimeSlotDefinitionDto sau khi tạo
        [HttpPost]
        public async Task<ActionResult<TimeSlotDefinitionDto>> PostTimeSlotDefinition(TimeSlotDefinition timeSlotDefinition) // Kiểu trả về là TimeSlotDefinitionDto
        {
            _context.TimeSlotDefinitions.Add(timeSlotDefinition);
            await _context.SaveChangesAsync();

            // Không cần LoadAsync() vì TimeSlotDefinitionDto không có Navigation Property cần tải

            // Ánh xạ TimeSlotDefinition Model vừa tạo sang TimeSlotDefinitionDto để trả về
            var timeSlotDefinitionDto = _mapper.Map<TimeSlotDefinitionDto>(timeSlotDefinition);
            return CreatedAtAction(nameof(GetTimeSlotDefinition), new { id = timeSlotDefinitionDto.SlotDefID }, timeSlotDefinitionDto);
        }

        // API: PUT api/TimeSlotDefinitions/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTimeSlotDefinition(string id, TimeSlotDefinition timeSlotDefinition)
        {
            if (id != timeSlotDefinition.SlotDefID)
            {
                return BadRequest();
            }

            _context.Entry(timeSlotDefinition).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TimeSlotDefinitionExists(id))
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

        // API: DELETE api/TimeSlotDefinitions/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTimeSlotDefinition(string id)
        {
            var timeSlotDefinition = await _context.TimeSlotDefinitions.FindAsync(id);
            if (timeSlotDefinition == null)
            {
                return NotFound();
            }

            _context.TimeSlotDefinitions.Remove(timeSlotDefinition);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TimeSlotDefinitionExists(string id)
        {
            return _context.TimeSlotDefinitions.Any(e => e.SlotDefID == id);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs; // Để sử dụng ScheduledWorkUnitDto
using AutoMapper; // Để sử dụng AutoMapper
using System.Collections.Generic; // Để sử dụng IEnumerable
using System; // Để sử dụng DateTime/TimeSpan

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduledWorkUnitsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Khai báo biến IMapper

        public ScheduledWorkUnitsController(ApplicationDbContext context, IMapper mapper) // Tiêm IMapper
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/ScheduledWorkUnits
        // Lấy tất cả các ScheduledWorkUnit, bao gồm thông tin DailySchedule, TimeSlotDefinition và Order liên quan, trả về DTO
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ScheduledWorkUnitDto>>> GetScheduledWorkUnits() // Kiểu trả về là ScheduledWorkUnitDto
        {
            var scheduledWorkUnits = await _context.ScheduledWorkUnits
                                                    .Include(swu => swu.DailySchedule)
                                                    .Include(swu => swu.TimeSlotDefinition)
                                                    .Include(swu => swu.Order)
                                                    .ToListAsync();
            // Sử dụng AutoMapper để ánh xạ từ List<ScheduledWorkUnit> sang List<ScheduledWorkUnitDto>
            var scheduledWorkUnitDtos = _mapper.Map<List<ScheduledWorkUnitDto>>(scheduledWorkUnits);
            return Ok(scheduledWorkUnitDtos);
        }

        // API: GET api/ScheduledWorkUnits/{id}
        // Lấy thông tin một ScheduledWorkUnit theo ScheduledWorkUnitID, bao gồm các thông tin liên quan, trả về DTO
        [HttpGet("{id}")]
        public async Task<ActionResult<ScheduledWorkUnitDto>> GetScheduledWorkUnit(string id) // Kiểu trả về là ScheduledWorkUnitDto
        {
            var scheduledWorkUnit = await _context.ScheduledWorkUnits
                                                        .Include(swu => swu.DailySchedule)
                                                        .Include(swu => swu.TimeSlotDefinition)
                                                        .Include(swu => swu.Order)
                                                        .FirstOrDefaultAsync(swu => swu.ScheduledWorkUnitID == id);

            if (scheduledWorkUnit == null)
            {
                return NotFound();
            }

            // Sử dụng AutoMapper để ánh xạ từ ScheduledWorkUnit Model sang ScheduledWorkUnitDto
            var scheduledWorkUnitDto = _mapper.Map<ScheduledWorkUnitDto>(scheduledWorkUnit);
            return Ok(scheduledWorkUnitDto);
        }

        // API: POST api/ScheduledWorkUnits
        // Tạo một ScheduledWorkUnit mới, nhận vào ScheduledWorkUnit Model, trả về ScheduledWorkUnitDto sau khi tạo
        [HttpPost]
        public async Task<ActionResult<ScheduledWorkUnitDto>> PostScheduledWorkUnit(ScheduledWorkUnit scheduledWorkUnit) // Kiểu trả về là ScheduledWorkUnitDto
        {
            // Kiểm tra FKs có tồn tại không
            if (!string.IsNullOrEmpty(scheduledWorkUnit.DailyScheduleID) && !TechnicianDailyScheduleExists(scheduledWorkUnit.DailyScheduleID))
            {
                return BadRequest("DailyScheduleID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(scheduledWorkUnit.SlotDefID) && !TimeSlotDefinitionExists(scheduledWorkUnit.SlotDefID))
            {
                return BadRequest("SlotDefID không tồn tại.");
            }
            // OrderID có thể null, chỉ kiểm tra nếu có giá trị
            if (!string.IsNullOrEmpty(scheduledWorkUnit.OrderID) && !OrderExists(scheduledWorkUnit.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }

            _context.ScheduledWorkUnits.Add(scheduledWorkUnit);
            await _context.SaveChangesAsync();

            // Tải lại thông tin liên quan để AutoMapper có thể ánh xạ đầy đủ
            await _context.Entry(scheduledWorkUnit).Reference(swu => swu.DailySchedule).LoadAsync();
            await _context.Entry(scheduledWorkUnit).Reference(swu => swu.TimeSlotDefinition).LoadAsync();
            await _context.Entry(scheduledWorkUnit).Reference(swu => swu.Order).LoadAsync();

            // Ánh xạ ScheduledWorkUnit Model vừa tạo sang ScheduledWorkUnitDto để trả về
            var scheduledWorkUnitDto = _mapper.Map<ScheduledWorkUnitDto>(scheduledWorkUnit);
            return CreatedAtAction(nameof(GetScheduledWorkUnit), new { id = scheduledWorkUnitDto.ScheduledWorkUnitID }, scheduledWorkUnitDto);
        }

        // API: PUT api/ScheduledWorkUnits/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutScheduledWorkUnit(string id, ScheduledWorkUnit scheduledWorkUnit)
        {
            if (id != scheduledWorkUnit.ScheduledWorkUnitID)
            {
                return BadRequest();
            }

            // Kiểm tra FKs trước khi cập nhật
            if (!string.IsNullOrEmpty(scheduledWorkUnit.DailyScheduleID) && !TechnicianDailyScheduleExists(scheduledWorkUnit.DailyScheduleID))
            {
                return BadRequest("DailyScheduleID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(scheduledWorkUnit.SlotDefID) && !TimeSlotDefinitionExists(scheduledWorkUnit.SlotDefID))
            {
                return BadRequest("SlotDefID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(scheduledWorkUnit.OrderID) && !OrderExists(scheduledWorkUnit.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }

            _context.Entry(scheduledWorkUnit).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScheduledWorkUnitExists(id))
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

        // API: DELETE api/ScheduledWorkUnits/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScheduledWorkUnit(string id)
        {
            var scheduledWorkUnit = await _context.ScheduledWorkUnits.FindAsync(id);
            if (scheduledWorkUnit == null)
            {
                return NotFound();
            }

            _context.ScheduledWorkUnits.Remove(scheduledWorkUnit);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ScheduledWorkUnitExists(string id)
        {
            return _context.ScheduledWorkUnits.Any(e => e.ScheduledWorkUnitID == id);
        }

        private bool TechnicianDailyScheduleExists(string id)
        {
            return _context.TechnicianDailySchedules.Any(e => e.DailyScheduleID == id);
        }

        private bool TimeSlotDefinitionExists(string id)
        {
            return _context.TimeSlotDefinitions.Any(e => e.SlotDefID == id);
        }

        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }
    }
}
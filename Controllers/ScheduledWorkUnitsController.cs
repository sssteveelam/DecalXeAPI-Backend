using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduledWorkUnitsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ScheduledWorkUnitsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: GET api/ScheduledWorkUnits
        // Lấy tất cả các ScheduledWorkUnit, bao gồm thông tin DailySchedule, TimeSlotDefinition và Order liên quan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ScheduledWorkUnit>>> GetScheduledWorkUnits()
        {
            return await _context.ScheduledWorkUnits
                                .Include(swu => swu.DailySchedule)
                                .Include(swu => swu.TimeSlotDefinition)
                                .Include(swu => swu.Order)
                                .ToListAsync();
        }

        // API: GET api/ScheduledWorkUnits/{id}
        // Lấy thông tin một ScheduledWorkUnit theo ScheduledWorkUnitID, bao gồm các thông tin liên quan
        [HttpGet("{id}")]
        public async Task<ActionResult<ScheduledWorkUnit>> GetScheduledWorkUnit(string id)
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

            return scheduledWorkUnit;
        }

        // API: POST api/ScheduledWorkUnits
        // Tạo một ScheduledWorkUnit mới
        [HttpPost]
        public async Task<ActionResult<ScheduledWorkUnit>> PostScheduledWorkUnit(ScheduledWorkUnit scheduledWorkUnit)
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
            if (!string.IsNullOrEmpty(scheduledWorkUnit.OrderID) && !OrderExists(scheduledWorkUnit.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }

            _context.ScheduledWorkUnits.Add(scheduledWorkUnit);
            await _context.SaveChangesAsync();

            // Tải lại thông tin liên quan để trả về đầy đủ
            await _context.Entry(scheduledWorkUnit).Reference(swu => swu.DailySchedule).LoadAsync();
            await _context.Entry(scheduledWorkUnit).Reference(swu => swu.TimeSlotDefinition).LoadAsync();
            await _context.Entry(scheduledWorkUnit).Reference(swu => swu.Order).LoadAsync();

            return CreatedAtAction(nameof(GetScheduledWorkUnit), new { id = scheduledWorkUnit.ScheduledWorkUnitID }, scheduledWorkUnit);
        }

        // API: PUT api/ScheduledWorkUnits/{id}
        // Cập nhật thông tin một ScheduledWorkUnit hiện có
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
        // Xóa một ScheduledWorkUnit
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

        // Hàm hỗ trợ: Kiểm tra xem ScheduledWorkUnit có tồn tại không
        private bool ScheduledWorkUnitExists(string id)
        {
            return _context.ScheduledWorkUnits.Any(e => e.ScheduledWorkUnitID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem TechnicianDailySchedule có tồn tại không (copy từ TechnicianDailySchedulesController)
        private bool TechnicianDailyScheduleExists(string id)
        {
            return _context.TechnicianDailySchedules.Any(e => e.DailyScheduleID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem TimeSlotDefinition có tồn tại không (copy từ TimeSlotDefinitionsController, sẽ tạo sau)
        private bool TimeSlotDefinitionExists(string id)
        {
            return _context.TimeSlotDefinitions.Any(e => e.SlotDefID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem Order có tồn tại không (copy từ OrdersController)
        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }
    }
}
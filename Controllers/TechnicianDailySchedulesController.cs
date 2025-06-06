using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using System;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TechnicianDailySchedulesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TechnicianDailySchedulesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: GET api/TechnicianDailySchedules
        // Lấy tất cả các TechnicianDailySchedule, bao gồm thông tin Employee liên quan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TechnicianDailySchedule>>> GetTechnicianDailySchedules()
        {
            return await _context.TechnicianDailySchedules.Include(tds => tds.Employee).ToListAsync();
        }

        // API: GET api/TechnicianDailySchedules/{id}
        // Lấy thông tin một TechnicianDailySchedule theo DailyScheduleID, bao gồm Employee liên quan
        [HttpGet("{id}")]
        public async Task<ActionResult<TechnicianDailySchedule>> GetTechnicianDailySchedule(string id)
        {
            var schedule = await _context.TechnicianDailySchedules.Include(tds => tds.Employee).FirstOrDefaultAsync(tds => tds.DailyScheduleID == id);

            if (schedule == null)
            {
                return NotFound();
            }

            return schedule;
        }

        // API: POST api/TechnicianDailySchedules
        // Tạo một TechnicianDailySchedule mới
        [HttpPost]
        public async Task<ActionResult<TechnicianDailySchedule>> PostTechnicianDailySchedule(TechnicianDailySchedule schedule)
        {
            // Kiểm tra xem EmployeeID có tồn tại không
            if (!string.IsNullOrEmpty(schedule.EmployeeID) && !EmployeeExists(schedule.EmployeeID))
            {
                return BadRequest("EmployeeID không tồn tại.");
            }

            _context.TechnicianDailySchedules.Add(schedule);
            await _context.SaveChangesAsync();

            // Tải lại thông tin Employee để trả về đầy đủ
            await _context.Entry(schedule).Reference(tds => tds.Employee).LoadAsync();

            return CreatedAtAction(nameof(GetTechnicianDailySchedule), new { id = schedule.DailyScheduleID }, schedule);
        }

        // API: PUT api/TechnicianDailySchedules/{id}
        // Cập nhật thông tin một TechnicianDailySchedule hiện có
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTechnicianDailySchedule(string id, TechnicianDailySchedule schedule)
        {
            if (id != schedule.DailyScheduleID)
            {
                return BadRequest();
            }

            // Kiểm tra EmployeeID trước khi cập nhật
            if (!string.IsNullOrEmpty(schedule.EmployeeID) && !EmployeeExists(schedule.EmployeeID))
            {
                return BadRequest("EmployeeID không tồn tại.");
            }

            _context.Entry(schedule).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TechnicianDailyScheduleExists(id))
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

        // API: DELETE api/TechnicianDailySchedules/{id}
        // Xóa một TechnicianDailySchedule
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTechnicianDailySchedule(string id)
        {
            var schedule = await _context.TechnicianDailySchedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            _context.TechnicianDailySchedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Hàm hỗ trợ: Kiểm tra xem TechnicianDailySchedule có tồn tại không
        private bool TechnicianDailyScheduleExists(string id)
        {
            return _context.TechnicianDailySchedules.Any(e => e.DailyScheduleID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem Employee có tồn tại không (copy từ EmployeesController)
        private bool EmployeeExists(string id)
        {
            return _context.Employees.Any(e => e.EmployeeID == id);
        }
    }
}
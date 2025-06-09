using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs; // Để sử dụng TechnicianDailyScheduleDto
using AutoMapper; // Để sử dụng AutoMapper
using System.Collections.Generic; // Để sử dụng IEnumerable
using System;
using Microsoft.AspNetCore.Authorization; // Để sử dụng DateTime

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Technician")]
    public class TechnicianDailySchedulesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Khai báo biến IMapper

        public TechnicianDailySchedulesController(ApplicationDbContext context, IMapper mapper) // Tiêm IMapper
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/TechnicianDailySchedules
        // Lấy tất cả các TechnicianDailySchedule, bao gồm thông tin Employee liên quan, trả về DTO
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TechnicianDailyScheduleDto>>> GetTechnicianDailySchedules() // Kiểu trả về là TechnicianDailyScheduleDto
        {
            var schedules = await _context.TechnicianDailySchedules.Include(tds => tds.Employee).ToListAsync();
            // Sử dụng AutoMapper để ánh xạ từ List<TechnicianDailySchedule> sang List<TechnicianDailyScheduleDto>
            var scheduleDtos = _mapper.Map<List<TechnicianDailyScheduleDto>>(schedules);
            return Ok(scheduleDtos);
        }

        // API: GET api/TechnicianDailySchedules/{id}
        // Lấy thông tin một TechnicianDailySchedule theo DailyScheduleID, bao gồm Employee liên quan, trả về DTO
        [HttpGet("{id}")]
        public async Task<ActionResult<TechnicianDailyScheduleDto>> GetTechnicianDailySchedule(string id) // Kiểu trả về là TechnicianDailyScheduleDto
        {
            var schedule = await _context.TechnicianDailySchedules.Include(tds => tds.Employee).FirstOrDefaultAsync(tds => tds.DailyScheduleID == id);

            if (schedule == null)
            {
                return NotFound();
            }

            // Sử dụng AutoMapper để ánh xạ từ TechnicianDailySchedule Model sang TechnicianDailyScheduleDto
            var scheduleDto = _mapper.Map<TechnicianDailyScheduleDto>(schedule);
            return Ok(scheduleDto);
        }

        // API: POST api/TechnicianDailySchedules
        // Tạo một TechnicianDailySchedule mới, nhận vào TechnicianDailySchedule Model, trả về TechnicianDailyScheduleDto sau khi tạo
        [HttpPost]
        public async Task<ActionResult<TechnicianDailyScheduleDto>> PostTechnicianDailySchedule(TechnicianDailySchedule schedule) // Kiểu trả về là TechnicianDailyScheduleDto
        {
            // Kiểm tra xem EmployeeID có tồn tại không
            if (!string.IsNullOrEmpty(schedule.EmployeeID) && !EmployeeExists(schedule.EmployeeID))
            {
                return BadRequest("EmployeeID không tồn tại.");
            }

            _context.TechnicianDailySchedules.Add(schedule);
            await _context.SaveChangesAsync();

            // Tải lại thông tin Employee để AutoMapper có thể ánh xạ EmployeeFullName
            await _context.Entry(schedule).Reference(tds => tds.Employee).LoadAsync();

            // Ánh xạ TechnicianDailySchedule Model vừa tạo sang TechnicianDailyScheduleDto để trả về
            var scheduleDto = _mapper.Map<TechnicianDailyScheduleDto>(schedule);
            return CreatedAtAction(nameof(GetTechnicianDailySchedule), new { id = scheduleDto.DailyScheduleID }, scheduleDto);
        }

        // API: PUT api/TechnicianDailySchedules/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTechnicianDailySchedule(string id, TechnicianDailySchedule schedule)
        {
            if (id != schedule.DailyScheduleID)
            {
                return BadRequest();
            }

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

        private bool TechnicianDailyScheduleExists(string id)
        {
            return _context.TechnicianDailySchedules.Any(e => e.DailyScheduleID == id);
        }

        private bool EmployeeExists(string id)
        {
            return _context.Employees.Any(e => e.EmployeeID == id);
        }
    }
}
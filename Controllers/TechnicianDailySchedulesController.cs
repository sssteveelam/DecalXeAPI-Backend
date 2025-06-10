using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data; // Vẫn cần DbContext cho các hàm Exists cơ bản
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // <-- THÊM DÒNG NÀY (Để sử dụng ITechnicianDailyScheduleService)
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System; // Để sử dụng ArgumentException

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Technician")]
    public class TechnicianDailySchedulesController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists cơ bản
        private readonly ITechnicianDailyScheduleService _technicianDailyScheduleService; // <-- KHAI BÁO BIẾN CHO SERVICE
        private readonly IMapper _mapper;
        private readonly ILogger<TechnicianDailySchedulesController> _logger;

        public TechnicianDailySchedulesController(ApplicationDbContext context, ITechnicianDailyScheduleService technicianDailyScheduleService, IMapper mapper, ILogger<TechnicianDailySchedulesController> logger) // <-- TIÊM ITechnicianDailyScheduleService
        {
            _context = context;
            _technicianDailyScheduleService = technicianDailyScheduleService;
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/TechnicianDailySchedules
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public async Task<ActionResult<IEnumerable<TechnicianDailyScheduleDto>>> GetTechnicianDailySchedules()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách lịch làm việc hàng ngày của kỹ thuật viên.");
            var schedules = await _technicianDailyScheduleService.GetTechnicianDailySchedulesAsync();
            return Ok(schedules);
        }

        // API: GET api/TechnicianDailySchedules/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public async Task<ActionResult<TechnicianDailyScheduleDto>> GetTechnicianDailySchedule(string id)
        {
            _logger.LogInformation("Yêu cầu lấy lịch làm việc hàng ngày với ID: {DailyScheduleID}", id);
            var scheduleDto = await _technicianDailyScheduleService.GetTechnicianDailyScheduleByIdAsync(id);

            if (scheduleDto == null)
            {
                _logger.LogWarning("Không tìm thấy lịch làm việc hàng ngày với ID: {DailyScheduleID}", id);
                return NotFound();
            }

            return Ok(scheduleDto);
        }

        // API: POST api/TechnicianDailySchedules
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<TechnicianDailyScheduleDto>> PostTechnicianDailySchedule(TechnicianDailySchedule schedule) // Vẫn nhận Model
        {
            _logger.LogInformation("Yêu cầu tạo lịch làm việc hàng ngày mới cho EmployeeID: {EmployeeID}", schedule.EmployeeID);

            // --- KIỂM TRA FKs CHÍNH TRƯỚC KHI GỬI VÀO SERVICE ---
            if (!string.IsNullOrEmpty(schedule.EmployeeID) && !EmployeeExists(schedule.EmployeeID))
            {
                return BadRequest("EmployeeID không tồn tại.");
            }

            try
            {
                var createdScheduleDto = await _technicianDailyScheduleService.CreateTechnicianDailyScheduleAsync(schedule);
                _logger.LogInformation("Đã tạo lịch làm việc hàng ngày mới với ID: {DailyScheduleID}", createdScheduleDto.DailyScheduleID);
                return CreatedAtAction(nameof(GetTechnicianDailySchedule), new { id = createdScheduleDto.DailyScheduleID }, createdScheduleDto);
            }
            catch (ArgumentException ex) // Bắt lỗi từ Service nếu FK không hợp lệ
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi tạo lịch làm việc hàng ngày: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/TechnicianDailySchedules/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public async Task<IActionResult> PutTechnicianDailySchedule(string id, TechnicianDailySchedule schedule)
        {
            _logger.LogInformation("Yêu cầu cập nhật lịch làm việc hàng ngày với ID: {DailyScheduleID}", id);
            if (id != schedule.DailyScheduleID)
            {
                return BadRequest();
            }

            // Kiểm tra FKs chính
            if (!string.IsNullOrEmpty(schedule.EmployeeID) && !EmployeeExists(schedule.EmployeeID))
            {
                return BadRequest("EmployeeID không tồn tại.");
            }

            try
            {
                var success = await _technicianDailyScheduleService.UpdateTechnicianDailyScheduleAsync(id, schedule);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy lịch làm việc hàng ngày để cập nhật với ID: {DailyScheduleID}", id);
                    return NotFound();
                }

                _logger.LogInformation("Đã cập nhật lịch làm việc hàng ngày với ID: {DailyScheduleID}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi cập nhật lịch làm việc hàng ngày: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
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
        }

        // API: DELETE api/TechnicianDailySchedules/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteTechnicianDailySchedule(string id)
        {
            _logger.LogInformation("Yêu cầu xóa lịch làm việc hàng ngày với ID: {DailyScheduleID}", id);
            var success = await _technicianDailyScheduleService.DeleteTechnicianDailyScheduleAsync(id);

            if (!success)
            {
                _logger.LogWarning("Không tìm thấy lịch làm việc hàng ngày để xóa với ID: {DailyScheduleID}", id);
                return NotFound();
            }

            return NoContent();
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool TechnicianDailyScheduleExists(string id) { return _context.TechnicianDailySchedules.Any(e => e.DailyScheduleID == id); }
        private bool EmployeeExists(string id) { return _context.Employees.Any(e => e.EmployeeID == id); }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Vẫn cần cho các hàm Exists
using DecalXeAPI.Data; // Vẫn cần ApplicationDbContext cho các hàm Exists
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // <-- THÊM DÒNG NÀY (Để sử dụng IScheduledWorkUnitService)
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ScheduledWorkUnitsController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists cơ bản
        private readonly IScheduledWorkUnitService _scheduledWorkUnitService; // <-- KHAI BÁO BIẾN CHO SERVICE
        private readonly IMapper _mapper; // Vẫn giữ để ánh xạ DTOs nếu có
        private readonly ILogger<ScheduledWorkUnitsController> _logger; // Logger cho Controller

        public ScheduledWorkUnitsController(ApplicationDbContext context, IScheduledWorkUnitService scheduledWorkUnitService, IMapper mapper, ILogger<ScheduledWorkUnitsController> logger) // <-- TIÊM IScheduledWorkUnitService
        {
            _context = context; // Để dùng các hàm hỗ trợ
            _scheduledWorkUnitService = scheduledWorkUnitService; // Gán Service
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/ScheduledWorkUnits
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public async Task<ActionResult<IEnumerable<ScheduledWorkUnitDto>>> GetScheduledWorkUnits()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách đơn vị công việc đã lên lịch.");
            // Ủy quyền logic cho Service Layer
            var scheduledWorkUnits = await _scheduledWorkUnitService.GetScheduledWorkUnitsAsync();
            return Ok(scheduledWorkUnits);
        }

        // API: GET api/ScheduledWorkUnits/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public async Task<ActionResult<ScheduledWorkUnitDto>> GetScheduledWorkUnit(string id)
        {
            _logger.LogInformation("Yêu cầu lấy đơn vị công việc đã lên lịch với ID: {ScheduledWorkUnitID}", id);
            // Ủy quyền logic cho Service Layer
            var scheduledWorkUnitDto = await _scheduledWorkUnitService.GetScheduledWorkUnitByIdAsync(id);

            if (scheduledWorkUnitDto == null)
            {
                return NotFound();
            }

            return Ok(scheduledWorkUnitDto);
        }

        // API: POST api/ScheduledWorkUnits
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")] // Chỉ Admin, Manager có quyền tạo/gán lịch
        public async Task<ActionResult<ScheduledWorkUnitDto>> PostScheduledWorkUnit(ScheduledWorkUnit scheduledWorkUnit) // Vẫn nhận ScheduledWorkUnit Model
        {
            _logger.LogInformation("Yêu cầu tạo đơn vị công việc đã lên lịch mới cho DailyScheduleID: {DailyScheduleID}", scheduledWorkUnit.DailyScheduleID);

            // --- KIỂM TRA FKs CHÍNH TRƯỚC KHI GỬI VÀO SERVICE ---
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

            // Ủy quyền logic tạo ScheduledWorkUnit cho Service Layer
            var (createdDto, errorMessage) = await _scheduledWorkUnitService.CreateScheduledWorkUnitAsync(scheduledWorkUnit);

            if (createdDto == null)
            {
                _logger.LogError("Lỗi khi tạo đơn vị công việc đã lên lịch: {ErrorMessage}", errorMessage);
                return BadRequest(errorMessage); // Trả về lỗi từ Service
            }

            _logger.LogInformation("Đã tạo đơn vị công việc đã lên lịch mới với ID: {ScheduledWorkUnitID}", createdDto.ScheduledWorkUnitID);
            return CreatedAtAction(nameof(GetScheduledWorkUnit), new { id = createdDto.ScheduledWorkUnitID }, createdDto);
        }

        // API: PUT api/ScheduledWorkUnits/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Technician")] // Admin, Manager có thể thay đổi bất kỳ, Technician chỉ của mình
        public async Task<IActionResult> PutScheduledWorkUnit(string id, ScheduledWorkUnit scheduledWorkUnit)
        {
            _logger.LogInformation("Yêu cầu cập nhật đơn vị công việc đã lên lịch với ID: {ScheduledWorkUnitID}", id);

            // Kiểm tra FKs chính
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

            // Ủy quyền logic cập nhật ScheduledWorkUnit cho Service Layer
            var (success, errorMessage) = await _scheduledWorkUnitService.UpdateScheduledWorkUnitAsync(id, scheduledWorkUnit);

            if (!success)
            {
                if (errorMessage == "Đơn vị công việc đã lên lịch không tồn tại.")
                {
                    _logger.LogWarning("Không tìm thấy đơn vị công việc đã lên lịch để cập nhật với ID: {ScheduledWorkUnitID}", id);
                    return NotFound(errorMessage);
                }
                else
                {
                    _logger.LogError("Lỗi khi cập nhật đơn vị công việc đã lên lịch: {ErrorMessage}", errorMessage);
                    return BadRequest(errorMessage); // Trả về lỗi từ Service
                }
            }

            _logger.LogInformation("Đã cập nhật đơn vị công việc đã lên lịch với ID: {ScheduledWorkUnitID}", id);
            return NoContent();
        }

        // API: DELETE api/ScheduledWorkUnits/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")] // Chỉ Admin, Manager có quyền xóa
        public async Task<IActionResult> DeleteScheduledWorkUnit(string id)
        {
            _logger.LogInformation("Yêu cầu xóa đơn vị công việc đã lên lịch với ID: {ScheduledWorkUnitID}", id);

            // Ủy quyền logic xóa cho Service Layer
            var (success, errorMessage) = await _scheduledWorkUnitService.DeleteScheduledWorkUnitAsync(id);

            if (!success)
            {
                _logger.LogWarning("Không tìm thấy đơn vị công việc đã lên lịch để xóa với ID: {ScheduledWorkUnitID}", id);
                return NotFound(errorMessage);
            }

            _logger.LogInformation("Đã xóa đơn vị công việc đã lên lịch với ID: {ScheduledWorkUnitID}", id);
            return NoContent();
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool TechnicianDailyScheduleExists(string id) { return _context.TechnicianDailySchedules.Any(e => e.DailyScheduleID == id); }
        private bool TimeSlotDefinitionExists(string id) { return _context.TimeSlotDefinitions.Any(e => e.SlotDefID == id); }
        private bool OrderExists(string id) { return _context.Orders.Any(e => e.OrderID == id); }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data; // Vẫn cần DbContext cho các hàm Exists cơ bản
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // <-- THÊM DÒNG NÀY (Để sử dụng ITimeSlotDefinitionService)
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System; // Để sử dụng ArgumentException

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")] // Quyền cho TimeSlotDefinitionsController
    public class TimeSlotDefinitionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists cơ bản
        private readonly ITimeSlotDefinitionService _timeSlotDefinitionService; // <-- KHAI BÁO BIẾN CHO SERVICE
        private readonly IMapper _mapper;
        private readonly ILogger<TimeSlotDefinitionsController> _logger;

        public TimeSlotDefinitionsController(ApplicationDbContext context, ITimeSlotDefinitionService timeSlotDefinitionService, IMapper mapper, ILogger<TimeSlotDefinitionsController> logger) // <-- TIÊM ITimeSlotDefinitionService
        {
            _context = context;
            _timeSlotDefinitionService = timeSlotDefinitionService;
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/TimeSlotDefinitions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TimeSlotDefinitionDto>>> GetTimeSlotDefinitions()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách định nghĩa khung giờ.");
            var timeSlotDefinitions = await _timeSlotDefinitionService.GetTimeSlotDefinitionsAsync();
            return Ok(timeSlotDefinitions);
        }

        // API: GET api/TimeSlotDefinitions/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TimeSlotDefinitionDto>> GetTimeSlotDefinition(string id)
        {
            _logger.LogInformation("Yêu cầu lấy định nghĩa khung giờ với ID: {SlotDefID}", id);
            var timeSlotDefinitionDto = await _timeSlotDefinitionService.GetTimeSlotDefinitionByIdAsync(id);

            if (timeSlotDefinitionDto == null)
            {
                _logger.LogWarning("Không tìm thấy định nghĩa khung giờ với ID: {SlotDefID}", id);
                return NotFound();
            }

            return Ok(timeSlotDefinitionDto);
        }

        // API: POST api/TimeSlotDefinitions
        [HttpPost]
        public async Task<ActionResult<TimeSlotDefinitionDto>> PostTimeSlotDefinition(TimeSlotDefinition timeSlotDefinition) // Vẫn nhận Model
        {
            _logger.LogInformation("Yêu cầu tạo định nghĩa khung giờ mới: {StartTime} - {EndTime}", timeSlotDefinition.StartTime, timeSlotDefinition.EndTime);
            try
            {
                var createdTimeSlotDefinitionDto = await _timeSlotDefinitionService.CreateTimeSlotDefinitionAsync(timeSlotDefinition);
                _logger.LogInformation("Đã tạo định nghĩa khung giờ mới với ID: {SlotDefID}", createdTimeSlotDefinitionDto.SlotDefID);
                return CreatedAtAction(nameof(GetTimeSlotDefinition), new { id = createdTimeSlotDefinitionDto.SlotDefID }, createdTimeSlotDefinitionDto);
            }
            catch (ArgumentException ex) // Bắt lỗi từ Service nếu có
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi tạo định nghĩa khung giờ: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/TimeSlotDefinitions/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTimeSlotDefinition(string id, TimeSlotDefinition timeSlotDefinition)
        {
            _logger.LogInformation("Yêu cầu cập nhật định nghĩa khung giờ với ID: {SlotDefID}", id);
            if (id != timeSlotDefinition.SlotDefID)
            {
                return BadRequest();
            }

            try
            {
                var success = await _timeSlotDefinitionService.UpdateTimeSlotDefinitionAsync(id, timeSlotDefinition);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy định nghĩa khung giờ để cập nhật với ID: {SlotDefID}", id);
                    return NotFound();
                }

                _logger.LogInformation("Đã cập nhật định nghĩa khung giờ với ID: {SlotDefID}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi cập nhật định nghĩa khung giờ: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
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
        }

        // API: DELETE api/TimeSlotDefinitions/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTimeSlotDefinition(string id)
        {
            _logger.LogInformation("Yêu cầu xóa định nghĩa khung giờ với ID: {SlotDefID}", id);
            var success = await _timeSlotDefinitionService.DeleteTimeSlotDefinitionAsync(id);

            if (!success)
            {
                _logger.LogWarning("Không tìm thấy định nghĩa khung giờ để xóa với ID: {SlotDefID}", id);
                return NotFound();
            }

            return NoContent();
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool TimeSlotDefinitionExists(string id) { return _context.TimeSlotDefinitions.Any(e => e.SlotDefID == id); }
    }
}
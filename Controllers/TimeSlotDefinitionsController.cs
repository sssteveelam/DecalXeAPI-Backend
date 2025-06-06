using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using System;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeSlotDefinitionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TimeSlotDefinitionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: GET api/TimeSlotDefinitions
        // Lấy tất cả các TimeSlotDefinition có trong database
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TimeSlotDefinition>>> GetTimeSlotDefinitions()
        {
            return await _context.TimeSlotDefinitions.ToListAsync();
        }

        // API: GET api/TimeSlotDefinitions/{id}
        // Lấy thông tin một TimeSlotDefinition theo SlotDefID
        [HttpGet("{id}")]
        public async Task<ActionResult<TimeSlotDefinition>> GetTimeSlotDefinition(string id)
        {
            var timeSlotDefinition = await _context.TimeSlotDefinitions.FindAsync(id);

            if (timeSlotDefinition == null)
            {
                return NotFound(); // Trả về lỗi 404 Not Found
            }

            return timeSlotDefinition; // Trả về TimeSlotDefinition tìm được
        }

        // API: POST api/TimeSlotDefinitions
        // Tạo một TimeSlotDefinition mới
        [HttpPost]
        public async Task<ActionResult<TimeSlotDefinition>> PostTimeSlotDefinition(TimeSlotDefinition timeSlotDefinition)
        {
            _context.TimeSlotDefinitions.Add(timeSlotDefinition); // Thêm TimeSlotDefinition mới vào DbSet
            await _context.SaveChangesAsync(); // Lưu các thay đổi vào database

            // Trả về kết quả 201 Created và thông tin của TimeSlotDefinition vừa tạo
            return CreatedAtAction(nameof(GetTimeSlotDefinition), new { id = timeSlotDefinition.SlotDefID }, timeSlotDefinition);
        }

        // API: PUT api/TimeSlotDefinitions/{id}
        // Cập nhật thông tin một TimeSlotDefinition hiện có
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTimeSlotDefinition(string id, TimeSlotDefinition timeSlotDefinition)
        {
            // Kiểm tra xem ID trong đường dẫn có khớp với SlotDefID trong body request không
            if (id != timeSlotDefinition.SlotDefID)
            {
                return BadRequest(); // Trả về lỗi 400 Bad Request nếu không khớp
            }

            _context.Entry(timeSlotDefinition).State = EntityState.Modified; // Đánh dấu Entity là đã được Modified

            try
            {
                await _context.SaveChangesAsync(); // Lưu các thay đổi vào database
            }
            catch (DbUpdateConcurrencyException) // Xử lý lỗi nếu có xung đột cập nhật
            {
                if (!TimeSlotDefinitionExists(id)) // Kiểm tra xem TimeSlotDefinition có tồn tại không
                {
                    return NotFound(); // Nếu không tồn tại, trả về 404 Not Found
                }
                else
                {
                    throw; // Nếu là lỗi khác, ném lại lỗi
                }
            }

            return NoContent(); // Trả về 204 No Content (cập nhật thành công nhưng không có nội dung trả về)
        }

        // API: DELETE api/TimeSlotDefinitions/{id}
        // Xóa một TimeSlotDefinition
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTimeSlotDefinition(string id)
        {
            var timeSlotDefinition = await _context.TimeSlotDefinitions.FindAsync(id); // Tìm TimeSlotDefinition cần xóa
            if (timeSlotDefinition == null)
            {
                return NotFound(); // Nếu không tìm thấy
            }

            _context.TimeSlotDefinitions.Remove(timeSlotDefinition); // Xóa TimeSlotDefinition khỏi DbSet
            await _context.SaveChangesAsync(); // Lưu thay đổi vào database

            return NoContent(); // Trả về 204 No Content
        }

        // Hàm hỗ trợ: Kiểm tra xem TimeSlotDefinition có tồn tại không
        private bool TimeSlotDefinitionExists(string id)
        {
            return _context.TimeSlotDefinitions.Any(e => e.SlotDefID == id);
        }
    }
}
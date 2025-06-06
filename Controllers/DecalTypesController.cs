using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DecalTypesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DecalTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: GET api/DecalTypes
        // Lấy tất cả các DecalType có trong database
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DecalType>>> GetDecalTypes()
        {
            return await _context.DecalTypes.ToListAsync();
        }

        // API: GET api/DecalTypes/{id}
        // Lấy thông tin một DecalType theo DecalTypeID
        [HttpGet("{id}")]
        public async Task<ActionResult<DecalType>> GetDecalType(string id)
        {
            var decalType = await _context.DecalTypes.FindAsync(id);

            if (decalType == null)
            {
                return NotFound(); // Trả về lỗi 404 Not Found
            }

            return decalType; // Trả về DecalType tìm được
        }

        // API: POST api/DecalTypes
        // Tạo một DecalType mới
        [HttpPost]
        public async Task<ActionResult<DecalType>> PostDecalType(DecalType decalType)
        {
            _context.DecalTypes.Add(decalType); // Thêm DecalType mới vào DbSet
            await _context.SaveChangesAsync(); // Lưu các thay đổi vào database

            // Trả về kết quả 201 Created và thông tin của DecalType vừa tạo
            return CreatedAtAction(nameof(GetDecalType), new { id = decalType.DecalTypeID }, decalType);
        }

        // API: PUT api/DecalTypes/{id}
        // Cập nhật thông tin một DecalType hiện có
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDecalType(string id, DecalType decalType)
        {
            // Kiểm tra xem ID trong đường dẫn có khớp với DecalTypeID trong body request không
            if (id != decalType.DecalTypeID)
            {
                return BadRequest(); // Trả về lỗi 400 Bad Request nếu không khớp
            }

            _context.Entry(decalType).State = EntityState.Modified; // Đánh dấu Entity là đã được Modified

            try
            {
                await _context.SaveChangesAsync(); // Lưu các thay đổi vào database
            }
            catch (DbUpdateConcurrencyException) // Xử lý lỗi nếu có xung đột cập nhật
            {
                if (!DecalTypeExists(id)) // Kiểm tra xem DecalType có tồn tại không
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

        // API: DELETE api/DecalTypes/{id}
        // Xóa một DecalType
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDecalType(string id)
        {
            var decalType = await _context.DecalTypes.FindAsync(id); // Tìm DecalType cần xóa
            if (decalType == null)
            {
                return NotFound(); // Nếu không tìm thấy
            }

            _context.DecalTypes.Remove(decalType); // Xóa DecalType khỏi DbSet
            await _context.SaveChangesAsync(); // Lưu thay đổi vào database

            return NoContent(); // Trả về 204 No Content
        }

        // Hàm hỗ trợ: Kiểm tra xem DecalType có tồn tại không
        private bool DecalTypeExists(string id)
        {
            return _context.DecalTypes.Any(e => e.DecalTypeID == id);
        }
    }
}
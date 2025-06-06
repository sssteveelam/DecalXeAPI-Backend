using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using System;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PromotionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: GET api/Promotions
        // Lấy tất cả các Promotion có trong database
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Promotion>>> GetPromotions()
        {
            return await _context.Promotions.ToListAsync();
        }

        // API: GET api/Promotions/{id}
        // Lấy thông tin một Promotion theo PromotionID
        [HttpGet("{id}")]
        public async Task<ActionResult<Promotion>> GetPromotion(string id)
        {
            var promotion = await _context.Promotions.FindAsync(id);

            if (promotion == null)
            {
                return NotFound(); // Trả về lỗi 404 Not Found
            }

            return promotion; // Trả về Promotion tìm được
        }

        // API: POST api/Promotions
        // Tạo một Promotion mới
        [HttpPost]
        public async Task<ActionResult<Promotion>> PostPromotion(Promotion promotion)
        {
            _context.Promotions.Add(promotion); // Thêm Promotion mới vào DbSet
            await _context.SaveChangesAsync(); // Lưu các thay đổi vào database

            // Trả về kết quả 201 Created và thông tin của Promotion vừa tạo
            return CreatedAtAction(nameof(GetPromotion), new { id = promotion.PromotionID }, promotion);
        }

        // API: PUT api/Promotions/{id}
        // Cập nhật thông tin một Promotion hiện có
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPromotion(string id, Promotion promotion)
        {
            // Kiểm tra xem ID trong đường dẫn có khớp với PromotionID trong body request không
            if (id != promotion.PromotionID)
            {
                return BadRequest(); // Trả về lỗi 400 Bad Request nếu không khớp
            }

            _context.Entry(promotion).State = EntityState.Modified; // Đánh dấu Entity là đã được Modified

            try
            {
                await _context.SaveChangesAsync(); // Lưu các thay đổi vào database
            }
            catch (DbUpdateConcurrencyException) // Xử lý lỗi nếu có xung đột cập nhật
            {
                if (!PromotionExists(id)) // Kiểm tra xem Promotion có tồn tại không
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

        // API: DELETE api/Promotions/{id}
        // Xóa một Promotion
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePromotion(string id)
        {
            var promotion = await _context.Promotions.FindAsync(id); // Tìm Promotion cần xóa
            if (promotion == null)
            {
                return NotFound(); // Nếu không tìm thấy
            }

            _context.Promotions.Remove(promotion); // Xóa Promotion khỏi DbSet
            await _context.SaveChangesAsync(); // Lưu thay đổi vào database

            return NoContent(); // Trả về 204 No Content
        }

        // Hàm hỗ trợ: Kiểm tra xem Promotion có tồn tại không
        private bool PromotionExists(string id)
        {
            return _context.Promotions.Any(e => e.PromotionID == id);
        }
    }
}
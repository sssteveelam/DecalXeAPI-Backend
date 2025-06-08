using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs; // Để sử dụng PromotionDto
using AutoMapper; // Để sử dụng AutoMapper
using System.Collections.Generic; // Để sử dụng IEnumerable
using System; // Để sử dụng DateTime

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Khai báo biến IMapper

        public PromotionsController(ApplicationDbContext context, IMapper mapper) // Tiêm IMapper
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/Promotions
        // Lấy tất cả các Promotion, trả về dưới dạng PromotionDto
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PromotionDto>>> GetPromotions() // Kiểu trả về là PromotionDto
        {
            var promotions = await _context.Promotions.ToListAsync();
            // Sử dụng AutoMapper để ánh xạ từ List<Promotion> sang List<PromotionDto>
            var promotionDtos = _mapper.Map<List<PromotionDto>>(promotions);
            return Ok(promotionDtos);
        }

        // API: GET api/Promotions/{id}
        // Lấy thông tin một Promotion theo PromotionID, trả về dưới dạng PromotionDto
        [HttpGet("{id}")]
        public async Task<ActionResult<PromotionDto>> GetPromotion(string id) // Kiểu trả về là PromotionDto
        {
            var promotion = await _context.Promotions.FindAsync(id);

            if (promotion == null)
            {
                return NotFound();
            }

            // Sử dụng AutoMapper để ánh xạ từ Promotion Model sang PromotionDto
            var promotionDto = _mapper.Map<PromotionDto>(promotion);
            return Ok(promotionDto);
        }

        // API: POST api/Promotions
        // Tạo một Promotion mới, nhận vào Promotion Model, trả về PromotionDto sau khi tạo
        [HttpPost]
        public async Task<ActionResult<PromotionDto>> PostPromotion(Promotion promotion) // Kiểu trả về là PromotionDto
        {
            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();

            // Không cần LoadAsync() vì PromotionDto không có Navigation Property cần tải

            // Ánh xạ Promotion Model vừa tạo sang PromotionDto để trả về
            var promotionDto = _mapper.Map<PromotionDto>(promotion);
            return CreatedAtAction(nameof(GetPromotion), new { id = promotionDto.PromotionID }, promotionDto);
        }

        // API: PUT api/Promotions/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPromotion(string id, Promotion promotion)
        {
            if (id != promotion.PromotionID)
            {
                return BadRequest();
            }

            _context.Entry(promotion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PromotionExists(id))
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

        // API: DELETE api/Promotions/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePromotion(string id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }

            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PromotionExists(string id)
        {
            return _context.Promotions.Any(e => e.PromotionID == id);
        }
    }
}
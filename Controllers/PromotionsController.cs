using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // <-- THÊM DÒNG NÀY (Để sử dụng IPromotionService)
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Sales")] // Quyền cho PromotionsController
    public class PromotionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists cơ bản
        private readonly IPromotionService _promotionService; // <-- KHAI BÁO BIẾN CHO SERVICE
        private readonly IMapper _mapper;
        private readonly ILogger<PromotionsController> _logger;

        public PromotionsController(ApplicationDbContext context, IPromotionService promotionService, IMapper mapper, ILogger<PromotionsController> logger) // <-- TIÊM IPromotionService
        {
            _context = context;
            _promotionService = promotionService;
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/Promotions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PromotionDto>>> GetPromotions()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách khuyến mãi.");
            var promotions = await _promotionService.GetPromotionsAsync();
            return Ok(promotions);
        }

        // API: GET api/Promotions/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PromotionDto>> GetPromotion(string id)
        {
            _logger.LogInformation("Yêu cầu lấy khuyến mãi với ID: {PromotionID}", id);
            var promotionDto = await _promotionService.GetPromotionByIdAsync(id);

            if (promotionDto == null)
            {
                _logger.LogWarning("Không tìm thấy khuyến mãi với ID: {PromotionID}", id);
                return NotFound();
            }

            return Ok(promotionDto);
        }

        // API: POST api/Promotions
        [HttpPost]
        public async Task<ActionResult<PromotionDto>> PostPromotion(Promotion promotion) // Vẫn nhận Model
        {
            _logger.LogInformation("Yêu cầu tạo khuyến mãi mới: {PromotionName}", promotion.PromotionName);
            try
            {
                var createdPromotionDto = await _promotionService.CreatePromotionAsync(promotion);
                _logger.LogInformation("Đã tạo khuyến mãi mới với ID: {PromotionID}", createdPromotionDto.PromotionID);
                return CreatedAtAction(nameof(GetPromotion), new { id = createdPromotionDto.PromotionID }, createdPromotionDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi tạo khuyến mãi: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/Promotions/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPromotion(string id, Promotion promotion)
        {
            _logger.LogInformation("Yêu cầu cập nhật khuyến mãi với ID: {PromotionID}", id);
            if (id != promotion.PromotionID)
            {
                return BadRequest();
            }

            try
            {
                var success = await _promotionService.UpdatePromotionAsync(id, promotion);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy khuyến mãi để cập nhật với ID: {PromotionID}", id);
                    return NotFound();
                }

                _logger.LogInformation("Đã cập nhật khuyến mãi với ID: {PromotionID}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi cập nhật khuyến mãi: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
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
        }

        // API: DELETE api/Promotions/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePromotion(string id)
        {
            _logger.LogInformation("Yêu cầu xóa khuyến mãi với ID: {PromotionID}", id);
            var success = await _promotionService.DeletePromotionAsync(id);

            if (!success)
            {
                _logger.LogWarning("Không tìm thấy khuyến mãi để xóa với ID: {PromotionID}", id);
                return NotFound();
            }

            return NoContent();
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool PromotionExists(string id) { return _context.Promotions.Any(e => e.PromotionID == id); }
    }
}
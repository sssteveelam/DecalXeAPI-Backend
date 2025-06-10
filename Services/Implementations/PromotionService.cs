using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace DecalXeAPI.Services.Implementations
{
    public class PromotionService : IPromotionService // <-- Kế thừa từ IPromotionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PromotionService> _logger;

        public PromotionService(ApplicationDbContext context, IMapper mapper, ILogger<PromotionService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<PromotionDto>> GetPromotionsAsync()
        {
            _logger.LogInformation("Lấy danh sách khuyến mãi.");
            var promotions = await _context.Promotions.ToListAsync();
            var promotionDtos = _mapper.Map<List<PromotionDto>>(promotions);
            return promotionDtos;
        }

        public async Task<PromotionDto?> GetPromotionByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy khuyến mãi với ID: {PromotionID}", id);
            var promotion = await _context.Promotions.FindAsync(id);

            if (promotion == null)
            {
                _logger.LogWarning("Không tìm thấy khuyến mãi với ID: {PromotionID}", id);
                return null;
            }

            var promotionDto = _mapper.Map<PromotionDto>(promotion);
            _logger.LogInformation("Đã trả về khuyến mãi với ID: {PromotionID}", id);
            return promotionDto;
        }

        public async Task<PromotionDto> CreatePromotionAsync(Promotion promotion)
        {
            _logger.LogInformation("Yêu cầu tạo khuyến mãi mới: {PromotionName}", promotion.PromotionName);
            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();

            var promotionDto = _mapper.Map<PromotionDto>(promotion);
            _logger.LogInformation("Đã tạo khuyến mãi mới với ID: {PromotionID}", promotion.PromotionID);
            return promotionDto;
        }

        public async Task<bool> UpdatePromotionAsync(string id, Promotion promotion)
        {
            _logger.LogInformation("Yêu cầu cập nhật khuyến mãi với ID: {PromotionID}", id);

            if (id != promotion.PromotionID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với PromotionID trong body ({PromotionIDBody})", id, promotion.PromotionID);
                return false;
            }

            if (!await PromotionExistsAsync(id))
            {
                _logger.LogWarning("Không tìm thấy khuyến mãi để cập nhật với ID: {PromotionID}", id);
                return false;
            }

            _context.Entry(promotion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật khuyến mãi với ID: {PromotionID}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật khuyến mãi với ID: {PromotionID}", id);
                throw;
            }
        }

        public async Task<bool> DeletePromotionAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa khuyến mãi với ID: {PromotionID}", id);
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
            {
                _logger.LogWarning("Không tìm thấy khuyến mãi để xóa với ID: {PromotionID}", id);
                return false;
            }

            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa khuyến mãi với ID: {PromotionID}", id);
            return true;
        }

        // Hàm kiểm tra tồn tại (PUBLIC cho INTERFACE)
        public async Task<bool> PromotionExistsAsync(string id)
        {
            return await _context.Promotions.AnyAsync(e => e.PromotionID == id);
        }
    }
}
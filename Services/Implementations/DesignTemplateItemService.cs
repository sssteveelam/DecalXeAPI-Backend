using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces;
using DecalXeAPI.Services.Helpers;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace DecalXeAPI.Services.Implementations
{
    public class DesignTemplateItemService : IDesignTemplateItemService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DesignTemplateItemService> _logger;

        public DesignTemplateItemService(ApplicationDbContext context, IMapper mapper, ILogger<DesignTemplateItemService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<DesignTemplateItemDto>> GetAllAsync()
        {
            _logger.LogInformation("Lấy danh sách tất cả design template items.");
            var items = await _context.DesignTemplateItems
                .Include(dti => dti.Design)
                .OrderBy(dti => dti.DesignId)
                .ThenBy(dti => dti.DisplayOrder)
                .ToListAsync();
            
            return items.Select(MapToDto);
        }

        public async Task<DesignTemplateItemDto?> GetByIdAsync(string id)
        {
            _logger.LogInformation("Lấy design template item với ID: {ItemID}", id);
            var item = await _context.DesignTemplateItems
                .Include(dti => dti.Design)
                .FirstOrDefaultAsync(dti => dti.Id == id);

            if (item == null)
            {
                _logger.LogWarning("Không tìm thấy design template item với ID: {ItemID}", id);
                return null;
            }

            return MapToDto(item);
        }

        public async Task<IEnumerable<DesignTemplateItemDto>> GetByDesignIdAsync(string designId)
        {
            _logger.LogInformation("Lấy danh sách template items cho design: {DesignID}", designId);
            var items = await _context.DesignTemplateItems
                .Include(dti => dti.Design)
                .Where(dti => dti.DesignId == designId)
                .OrderBy(dti => dti.DisplayOrder)
                .ToListAsync();

            return items.Select(MapToDto);
        }

        public async Task<IEnumerable<DesignTemplateItemDto>> GetByPlacementPositionAsync(VehiclePart placementPosition)
        {
            _logger.LogInformation("Lấy danh sách template items theo vị trí: {PlacementPosition}", placementPosition);
            var items = await _context.DesignTemplateItems
                .Include(dti => dti.Design)
                .Where(dti => dti.PlacementPosition == placementPosition)
                .OrderBy(dti => dti.DesignId)
                .ThenBy(dti => dti.DisplayOrder)
                .ToListAsync();

            return items.Select(MapToDto);
        }

        public async Task<DesignTemplateItemDto> CreateAsync(CreateDesignTemplateItemDto createDto)
        {
            _logger.LogInformation("Tạo design template item mới cho design: {DesignID}", createDto.DesignId);
            
            // Kiểm tra design có tồn tại không
            if (!await DesignExistsAsync(createDto.DesignId))
            {
                throw new InvalidOperationException($"Design với ID {createDto.DesignId} không tồn tại.");
            }

            var item = _mapper.Map<DesignTemplateItem>(createDto);
            item.Id = Guid.NewGuid().ToString();
            item.CreatedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;

            // Nếu DisplayOrder không được set, tự động tính toán
            if (item.DisplayOrder == 0)
            {
                item.DisplayOrder = await GetNextDisplayOrderForDesignAsync(createDto.DesignId);
            }

            _context.DesignTemplateItems.Add(item);
            await _context.SaveChangesAsync();

            // Lấy lại với thông tin đầy đủ
            var createdItem = await _context.DesignTemplateItems
                .Include(dti => dti.Design)
                .FirstOrDefaultAsync(dti => dti.Id == item.Id);

            _logger.LogInformation("Đã tạo design template item với ID: {ItemID}", item.Id);
            return MapToDto(createdItem!);
        }

        public async Task<DesignTemplateItemDto?> UpdateAsync(string id, UpdateDesignTemplateItemDto updateDto)
        {
            _logger.LogInformation("Cập nhật design template item với ID: {ItemID}", id);
            var item = await _context.DesignTemplateItems.FindAsync(id);

            if (item == null)
            {
                _logger.LogWarning("Không tìm thấy design template item với ID: {ItemID}", id);
                return null;
            }

            // Map các thuộc tính không null
            if (!string.IsNullOrEmpty(updateDto.ItemName))
                item.ItemName = updateDto.ItemName;
            
            if (updateDto.Description != null)
                item.Description = updateDto.Description;
            
            if (updateDto.PlacementPosition.HasValue)
                item.PlacementPosition = updateDto.PlacementPosition.Value;
            
            if (updateDto.ImageUrl != null)
                item.ImageUrl = updateDto.ImageUrl;
            
            if (updateDto.Width.HasValue)
                item.Width = updateDto.Width.Value;
            
            if (updateDto.Height.HasValue)
                item.Height = updateDto.Height.Value;
            
            if (updateDto.DisplayOrder.HasValue)
                item.DisplayOrder = updateDto.DisplayOrder.Value;

            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Lấy lại với thông tin đầy đủ
            var updatedItem = await _context.DesignTemplateItems
                .Include(dti => dti.Design)
                .FirstOrDefaultAsync(dti => dti.Id == id);

            _logger.LogInformation("Đã cập nhật design template item với ID: {ItemID}", id);
            return MapToDto(updatedItem!);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            _logger.LogInformation("Xóa design template item với ID: {ItemID}", id);
            var item = await _context.DesignTemplateItems.FindAsync(id);

            if (item == null)
            {
                _logger.LogWarning("Không tìm thấy design template item với ID: {ItemID}", id);
                return false;
            }

            _context.DesignTemplateItems.Remove(item);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Đã xóa design template item với ID: {ItemID}", id);
            return true;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.DesignTemplateItems.AnyAsync(dti => dti.Id == id);
        }

        public async Task<bool> DesignExistsAsync(string designId)
        {
            return await _context.Designs.AnyAsync(d => d.DesignID == designId);
        }

        public async Task<int> GetNextDisplayOrderForDesignAsync(string designId)
        {
            var maxOrder = await _context.DesignTemplateItems
                .Where(dti => dti.DesignId == designId)
                .MaxAsync(dti => (int?)dti.DisplayOrder);
            
            return (maxOrder ?? 0) + 1;
        }

        private DesignTemplateItemDto MapToDto(DesignTemplateItem item)
        {
            var dto = _mapper.Map<DesignTemplateItemDto>(item);
            dto.PlacementPositionName = VehiclePartHelper.GetDescription(item.PlacementPosition);
            return dto;
        }
    }
}

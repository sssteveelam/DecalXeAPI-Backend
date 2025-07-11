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
    public class DecalTemplateService : IDecalTemplateService // <-- Kế thừa từ IDecalTemplateService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DecalTemplateService> _logger;

        public DecalTemplateService(ApplicationDbContext context, IMapper mapper, ILogger<DecalTemplateService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<DecalTemplateDto>> GetDecalTemplatesAsync()
        {
            _logger.LogInformation("Lấy danh sách mẫu decal.");
            var decalTemplates = await _context.DecalTemplates.Include(dt => dt.DecalType).ToListAsync();
            var decalTemplateDtos = _mapper.Map<List<DecalTemplateDto>>(decalTemplates);
            return decalTemplateDtos;
        }

        public async Task<DecalTemplateDto?> GetDecalTemplateByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy mẫu decal với ID: {TemplateID}", id);
            var decalTemplate = await _context.DecalTemplates.Include(dt => dt.DecalType).FirstOrDefaultAsync(dt => dt.TemplateID == id);

            if (decalTemplate == null)
            {
                _logger.LogWarning("Không tìm thấy mẫu decal với ID: {TemplateID}", id);
                return null;
            }

            var decalTemplateDto = _mapper.Map<DecalTemplateDto>(decalTemplate);
            _logger.LogInformation("Đã trả về mẫu decal với ID: {TemplateID}", id);
            return decalTemplateDto;
        }

        public async Task<DecalTemplateDto> CreateDecalTemplateAsync(DecalTemplate decalTemplate)
        {
            _logger.LogInformation("Yêu cầu tạo mẫu decal mới: {TemplateName}", decalTemplate.TemplateName);

            // Kiểm tra FKs
            if (!string.IsNullOrEmpty(decalTemplate.DecalTypeID) && !await DecalTypeExistsAsync(decalTemplate.DecalTypeID))
            {
                _logger.LogWarning("DecalTypeID không tồn tại khi tạo DecalTemplate: {DecalTypeID}", decalTemplate.DecalTypeID);
                throw new ArgumentException("DecalTypeID không tồn tại.");
            }

            _context.DecalTemplates.Add(decalTemplate);
            await _context.SaveChangesAsync();

            await _context.Entry(decalTemplate).Reference(dt => dt.DecalType).LoadAsync();

            var decalTemplateDto = _mapper.Map<DecalTemplateDto>(decalTemplate);
            _logger.LogInformation("Đã tạo mẫu decal mới với ID: {TemplateID}", decalTemplate.TemplateID);
            return decalTemplateDto;
        }

        public async Task<bool> UpdateDecalTemplateAsync(string id, DecalTemplate decalTemplate)
        {
            _logger.LogInformation("Yêu cầu cập nhật mẫu decal với ID: {TemplateID}", id);

            if (id != decalTemplate.TemplateID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với TemplateID trong body ({TemplateIDBody})", id, decalTemplate.TemplateID);
                return false;
            }

            if (!await DecalTemplateExistsAsync(id))
            {
                _logger.LogWarning("Không tìm thấy mẫu decal để cập nhật với ID: {TemplateID}", id);
                return false;
            }

            // Kiểm tra FKs
            if (!string.IsNullOrEmpty(decalTemplate.DecalTypeID) && !await DecalTypeExistsAsync(decalTemplate.DecalTypeID))
            {
                _logger.LogWarning("DecalTypeID không tồn tại khi cập nhật DecalTemplate: {DecalTypeID}", decalTemplate.DecalTypeID);
                throw new ArgumentException("DecalTypeID không tồn tại.");
            }

            _context.Entry(decalTemplate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật mẫu decal với ID: {TemplateID}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật mẫu decal với ID: {TemplateID}", id);
                throw;
            }
        }

        public async Task<bool> DeleteDecalTemplateAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa mẫu decal với ID: {TemplateID}", id);
            var decalTemplate = await _context.DecalTemplates.FindAsync(id);
            if (decalTemplate == null)
            {
                _logger.LogWarning("Không tìm thấy mẫu decal để xóa với ID: {TemplateID}", id);
                return false;
            }

            _context.DecalTemplates.Remove(decalTemplate);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa mẫu decal với ID: {TemplateID}", id);
            return true;
        }

        // --- BỔ SUNG PHƯƠNG THỨC MỚI ---
        public async Task<(bool Success, string? ErrorMessage)> AssignTemplateToVehicleAsync(string templateId, string modelId)
        {
            if (!await DecalTemplateExistsAsync(templateId))
                return (false, "Mẫu decal không tồn tại.");
            if (!await _context.VehicleModels.AnyAsync(m => m.ModelID == modelId))
                return (false, "Mẫu xe không tồn tại.");
            if (await _context.VehicleModelDecalTemplates.AnyAsync(l => l.TemplateID == templateId && l.ModelID == modelId))
                return (false, "Mẫu decal này đã được gán cho mẫu xe.");

            var link = new VehicleModelDecalTemplate
            {
                VehicleModelDecalTemplateID = Guid.NewGuid().ToString(),
                TemplateID = templateId,
                ModelID = modelId
            };

            _context.VehicleModelDecalTemplates.Add(link);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UnassignTemplateFromVehicleAsync(string templateId, string modelId)
        {
            var link = await _context.VehicleModelDecalTemplates
                .FirstOrDefaultAsync(l => l.TemplateID == templateId && l.ModelID == modelId);

            if (link == null)
                return (false, "Liên kết không tồn tại để xóa.");

            _context.VehicleModelDecalTemplates.Remove(link);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        // --- HÀM HỖ TRỢ: KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (PUBLIC CHO INTERFACE) ---
        public async Task<bool> DecalTemplateExistsAsync(string id)
        {
            return await _context.DecalTemplates.AnyAsync(e => e.TemplateID == id);
        }

        public async Task<bool> DecalTypeExistsAsync(string id)
        {
            return await _context.DecalTypes.AnyAsync(e => e.DecalTypeID == id);
        }


        public async Task<IEnumerable<DecalTemplateDto>> GetTemplatesByModelIdAsync(string modelId)
        {
            _logger.LogInformation("Đang lấy danh sách các mẫu decal cho VehicleModel ID: {ModelID}", modelId);

            // 1. Từ bảng liên kết, tìm tất cả những dòng có ModelID mà mình cần
            var links = await _context.VehicleModelDecalTemplates
                                    .Where(link => link.ModelID == modelId)
                                    // 2. Từ những dòng đó, lấy thông tin chi tiết của Mẫu Decal (DecalTemplate)
                                    .Include(link => link.DecalTemplate) 
                                        // 3. Kèm theo đó, lấy luôn thông tin về Loại Decal (DecalType)
                                        .ThenInclude(template => template.DecalType)
                                    // 4. Chỉ chọn ra các đối tượng DecalTemplate hoàn chỉnh
                                    .Select(link => link.DecalTemplate) 
                                    .ToListAsync();

            // 5. Dùng AutoMapper để "dịch" danh sách đó sang DTO và trả về
            return _mapper.Map<List<DecalTemplateDto>>(links);
        }

        // --- KẾT THÚC PHƯƠNG THỨC MỚI ---
    }
}
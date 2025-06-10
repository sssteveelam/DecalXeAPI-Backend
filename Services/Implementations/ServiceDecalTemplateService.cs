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
    public class ServiceDecalTemplateService : IServiceDecalTemplateService // <-- Kế thừa từ IServiceDecalTemplateService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ServiceDecalTemplateService> _logger;

        public ServiceDecalTemplateService(ApplicationDbContext context, IMapper mapper, ILogger<ServiceDecalTemplateService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ServiceDecalTemplateDto>> GetServiceDecalTemplatesAsync()
        {
            _logger.LogInformation("Lấy danh sách mẫu decal dịch vụ.");
            var serviceDecalTemplates = await _context.ServiceDecalTemplates
                                                        .Include(sdt => sdt.DecalService)
                                                        .Include(sdt => sdt.DecalTemplate)
                                                        .ToListAsync();
            var serviceDecalTemplateDtos = _mapper.Map<List<ServiceDecalTemplateDto>>(serviceDecalTemplates);
            return serviceDecalTemplateDtos;
        }

        public async Task<ServiceDecalTemplateDto?> GetServiceDecalTemplateByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy mẫu decal dịch vụ với ID: {ServiceDecalTemplateID}", id);
            var serviceDecalTemplate = await _context.ServiceDecalTemplates
                                                            .Include(sdt => sdt.DecalService)
                                                            .Include(sdt => sdt.DecalTemplate)
                                                            .FirstOrDefaultAsync(sdt => sdt.ServiceDecalTemplateID == id);

            if (serviceDecalTemplate == null)
            {
                _logger.LogWarning("Không tìm thấy mẫu decal dịch vụ với ID: {ServiceDecalTemplateID}", id);
                return null;
            }

            var serviceDecalTemplateDto = _mapper.Map<ServiceDecalTemplateDto>(serviceDecalTemplate);
            _logger.LogInformation("Đã trả về mẫu decal dịch vụ với ID: {ServiceDecalTemplateID}", id);
            return serviceDecalTemplateDto;
        }

        public async Task<ServiceDecalTemplateDto> CreateServiceDecalTemplateAsync(ServiceDecalTemplate serviceDecalTemplate)
        {
            _logger.LogInformation("Yêu cầu tạo mẫu decal dịch vụ mới cho ServiceID: {ServiceID}, TemplateID: {TemplateID}", serviceDecalTemplate.ServiceID, serviceDecalTemplate.TemplateID);

            // Kiểm tra FKs
            if (!string.IsNullOrEmpty(serviceDecalTemplate.ServiceID) && !await DecalServiceExistsAsync(serviceDecalTemplate.ServiceID))
            {
                _logger.LogWarning("ServiceID không tồn tại khi tạo ServiceDecalTemplate: {ServiceID}", serviceDecalTemplate.ServiceID);
                throw new ArgumentException("ServiceID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(serviceDecalTemplate.TemplateID) && !await DecalTemplateExistsAsync(serviceDecalTemplate.TemplateID))
            {
                _logger.LogWarning("TemplateID không tồn tại khi tạo ServiceDecalTemplate: {TemplateID}", serviceDecalTemplate.TemplateID);
                throw new ArgumentException("TemplateID không tồn tại.");
            }

            _context.ServiceDecalTemplates.Add(serviceDecalTemplate);
            await _context.SaveChangesAsync();

            // Tải lại thông tin liên quan
            await _context.Entry(serviceDecalTemplate).Reference(sdt => sdt.DecalService).LoadAsync();
            await _context.Entry(serviceDecalTemplate).Reference(sdt => sdt.DecalTemplate).LoadAsync();

            var serviceDecalTemplateDto = _mapper.Map<ServiceDecalTemplateDto>(serviceDecalTemplate);
            _logger.LogInformation("Đã tạo mẫu decal dịch vụ mới với ID: {ServiceDecalTemplateID}", serviceDecalTemplate.ServiceDecalTemplateID);
            return serviceDecalTemplateDto;
        }

        public async Task<bool> UpdateServiceDecalTemplateAsync(string id, ServiceDecalTemplate serviceDecalTemplate)
        {
            _logger.LogInformation("Yêu cầu cập nhật mẫu decal dịch vụ với ID: {ServiceDecalTemplateID}", id);

            if (id != serviceDecalTemplate.ServiceDecalTemplateID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với ServiceDecalTemplateID trong body ({ServiceDecalTemplateIDBody})", id, serviceDecalTemplate.ServiceDecalTemplateID);
                return false;
            }

            if (!await ServiceDecalTemplateExistsAsync(id))
            {
                _logger.LogWarning("Không tìm thấy mẫu decal dịch vụ để cập nhật với ID: {ServiceDecalTemplateID}", id);
                return false;
            }

            // Kiểm tra FKs
            if (!string.IsNullOrEmpty(serviceDecalTemplate.ServiceID) && !await DecalServiceExistsAsync(serviceDecalTemplate.ServiceID))
            {
                _logger.LogWarning("ServiceID không tồn tại khi cập nhật ServiceDecalTemplate: {ServiceID}", serviceDecalTemplate.ServiceID);
                throw new ArgumentException("ServiceID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(serviceDecalTemplate.TemplateID) && !await DecalTemplateExistsAsync(serviceDecalTemplate.TemplateID))
            {
                _logger.LogWarning("TemplateID không tồn tại khi cập nhật ServiceDecalTemplate: {TemplateID}", serviceDecalTemplate.TemplateID);
                throw new ArgumentException("TemplateID không tồn tại.");
            }

            _context.Entry(serviceDecalTemplate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật mẫu decal dịch vụ với ID: {ServiceDecalTemplateID}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật mẫu decal dịch vụ với ID: {ServiceDecalTemplateID}", id);
                throw;
            }
        }

        public async Task<bool> DeleteServiceDecalTemplateAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa mẫu decal dịch vụ với ID: {ServiceDecalTemplateID}", id);
            var serviceDecalTemplate = await _context.ServiceDecalTemplates.FindAsync(id);
            if (serviceDecalTemplate == null)
            {
                _logger.LogWarning("Không tìm thấy mẫu decal dịch vụ để xóa với ID: {ServiceDecalTemplateID}", id);
                return false;
            }

            _context.ServiceDecalTemplates.Remove(serviceDecalTemplate);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa mẫu decal dịch vụ với ID: {ServiceDecalTemplateID}", id);
            return true;
        }

        // --- HÀM HỖ TRỢ: KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (PUBLIC CHO INTERFACE) ---
        public async Task<bool> ServiceDecalTemplateExistsAsync(string id)
        {
            return await _context.ServiceDecalTemplates.AnyAsync(e => e.ServiceDecalTemplateID == id);
        }

        public async Task<bool> DecalServiceExistsAsync(string id)
        {
            return await _context.DecalServices.AnyAsync(e => e.ServiceID == id);
        }

        public async Task<bool> DecalTemplateExistsAsync(string id)
        {
            return await _context.DecalTemplates.AnyAsync(e => e.TemplateID == id);
        }
    }
}
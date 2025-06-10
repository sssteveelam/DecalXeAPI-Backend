using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IServiceDecalTemplateService
    {
        Task<IEnumerable<ServiceDecalTemplateDto>> GetServiceDecalTemplatesAsync();
        Task<ServiceDecalTemplateDto?> GetServiceDecalTemplateByIdAsync(string id);
        Task<ServiceDecalTemplateDto> CreateServiceDecalTemplateAsync(ServiceDecalTemplate serviceDecalTemplate);
        Task<bool> UpdateServiceDecalTemplateAsync(string id, ServiceDecalTemplate serviceDecalTemplate);
        Task<bool> DeleteServiceDecalTemplateAsync(string id);

        // Các hàm kiểm tra tồn tại (Exists) cần thiết cho Service này
        Task<bool> ServiceDecalTemplateExistsAsync(string id);
        Task<bool> DecalServiceExistsAsync(string id); // Cần để kiểm tra FK
        Task<bool> DecalTemplateExistsAsync(string id); // Cần để kiểm tra FK
    }
}
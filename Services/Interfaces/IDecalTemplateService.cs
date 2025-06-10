using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IDecalTemplateService
    {
        Task<IEnumerable<DecalTemplateDto>> GetDecalTemplatesAsync();
        Task<DecalTemplateDto?> GetDecalTemplateByIdAsync(string id);
        Task<DecalTemplateDto> CreateDecalTemplateAsync(DecalTemplate decalTemplate);
        Task<bool> UpdateDecalTemplateAsync(string id, DecalTemplate decalTemplate);
        Task<bool> DeleteDecalTemplateAsync(string id);

        // Hàm kiểm tra tồn tại (Exists) cần thiết cho Service này
        Task<bool> DecalTemplateExistsAsync(string id);
        Task<bool> DecalTypeExistsAsync(string id); // Cần để kiểm tra FK
    }
}
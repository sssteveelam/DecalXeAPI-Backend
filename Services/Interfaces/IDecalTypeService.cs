using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IDecalTypeService
    {
        Task<IEnumerable<DecalTypeDto>> GetDecalTypesAsync();
        Task<DecalTypeDto?> GetDecalTypeByIdAsync(string id);
        Task<DecalTypeDto> CreateDecalTypeAsync(DecalType decalType);
        Task<bool> UpdateDecalTypeAsync(string id, DecalType decalType);
        Task<bool> DeleteDecalTypeAsync(string id);

        // Hàm kiểm tra tồn tại (Exists) cần thiết cho Service này
        Task<bool> DecalTypeExistsAsync(string id);
    }
}
// DecalXeAPI/Services/Interfaces/IDesignTemplateItemService.cs
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IDesignTemplateItemService
    {
        Task<IEnumerable<DesignTemplateItemDto>> GetAllAsync();
        Task<DesignTemplateItemDto?> GetByIdAsync(string id);
        Task<IEnumerable<DesignTemplateItemDto>> GetByDesignIdAsync(string designId);
        Task<IEnumerable<DesignTemplateItemDto>> GetByPlacementPositionAsync(VehiclePart placementPosition);
        Task<DesignTemplateItemDto> CreateAsync(CreateDesignTemplateItemDto createDto);
        Task<DesignTemplateItemDto?> UpdateAsync(string id, UpdateDesignTemplateItemDto updateDto);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task<bool> DesignExistsAsync(string designId);
        Task<int> GetNextDisplayOrderForDesignAsync(string designId);
    }
}

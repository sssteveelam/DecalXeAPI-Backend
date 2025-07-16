// DecalXeAPI/Services/Interfaces/ICategoryService.cs
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
        Task<CategoryDto?> GetCategoryByIdAsync(string id);
        Task<CategoryDto> CreateCategoryAsync(Category category);
        Task<bool> UpdateCategoryAsync(string id, Category category);
        Task<bool> DeleteCategoryAsync(string id);
        Task<bool> CategoryExistsAsync(string id);
    }
}
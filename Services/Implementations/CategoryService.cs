// DecalXeAPI/Services/Implementations/CategoryService.cs
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
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ApplicationDbContext context, IMapper mapper, ILogger<CategoryService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
        {
            _logger.LogInformation("Lấy danh sách danh mục.");
            var categories = await _context.Categories.ToListAsync();
            return _mapper.Map<List<CategoryDto>>(categories);
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy danh mục với ID: {CategoryID}", id);
            var category = await _context.Categories.FindAsync(id);
            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> CreateCategoryAsync(Category category)
        {
            _logger.LogInformation("Yêu cầu tạo danh mục mới: {CategoryName}", category.CategoryName);
            if (await _context.Categories.AnyAsync(c => c.CategoryName == category.CategoryName))
            {
                throw new ArgumentException("Tên danh mục đã tồn tại.");
            }
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<bool> UpdateCategoryAsync(string id, Category category)
        {
            _logger.LogInformation("Yêu cầu cập nhật danh mục với ID: {CategoryID}", id);
            if (id != category.CategoryID) return false;
            if (!await CategoryExistsAsync(id)) return false;
            if (await _context.Categories.AnyAsync(c => c.CategoryName == category.CategoryName && c.CategoryID != id))
            {
                throw new ArgumentException("Tên danh mục đã tồn tại.");
            }
            _context.Entry(category).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); return true; }
            catch (DbUpdateConcurrencyException ex) { _logger.LogError(ex, "Lỗi xung đột khi cập nhật danh mục."); throw; }
        }

        public async Task<bool> DeleteCategoryAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa danh mục với ID: {CategoryID}", id);
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;
            if (await _context.Products.AnyAsync(p => p.CategoryID == id))
            {
                throw new InvalidOperationException("Không thể xóa danh mục vì có sản phẩm liên kết.");
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CategoryExistsAsync(string id)
        {
            return await _context.Categories.AnyAsync(e => e.CategoryID == id);
        }
    }
}
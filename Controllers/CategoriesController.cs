// DecalXeAPI/Controllers/CategoriesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces;
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Inventory")] // Quyền cho CategoriesController
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ApplicationDbContext context, ICategoryService categoryService, IMapper mapper, ILogger<CategoriesController> logger)
        {
            _context = context;
            _categoryService = categoryService;
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách danh mục.");
            var categories = await _categoryService.GetCategoriesAsync();
            return Ok(categories);
        }

        // API: GET api/Categories/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(string id)
        {
            _logger.LogInformation("Yêu cầu lấy danh mục với ID: {CategoryID}", id);
            var categoryDto = await _categoryService.GetCategoryByIdAsync(id);

            if (categoryDto == null)
            {
                _logger.LogWarning("Không tìm thấy danh mục với ID: {CategoryID}", id);
                return NotFound();
            }

            return Ok(categoryDto);
        }

        // API: POST api/Categories
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> PostCategory(Category category)
        {
            _logger.LogInformation("Yêu cầu tạo danh mục mới: {CategoryName}", category.CategoryName);

            try
            {
                var createdCategoryDto = await _categoryService.CreateCategoryAsync(category);
                _logger.LogInformation("Đã tạo danh mục mới với ID: {CategoryID}", createdCategoryDto.CategoryID);
                return CreatedAtAction(nameof(GetCategory), new { id = createdCategoryDto.CategoryID }, createdCategoryDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi tạo danh mục: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/Categories/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(string id, Category category)
        {
            _logger.LogInformation("Yêu cầu cập nhật danh mục với ID: {CategoryID}", id);
            if (id != category.CategoryID)
            {
                return BadRequest();
            }

            try
            {
                var success = await _categoryService.UpdateCategoryAsync(id, category);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy danh mục để cập nhật với ID: {CategoryID}", id);
                    return NotFound();
                }

                _logger.LogInformation("Đã cập nhật danh mục với ID: {CategoryID}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi cập nhật danh mục: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // API: DELETE api/Categories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            _logger.LogInformation("Yêu cầu xóa danh mục với ID: {CategoryID}", id);
            try
            {
                var success = await _categoryService.DeleteCategoryAsync(id);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy danh mục để xóa với ID: {CategoryID}", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi xóa danh mục: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG ---
        private bool CategoryExists(string id) { return _context.Categories.Any(e => e.CategoryID == id); }
    }
}
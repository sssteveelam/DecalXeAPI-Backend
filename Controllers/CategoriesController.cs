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
        public async Task<ActionResult<CategoryDto>> PostCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            // Dùng AutoMapper để tạo một đối tượng Category mới từ DTO
            var category = _mapper.Map<Category>(createCategoryDto);
            category.CategoryID = Guid.NewGuid().ToString();;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Trả về CategoryDto đầy đủ thông tin sau khi tạo thành công
            var categoryDto = _mapper.Map<CategoryDto>(category);
            return CreatedAtAction("GetCategory", new { id = category.CategoryID }, categoryDto);
        }

        // API: PUT api/Categories/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(String id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Dùng AutoMapper để cập nhật category từ DTO
            _mapper.Map(updateCategoryDto, category);

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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

            return NoContent();
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
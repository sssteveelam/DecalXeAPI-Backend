using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // <-- THÊM DÒNG NÀY (Để sử dụng IProductService)
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Inventory")] // Quyền cho ProductsController
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists cơ bản
        private readonly IProductService _productService; // <-- KHAI BÁO BIẾN CHO SERVICE
        private readonly IMapper _mapper;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ApplicationDbContext context, IProductService productService, IMapper mapper, ILogger<ProductsController> logger) // <-- TIÊM IProductService
        {
            _context = context;
            _productService = productService;
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách sản phẩm.");
            var products = await _productService.GetProductsAsync();
            return Ok(products);
        }

        // API: GET api/Products/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(string id)
        {
            _logger.LogInformation("Yêu cầu lấy sản phẩm với ID: {ProductID}", id);
            var productDto = await _productService.GetProductByIdAsync(id);

            if (productDto == null)
            {
                _logger.LogWarning("Không tìm thấy sản phẩm với ID: {ProductID}", id);
                return NotFound();
            }

            return Ok(productDto);
        }

        // API: POST api/Products (ĐÃ NÂNG CẤP)
        [HttpPost]
        public async Task<ActionResult<ProductDto>> PostProduct(CreateProductDto createDto)
        {
            _logger.LogInformation("Yêu cầu tạo sản phẩm mới: {ProductName}", createDto.ProductName);
            
            // Đệ có thể thêm logic kiểm tra CategoryID tồn tại ở đây nếu muốn
            
            var product = _mapper.Map<Product>(createDto);
            
            try
            {
                var createdProductDto = await _productService.CreateProductAsync(product);
                return CreatedAtAction(nameof(GetProduct), new { id = createdProductDto.ProductID }, createdProductDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/Products/{id} (ĐÃ NÂNG CẤP)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(string id, UpdateProductDto updateDto)
        {
            _logger.LogInformation("Yêu cầu cập nhật sản phẩm với ID: {ProductID}", id);
            
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _mapper.Map(updateDto, product);

            try
            {
                var success = await _productService.UpdateProductAsync(id, product);
                if (!success) return NotFound();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // API: DELETE api/Products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            _logger.LogInformation("Yêu cầu xóa sản phẩm với ID: {ProductID}", id);
            var success = await _productService.DeleteProductAsync(id);

            if (!success)
            {
                _logger.LogWarning("Không tìm thấy sản phẩm để xóa với ID: {ProductID}", id);
                return NotFound();
            }

            return NoContent();
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool ProductExists(string id) { return _context.Products.Any(e => e.ProductID == id); }
    }
}
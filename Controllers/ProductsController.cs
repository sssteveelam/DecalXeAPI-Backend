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

        // API: POST api/Products
        [HttpPost]
        public async Task<ActionResult<ProductDto>> PostProduct(Product product) // Vẫn nhận Product Model
        {
            _logger.LogInformation("Yêu cầu tạo sản phẩm mới: {ProductName}", product.ProductName);
            try
            {
                var createdProductDto = await _productService.CreateProductAsync(product);
                _logger.LogInformation("Đã tạo sản phẩm mới với ID: {ProductID}", createdProductDto.ProductID);
                return CreatedAtAction(nameof(GetProduct), new { id = createdProductDto.ProductID }, createdProductDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi tạo sản phẩm: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/Products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(string id, Product product)
        {
            _logger.LogInformation("Yêu cầu cập nhật sản phẩm với ID: {ProductID}", id);
            if (id != product.ProductID)
            {
                return BadRequest();
            }

            try
            {
                var success = await _productService.UpdateProductAsync(id, product);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy sản phẩm để cập nhật với ID: {ProductID}", id);
                    return NotFound();
                }

                _logger.LogInformation("Đã cập nhật sản phẩm với ID: {ProductID}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi cập nhật sản phẩm: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // Cần cho IProductService và ICategoryService (nếu dùng ở Controller)
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using Swashbuckle.AspNetCore.Filters; // Cần cho SwaggerRequestExample

namespace DecalXeAPI.Controllers
{
    /// <summary>
    /// Controller quản lý thông tin sản phẩm/vật tư trong kho.
    /// Yêu cầu quyền Admin, Manager hoặc Inventory để truy cập.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Inventory")] // Quyền cho ProductsController
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists cơ bản
        private readonly IProductService _productService;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ApplicationDbContext context, IProductService productService, IMapper mapper, ILogger<ProductsController> logger)
        {
            _context = context;
            _productService = productService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả các sản phẩm/vật tư hiện có trong kho.
        /// </summary>
        /// <returns>Danh sách các đối tượng ProductDto.</returns>
        /// <response code="200">Trả về danh sách sản phẩm.</response>
        /// <response code="401">Không được ủy quyền.</response>
        /// <response code="403">Bị cấm.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách sản phẩm.");
            var products = await _productService.GetProductsAsync();
            return Ok(products);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một sản phẩm/vật tư dựa trên ProductID.
        /// </summary>
        /// <param name="id">ProductID của sản phẩm cần lấy.</param>
        /// <returns>Đối tượng ProductDto chứa thông tin chi tiết.</returns>
        /// <response code="200">Trả về thông tin sản phẩm.</response>
        /// <response code="404">Không tìm thấy sản phẩm với ID đã cho.</response>
        /// <response code="401">Không được ủy quyền.</response>
        /// <response code="403">Bị cấm.</response>
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

        /// <summary>
        /// Tạo một sản phẩm/vật tư mới.
        /// </summary>
        /// <remarks>
        /// Cần cung cấp đầy đủ thông tin sản phẩm và CategoryID hợp lệ.
        /// </remarks>
        /// <param name="createDto">Đối tượng CreateProductDto chứa thông tin sản phẩm cần tạo.</param>
        /// <returns>Đối tượng ProductDto của sản phẩm vừa tạo.</returns>
        /// <response code="201">Tạo sản phẩm thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ hoặc CategoryID không tồn tại.</response>
        /// <response code="401">Không được ủy quyền.</response>
        /// <response code="403">Bị cấm.</response>
        [HttpPost]
        public async Task<ActionResult<ProductDto>> PostProduct(CreateProductDto createDto)
        {
            _logger.LogInformation("Yêu cầu tạo sản phẩm mới: {ProductName}", createDto.ProductName);
            
            // Kiểm tra CategoryID tồn tại ở Controller trước khi gọi Service
            if (!string.IsNullOrEmpty(createDto.CategoryID) && !CategoryExists(createDto.CategoryID))
            {
                return BadRequest("CategoryID không tồn tại.");
            }
            
            var product = _mapper.Map<Product>(createDto);
            
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

        /// <summary>
        /// Cập nhật thông tin của một sản phẩm/vật tư hiện có dựa trên ProductID.
        /// </summary>
        /// <param name="id">ProductID của sản phẩm cần cập nhật.</param>
        /// <param name="updateDto">Đối tượng UpdateProductDto chứa thông tin cập nhật.</param>
        /// <returns>Không có nội dung nếu cập nhật thành công.</returns>
        /// <response code="204">Cập nhật thành công.</response>
        /// <response code="400">ID trong URL không khớp với ProductID trong body, dữ liệu không hợp lệ, hoặc CategoryID không tồn tại.</response>
        /// <response code="404">Không tìm thấy sản phẩm để cập nhật.</response>
        /// <response code="401">Không được ủy quyền.</response>
        /// <response code="403">Bị cấm.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(string id, UpdateProductDto updateDto)
        {
            _logger.LogInformation("Yêu cầu cập nhật sản phẩm với ID: {ProductID}", id);
            
            // Kiểm tra CategoryID tồn tại ở Controller trước khi gọi Service
            if (!string.IsNullOrEmpty(updateDto.CategoryID) && !CategoryExists(updateDto.CategoryID))
            {
                return BadRequest("CategoryID không tồn tại.");
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _mapper.Map(updateDto, product); // Ánh xạ dữ liệu từ DTO vào Entity Model
            
            try
            {
                var success = await _productService.UpdateProductAsync(id, product);
                if (!success) return NotFound(); // Service sẽ trả về false nếu không tìm thấy

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

        /// <summary>
        /// Xóa một sản phẩm/vật tư khỏi hệ thống dựa trên ProductID.
        /// </summary>
        /// <remarks>
        /// Sản phẩm không thể bị xóa nếu đang được sử dụng (ví dụ: trong ServiceVehicleModelProduct).
        /// </remarks>
        /// <param name="id">ProductID của sản phẩm cần xóa.</param>
        /// <returns>Không có nội dung nếu xóa thành công.</returns>
        /// <response code="204">Xóa thành công.</response>
        /// <response code="400">Không thể xóa sản phẩm vì đang được sử dụng.</response>
        /// <response code="404">Không tìm thấy sản phẩm để xóa.</response>
        /// <response code="401">Không được ủy quyền.</response>
        /// <response code="403">Bị cấm.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            _logger.LogInformation("Yêu cầu xóa sản phẩm với ID: {ProductID}", id);
            try
            {
                var success = await _productService.DeleteProductAsync(id);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy sản phẩm để xóa với ID: {ProductID}", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi xóa sản phẩm: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG ---
        private bool ProductExists(string id) { return _context.Products.Any(e => e.ProductID == id); }
        private bool CategoryExists(string id) { return _context.Categories.Any(e => e.CategoryID == id); } // <-- THÊM DÒNG NÀY
    }
}
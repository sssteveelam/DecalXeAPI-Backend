using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data; // Vẫn cần DbContext cho các hàm Exists cơ bản
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // <-- THÊM DÒNG NÀY (Để sử dụng IServiceProductService)
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System; // Để sử dụng ArgumentException

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Inventory")] // Quyền cho ServiceProductsController
    public class ServiceProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists cơ bản
        private readonly IServiceProductService _serviceProductService; // <-- KHAI BÁO BIẾN CHO SERVICE
        private readonly IMapper _mapper;
        private readonly ILogger<ServiceProductsController> _logger;

        public ServiceProductsController(ApplicationDbContext context, IServiceProductService serviceProductService, IMapper mapper, ILogger<ServiceProductsController> logger) // <-- TIÊM IServiceProductService
        {
            _context = context;
            _serviceProductService = serviceProductService;
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/ServiceProducts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceProductDto>>> GetServiceProducts()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách sản phẩm dịch vụ.");
            // Ủy quyền logic cho Service Layer
            var serviceProducts = await _serviceProductService.GetServiceProductsAsync();
            return Ok(serviceProducts);
        }

        // API: GET api/ServiceProducts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceProductDto>> GetServiceProduct(string id)
        {
            _logger.LogInformation("Yêu cầu lấy sản phẩm dịch vụ với ID: {ServiceProductID}", id);
            // Ủy quyền logic cho Service Layer
            var serviceProductDto = await _serviceProductService.GetServiceProductByIdAsync(id);

            if (serviceProductDto == null)
            {
                _logger.LogWarning("Không tìm thấy sản phẩm dịch vụ với ID: {ServiceProductID}", id);
                return NotFound();
            }

            return Ok(serviceProductDto);
        }

        // API: POST api/ServiceProducts
        [HttpPost]
        public async Task<ActionResult<ServiceProductDto>> PostServiceProduct(ServiceProduct serviceProduct) // Vẫn nhận Model
        {
            _logger.LogInformation("Yêu cầu tạo sản phẩm dịch vụ mới cho ServiceID: {ServiceID}, ProductID: {ProductID}", serviceProduct.ServiceID, serviceProduct.ProductID);

            // --- KIỂM TRA FKs CHÍNH TRƯỚC KHI GỬI VÀO SERVICE ---
            if (!string.IsNullOrEmpty(serviceProduct.ServiceID) && !DecalServiceExists(serviceProduct.ServiceID))
            {
                return BadRequest("ServiceID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(serviceProduct.ProductID) && !ProductExists(serviceProduct.ProductID))
            {
                return BadRequest("ProductID không tồn tại.");
            }

            try
            {
                // Ủy quyền logic tạo ServiceProduct cho Service Layer
                var createdServiceProductDto = await _serviceProductService.CreateServiceProductAsync(serviceProduct);
                _logger.LogInformation("Đã tạo sản phẩm dịch vụ mới với ID: {ServiceProductID}", createdServiceProductDto.ServiceProductID);
                return CreatedAtAction(nameof(GetServiceProduct), new { id = createdServiceProductDto.ServiceProductID }, createdServiceProductDto);
            }
            catch (ArgumentException ex) // Bắt lỗi từ Service nếu có (ví dụ: duplicate name)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi tạo sản phẩm dịch vụ: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/ServiceProducts/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServiceProduct(string id, ServiceProduct serviceProduct)
        {
            _logger.LogInformation("Yêu cầu cập nhật sản phẩm dịch vụ với ID: {ServiceProductID}", id);
            if (id != serviceProduct.ServiceProductID)
            {
                return BadRequest();
            }

            // Kiểm tra FKs chính
            if (!string.IsNullOrEmpty(serviceProduct.ServiceID) && !DecalServiceExists(serviceProduct.ServiceID))
            {
                return BadRequest("ServiceID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(serviceProduct.ProductID) && !ProductExists(serviceProduct.ProductID))
            {
                return BadRequest("ProductID không tồn tại.");
            }

            try
            {
                // Ủy quyền logic cập nhật ServiceProduct cho Service Layer
                var success = await _serviceProductService.UpdateServiceProductAsync(id, serviceProduct);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy sản phẩm dịch vụ để cập nhật với ID: {ServiceProductID}", id);
                    return NotFound();
                }

                _logger.LogInformation("Đã cập nhật sản phẩm dịch vụ với ID: {ServiceProductID}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi cập nhật sản phẩm dịch vụ: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (DbUpdateConcurrencyException) // Vẫn bắt riêng lỗi này ở Controller
            {
                if (!ServiceProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // API: DELETE api/ServiceProducts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceProduct(string id)
        {
            _logger.LogInformation("Yêu cầu xóa sản phẩm dịch vụ với ID: {ServiceProductID}", id);
            // Ủy quyền logic xóa ServiceProduct cho Service Layer
            var success = await _serviceProductService.DeleteServiceProductAsync(id);

            if (!success)
            {
                _logger.LogWarning("Không tìm thấy sản phẩm dịch vụ để xóa với ID: {ServiceProductID}", id);
                return NotFound();
            }

            return NoContent();
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool ServiceProductExists(string id) { return _context.ServiceProducts.Any(e => e.ServiceProductID == id); }
        private bool DecalServiceExists(string id) { return _context.DecalServices.Any(e => e.ServiceID == id); }
        private bool ProductExists(string id) { return _context.Products.Any(e => e.ProductID == id); }
    }
}
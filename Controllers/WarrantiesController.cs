using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data; // Vẫn cần DbContext cho các hàm Exists cơ bản
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // <-- THÊM DÒNG NÀY (Để sử dụng IWarrantyService)
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System; // Để sử dụng ArgumentException

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Sales")] // Quyền cho WarrantiesController
    public class WarrantiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists cơ bản
        private readonly IWarrantyService _warrantyService; // <-- KHAI BÁO BIẾN CHO SERVICE
        private readonly IMapper _mapper;
        private readonly ILogger<WarrantiesController> _logger;

        public WarrantiesController(ApplicationDbContext context, IWarrantyService warrantyService, IMapper mapper, ILogger<WarrantiesController> logger) // <-- TIÊM IWarrantyService
        {
            _context = context;
            _warrantyService = warrantyService;
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/Warranties
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WarrantyDto>>> GetWarranties()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách bảo hành.");
            var warranties = await _warrantyService.GetWarrantiesAsync();
            return Ok(warranties);
        }

        // API: GET api/Warranties/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<WarrantyDto>> GetWarranty(string id)
        {
            _logger.LogInformation("Yêu cầu lấy bảo hành với ID: {WarrantyID}", id);
            var warrantyDto = await _warrantyService.GetWarrantyByIdAsync(id);

            if (warrantyDto == null)
            {
                _logger.LogWarning("Không tìm thấy bảo hành với ID: {WarrantyID}", id);
                return NotFound();
            }

            return Ok(warrantyDto);
        }

        // API: POST api/Warranties
        [HttpPost]
        public async Task<ActionResult<WarrantyDto>> PostWarranty(Warranty warranty) // Vẫn nhận Model
        {
            _logger.LogInformation("Yêu cầu tạo bảo hành mới cho OrderID: {OrderID}", warranty.OrderID);

            // --- KIỂM TRA FKs CHÍNH TRƯỚC KHI GỬI VÀO SERVICE ---
            if (!string.IsNullOrEmpty(warranty.OrderID) && !OrderExists(warranty.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }

            try
            {
                var createdWarrantyDto = await _warrantyService.CreateWarrantyAsync(warranty);
                _logger.LogInformation("Đã tạo bảo hành mới với ID: {WarrantyID}", createdWarrantyDto.WarrantyID);
                return CreatedAtAction(nameof(GetWarranty), new { id = createdWarrantyDto.WarrantyID }, createdWarrantyDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi tạo bảo hành: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/Warranties/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWarranty(string id, Warranty warranty)
        {
            _logger.LogInformation("Yêu cầu cập nhật bảo hành với ID: {WarrantyID}", id);
            if (id != warranty.WarrantyID)
            {
                return BadRequest();
            }

            // Kiểm tra FKs chính
            if (!string.IsNullOrEmpty(warranty.OrderID) && !OrderExists(warranty.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }

            try
            {
                var success = await _warrantyService.UpdateWarrantyAsync(id, warranty);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy bảo hành để cập nhật với ID: {WarrantyID}", id);
                    return NotFound();
                }

                _logger.LogInformation("Đã cập nhật bảo hành với ID: {WarrantyID}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi cập nhật bảo hành: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WarrantyExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // API: DELETE api/Warranties/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWarranty(string id)
        {
            _logger.LogInformation("Yêu cầu xóa bảo hành với ID: {WarrantyID}", id);
            var success = await _warrantyService.DeleteWarrantyAsync(id);

            if (!success)
            {
                _logger.LogWarning("Không tìm thấy bảo hành để xóa với ID: {WarrantyID}", id);
                return NotFound();
            }

            return NoContent();
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool WarrantyExists(string id) { return _context.Warranties.Any(e => e.WarrantyID == id); }
        private bool OrderExists(string id) { return _context.Orders.Any(e => e.OrderID == id); }
    }
}
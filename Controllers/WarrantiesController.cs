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
    [Authorize(Roles = "Admin,Manager,Sales")]
    public class WarrantiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWarrantyService _warrantyService;
        private readonly IMapper _mapper;
        private readonly ILogger<WarrantiesController> _logger;

        public WarrantiesController(ApplicationDbContext context, IWarrantyService warrantyService, IMapper mapper, ILogger<WarrantiesController> logger)
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
        public async Task<ActionResult<WarrantyDto>> PostWarranty(Warranty warranty)
        {
            _logger.LogInformation("Yêu cầu tạo bảo hành mới cho VehicleID: {VehicleID}", warranty.VehicleID);

            // Kiểm tra VehicleID (FK) trước khi gọi Service
            if (!string.IsNullOrEmpty(warranty.VehicleID) && !CustomerVehicleExists(warranty.VehicleID))
            {
                return BadRequest("VehicleID không tồn tại.");
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

            // Kiểm tra VehicleID (FK) trước khi gọi Service
            if (!string.IsNullOrEmpty(warranty.VehicleID) && !CustomerVehicleExists(warranty.VehicleID))
            {
                return BadRequest("VehicleID không tồn tại.");
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
            try
            {
                var success = await _warrantyService.DeleteWarrantyAsync(id);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy bảo hành để xóa với ID: {WarrantyID}", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi xóa bảo hành: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool WarrantyExists(string id) { return _context.Warranties.Any(e => e.WarrantyID == id); }
        private bool CustomerVehicleExists(string id) { return _context.CustomerVehicles.Any(e => e.VehicleID == id); }
    }
}
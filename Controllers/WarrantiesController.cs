// DecalXeAPI/Controllers/WarrantiesController.cs
using Microsoft.AspNetCore.Mvc;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.EntityFrameworkCore;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Sales")]
    public class WarrantiesController : ControllerBase
    {
        private readonly IWarrantyService _warrantyService;
        private readonly ILogger<WarrantiesController> _logger;

        public WarrantiesController(IWarrantyService warrantyService, ILogger<WarrantiesController> logger)
        {
            _warrantyService = warrantyService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WarrantyDto>>> GetWarranties()
        {
            var warranties = await _warrantyService.GetWarrantiesAsync();
            return Ok(warranties);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WarrantyDto>> GetWarranty(string id)
        {
            var warrantyDto = await _warrantyService.GetWarrantyByIdAsync(id);
            if (warrantyDto == null) return NotFound();
            return Ok(warrantyDto);
        }

        [HttpPost]
        public async Task<ActionResult<WarrantyDto>> PostWarranty(Warranty warranty)
        {
            _logger.LogInformation("Yêu cầu tạo bảo hành mới cho VehicleID: {VehicleID}", warranty.VehicleID);
            try
            {
                var createdWarrantyDto = await _warrantyService.CreateWarrantyAsync(warranty);
                return CreatedAtAction(nameof(GetWarranty), new { id = createdWarrantyDto.WarrantyID }, createdWarrantyDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutWarranty(string id, Warranty warranty)
        {
            if (id != warranty.WarrantyID) return BadRequest();

            try
            {
                var success = await _warrantyService.UpdateWarrantyAsync(id, warranty);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Lỗi xung đột dữ liệu, vui lòng thử lại.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWarranty(string id)
        {
            var success = await _warrantyService.DeleteWarrantyAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
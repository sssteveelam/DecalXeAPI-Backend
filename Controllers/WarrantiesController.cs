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
using AutoMapper;
using DecalXeAPI.Data;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Sales")]
    public class WarrantiesController : ControllerBase
    {
        private readonly IWarrantyService _warrantyService;
        private readonly ILogger<WarrantiesController> _logger;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public WarrantiesController(IWarrantyService warrantyService, ILogger<WarrantiesController> logger, IMapper mapper, ApplicationDbContext context)
        {
            _warrantyService = warrantyService;
            _logger = logger;
            _mapper = mapper;
            _context = context;
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

        // API: POST api/Warranties (ĐÃ NÂNG CẤP)
        [HttpPost]
        public async Task<ActionResult<WarrantyDto>> PostWarranty(CreateWarrantyDto createDto)
        {
            _logger.LogInformation("Yêu cầu tạo bảo hành mới cho VehicleID: {VehicleID}", createDto.VehicleID);
            
            var warranty = _mapper.Map<Warranty>(createDto);
            // Server sẽ tự gán các giá trị mặc định khi tạo
            warranty.WarrantyStatus = "Active";

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

        // API: PUT api/Warranties/{id} (ĐÃ NÂNG CẤP)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWarranty(string id, UpdateWarrantyDto updateDto)
        {
            var warranty = await _context.Warranties.FindAsync(id);
            if (warranty == null)
            {
                return NotFound();
            }
            
            _mapper.Map(updateDto, warranty);
            
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
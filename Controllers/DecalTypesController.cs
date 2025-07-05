using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // <-- THÊM DÒNG NÀY (Để sử dụng IDecalTypeService)
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")] // Quyền cho DecalTypesController
    public class DecalTypesController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists cơ bản
        private readonly IDecalTypeService _decalTypeService; // <-- KHAI BÁO BIẾN CHO SERVICE
        private readonly IMapper _mapper;
        private readonly ILogger<DecalTypesController> _logger;

        public DecalTypesController(ApplicationDbContext context, IDecalTypeService decalTypeService, IMapper mapper, ILogger<DecalTypesController> logger) // <-- TIÊM IDecalTypeService
        {
            _context = context;
            _decalTypeService = decalTypeService;
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/DecalTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DecalTypeDto>>> GetDecalTypes()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách loại decal.");
            var decalTypes = await _decalTypeService.GetDecalTypesAsync();
            return Ok(decalTypes);
        }

        // API: GET api/DecalTypes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<DecalTypeDto>> GetDecalType(string id)
        {
            _logger.LogInformation("Yêu cầu lấy loại decal với ID: {DecalTypeID}", id);
            var decalTypeDto = await _decalTypeService.GetDecalTypeByIdAsync(id);

            if (decalTypeDto == null)
            {
                _logger.LogWarning("Không tìm thấy loại decal với ID: {DecalTypeID}", id);
                return NotFound();
            }

            return Ok(decalTypeDto);
        }

        // API: POST api/DecalTypes (ĐÃ NÂNG CẤP)
        [HttpPost]
        public async Task<ActionResult<DecalTypeDto>> PostDecalType(CreateDecalTypeDto createDto)
        {
            _logger.LogInformation("Yêu cầu tạo loại decal mới: {DecalTypeName}", createDto.DecalTypeName);
            
            var decalType = _mapper.Map<DecalType>(createDto);

            try
            {
                var createdDecalTypeDto = await _decalTypeService.CreateDecalTypeAsync(decalType);
                _logger.LogInformation("Đã tạo loại decal mới với ID: {DecalTypeID}", createdDecalTypeDto.DecalTypeID);
                return CreatedAtAction(nameof(GetDecalType), new { id = createdDecalTypeDto.DecalTypeID }, createdDecalTypeDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/DecalTypes/{id} (ĐÃ NÂNG CẤP)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDecalType(string id, UpdateDecalTypeDto updateDto)
        {
            _logger.LogInformation("Yêu cầu cập nhật loại decal với ID: {DecalTypeID}", id);
            
            var decalType = await _context.DecalTypes.FindAsync(id);
            if (decalType == null)
            {
                return NotFound();
            }

            _mapper.Map(updateDto, decalType);

            try
            {
                var success = await _decalTypeService.UpdateDecalTypeAsync(id, decalType);
                if (!success) return NotFound();

                _logger.LogInformation("Đã cập nhật loại decal với ID: {DecalTypeID}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DecalTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // API: DELETE api/DecalTypes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDecalType(string id)
        {
            _logger.LogInformation("Yêu cầu xóa loại decal với ID: {DecalTypeID}", id);
            var success = await _decalTypeService.DeleteDecalTypeAsync(id);

            if (!success)
            {
                _logger.LogWarning("Không tìm thấy loại decal để xóa với ID: {DecalTypeID}", id);
                return NotFound();
            }

            return NoContent();
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool DecalTypeExists(string id) { return _context.DecalTypes.Any(e => e.DecalTypeID == id); }
    }
}
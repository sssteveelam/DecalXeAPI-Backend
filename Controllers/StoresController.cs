using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // <-- THÊM DÒNG NÀY (Để sử dụng IStoreService)
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System; // Để sử dụng ArgumentException

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")] // Quyền cho StoresController
    public class StoresController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists cơ bản
        private readonly IStoreService _storeService; // <-- KHAI BÁO BIẾN CHO SERVICE
        private readonly IMapper _mapper;
        private readonly ILogger<StoresController> _logger;

        public StoresController(ApplicationDbContext context, IStoreService storeService, IMapper mapper, ILogger<StoresController> logger) // <-- TIÊM IStoreService
        {
            _context = context;
            _storeService = storeService;
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/Stores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StoreDto>>> GetStores()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách cửa hàng.");
            var stores = await _storeService.GetStoresAsync();
            return Ok(stores);
        }

        // API: GET api/Stores/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<StoreDto>> GetStore(string id)
        {
            _logger.LogInformation("Yêu cầu lấy cửa hàng với ID: {StoreID}", id);
            var storeDto = await _storeService.GetStoreByIdAsync(id);

            if (storeDto == null)
            {
                _logger.LogWarning("Không tìm thấy cửa hàng với ID: {StoreID}", id);
                return NotFound();
            }

            return Ok(storeDto);
        }

        // API: POST api/Stores (ĐÃ NÂNG CẤP)
        [HttpPost]
        public async Task<ActionResult<StoreDto>> PostStore(CreateStoreDto createDto)
        {
            _logger.LogInformation("Yêu cầu tạo cửa hàng mới: {StoreName}", createDto.StoreName);
            var store = _mapper.Map<Store>(createDto);
            try
            {
                var createdStoreDto = await _storeService.CreateStoreAsync(store);
                return CreatedAtAction(nameof(GetStore), new { id = createdStoreDto.StoreID }, createdStoreDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/Stores/{id} (ĐÃ NÂNG CẤP)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStore(string id, UpdateStoreDto updateDto)
        {
            _logger.LogInformation("Yêu cầu cập nhật cửa hàng với ID: {StoreID}", id);
            var store = await _context.Stores.FindAsync(id);
            if (store == null)
            {
                return NotFound();
            }

            _mapper.Map(updateDto, store);

            try
            {
                var success = await _storeService.UpdateStoreAsync(id, store);
                if (!success) return NotFound();
                
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Trong file: DecalXeAPI/Controllers/StoresController.cs
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteStore(string id)
        {
            _logger.LogInformation("Yêu cầu xóa cửa hàng với ID: {StoreID}", id);
            try
            {
                var success = await _storeService.DeleteStoreAsync(id);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy cửa hàng để xóa với ID: {StoreID}", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex) // Bắt lỗi nghiệp vụ cụ thể từ Service
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi xóa cửa hàng {StoreID}: {ErrorMessage}", id, ex.Message);
                // Trả về lỗi 400 Bad Request với thông báo từ Service
                return BadRequest(new { message = ex.Message });
            }
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool StoreExists(string id) { return _context.Stores.Any(e => e.StoreID == id); }
    }
}
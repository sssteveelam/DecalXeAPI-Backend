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

        // API: POST api/Stores
        [HttpPost]
        public async Task<ActionResult<StoreDto>> PostStore(Store store) // Vẫn nhận Store Model
        {
            _logger.LogInformation("Yêu cầu tạo cửa hàng mới: {StoreName}", store.StoreName);
            try
            {
                var createdStoreDto = await _storeService.CreateStoreAsync(store);
                _logger.LogInformation("Đã tạo cửa hàng mới với ID: {StoreID}", createdStoreDto.StoreID);
                return CreatedAtAction(nameof(GetStore), new { id = createdStoreDto.StoreID }, createdStoreDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi tạo cửa hàng: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/Stores/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStore(string id, Store store)
        {
            _logger.LogInformation("Yêu cầu cập nhật cửa hàng với ID: {StoreID}", id);
            if (id != store.StoreID)
            {
                return BadRequest();
            }

            try
            {
                var success = await _storeService.UpdateStoreAsync(id, store);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy cửa hàng để cập nhật với ID: {StoreID}", id);
                    return NotFound();
                }

                _logger.LogInformation("Đã cập nhật cửa hàng với ID: {StoreID}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi cập nhật cửa hàng: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StoreExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // API: DELETE api/Stores/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStore(string id)
        {
            _logger.LogInformation("Yêu cầu xóa cửa hàng với ID: {StoreID}", id);
            var success = await _storeService.DeleteStoreAsync(id);

            if (!success)
            {
                _logger.LogWarning("Không tìm thấy cửa hàng để xóa với ID: {StoreID}", id);
                return NotFound();
            }

            return NoContent();
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool StoreExists(string id) { return _context.Stores.Any(e => e.StoreID == id); }
    }
}
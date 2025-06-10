using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Vẫn cần DbContext cho các hàm Exists cơ bản
using DecalXeAPI.Data; // Vẫn cần ApplicationDbContext cho các hàm Exists
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // <-- THÊM DÒNG NÀY (Để sử dụng IRoleService)
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System; // Để sử dụng ArgumentException hoặc InvalidOperationException

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")] // Quyền cho RolesController
    public class RolesController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists cơ bản
        private readonly IRoleService _roleService; // <-- KHAI BÁO BIẾN CHO SERVICE
        private readonly IMapper _mapper; // Vẫn giữ để ánh xạ DTOs nếu có
        private readonly ILogger<RolesController> _logger; // Logger cho Controller

        public RolesController(ApplicationDbContext context, IRoleService roleService, IMapper mapper, ILogger<RolesController> logger) // <-- TIÊM IRoleService
        {
            _context = context;
            _roleService = roleService;
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/Roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách vai trò.");
            var roles = await _roleService.GetRolesAsync();
            return Ok(roles);
        }

        // API: GET api/Roles/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDto>> GetRole(string id)
        {
            _logger.LogInformation("Yêu cầu lấy vai trò với ID: {RoleID}", id);
            var roleDto = await _roleService.GetRoleByIdAsync(id);

            if (roleDto == null)
            {
                _logger.LogWarning("Không tìm thấy vai trò với ID: {RoleID}", id);
                return NotFound();
            }

            return Ok(roleDto);
        }

        // API: POST api/Roles
        [HttpPost]
        public async Task<ActionResult<RoleDto>> PostRole(Role role) // Vẫn nhận Role Model
        {
            _logger.LogInformation("Yêu cầu tạo vai trò mới: {RoleName}", role.RoleName);
            try
            {
                var createdRoleDto = await _roleService.CreateRoleAsync(role);
                _logger.LogInformation("Đã tạo vai trò mới với ID: {RoleID}", createdRoleDto.RoleID);
                return CreatedAtAction(nameof(GetRole), new { id = createdRoleDto.RoleID }, createdRoleDto);
            }
            catch (ArgumentException ex) // Bắt lỗi từ Service nếu có (ví dụ: RoleName đã tồn tại)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi tạo vai trò: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/Roles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRole(string id, Role role)
        {
            _logger.LogInformation("Yêu cầu cập nhật vai trò với ID: {RoleID}", id);
            if (id != role.RoleID)
            {
                return BadRequest();
            }

            try
            {
                var success = await _roleService.UpdateRoleAsync(id, role);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy vai trò để cập nhật với ID: {RoleID}", id);
                    return NotFound();
                }

                _logger.LogInformation("Đã cập nhật vai trò với ID: {RoleID}", id);
                return NoContent();
            }
            catch (ArgumentException ex) // Bắt lỗi từ Service nếu có (ví dụ: RoleName trùng lặp)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi cập nhật vai trò: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (DbUpdateConcurrencyException) // Vẫn bắt riêng lỗi này ở Controller
            {
                if (!RoleExists(id)) // Vẫn dùng hàm hỗ trợ của Controller để kiểm tra tồn tại
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // API: DELETE api/Roles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            _logger.LogInformation("Yêu cầu xóa vai trò với ID: {RoleID}", id);
            try
            {
                var success = await _roleService.DeleteRoleAsync(id);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy vai trò để xóa với ID: {RoleID}", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex) // Bắt lỗi nghiệp vụ nếu Role đang được sử dụng
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi xóa vai trò: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool RoleExists(string id) { return _context.Roles.Any(e => e.RoleID == id); }
    }
}
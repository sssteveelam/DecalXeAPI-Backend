using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace DecalXeAPI.Services.Implementations
{
    public class RoleService : IRoleService // <-- Kế thừa từ IRoleService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<RoleService> _logger;

        public RoleService(ApplicationDbContext context, IMapper mapper, ILogger<RoleService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<RoleDto>> GetRolesAsync()
        {
            _logger.LogInformation("Lấy danh sách vai trò.");
            var roles = await _context.Roles.ToListAsync();
            var roleDtos = _mapper.Map<List<RoleDto>>(roles);
            return roleDtos;
        }

        public async Task<RoleDto?> GetRoleByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy vai trò với ID: {RoleID}", id);
            var role = await _context.Roles.FindAsync(id);

            if (role == null)
            {
                _logger.LogWarning("Không tìm thấy vai trò với ID: {RoleID}", id);
                return null;
            }

            var roleDto = _mapper.Map<RoleDto>(role);
            _logger.LogInformation("Đã trả về vai trò với ID: {RoleID}", id);
            return roleDto;
        }

        public async Task<RoleDto> CreateRoleAsync(Role role)
        {
            _logger.LogInformation("Yêu cầu tạo vai trò mới: {RoleName}", role.RoleName);

            // Kiểm tra xem RoleName đã tồn tại chưa
            if (await _context.Roles.AnyAsync(r => r.RoleName == role.RoleName))
            {
                _logger.LogWarning("RoleName đã tồn tại: {RoleName}", role.RoleName);
                throw new ArgumentException("RoleName đã tồn tại.");
            }

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            var roleDto = _mapper.Map<RoleDto>(role);
            _logger.LogInformation("Đã tạo vai trò mới với ID: {RoleID}", role.RoleID);
            return roleDto;
        }

        public async Task<bool> UpdateRoleAsync(string id, Role role)
        {
            _logger.LogInformation("Yêu cầu cập nhật vai trò với ID: {RoleID}", id);

            if (id != role.RoleID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với RoleID trong body ({RoleIDBody})", id, role.RoleID);
                return false;
            }

            if (!await RoleExistsAsync(id))
            {
                _logger.LogWarning("Không tìm thấy vai trò để cập nhật với ID: {RoleID}", id);
                return false;
            }

            // Kiểm tra xem RoleName có thay đổi và có trùng lặp không
            if (await _context.Roles.AnyAsync(r => r.RoleName == role.RoleName && r.RoleID != id))
            {
                _logger.LogWarning("RoleName đã tồn tại khi cập nhật: {RoleName}", role.RoleName);
                throw new ArgumentException("RoleName đã tồn tại.");
            }

            _context.Entry(role).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật vai trò với ID: {RoleID}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật vai trò với ID: {RoleID}", id);
                throw;
            }
        }

        public async Task<bool> DeleteRoleAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa vai trò với ID: {RoleID}", id);
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                _logger.LogWarning("Không tìm thấy vai trò để xóa với ID: {RoleID}", id);
                return false;
            }

            // Kiểm tra xem có Account nào đang sử dụng Role này không trước khi xóa
            if (await _context.Accounts.AnyAsync(a => a.RoleID == id))
            {
                _logger.LogWarning("Không thể xóa vai trò {RoleID} vì có tài khoản đang sử dụng.", id);
                throw new InvalidOperationException("Không thể xóa vai trò này vì có tài khoản đang sử dụng.");
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa vai trò với ID: {RoleID}", id);
            return true;
        }

        // Hàm kiểm tra tồn tại (PUBLIC cho INTERFACE)
        public async Task<bool> RoleExistsAsync(string id)
        {
            return await _context.Roles.AnyAsync(e => e.RoleID == id);
        }
    }
}
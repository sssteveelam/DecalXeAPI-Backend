using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetRolesAsync();
        Task<RoleDto?> GetRoleByIdAsync(string id);
        Task<RoleDto> CreateRoleAsync(Role role);
        Task<bool> UpdateRoleAsync(string id, Role role);
        Task<bool> DeleteRoleAsync(string id);

        // Hàm kiểm tra tồn tại (Exists) cần thiết cho Service này
        Task<bool> RoleExistsAsync(string id);
    }
}
// File: Services/Interfaces/IAccountService.cs
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IAccountService
    {
        Task<IEnumerable<AccountDto>> GetAccountsAsync();
        Task<AccountDto?> GetAccountByIdAsync(string id);
        Task<AccountDto> CreateAccountAsync(Account account);
        
        Task<bool> DeleteAccountAsync(string id);

        // --- PHƯƠNG THỨC MỚI CHO TÍNH NĂNG ĐỔI MẬT KHẨU (CÓ XÁC MINH MẬT KHẨU CŨ) ---
        Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(string accountId, ChangePasswordRequestDto request);

        // --- MỚI: PHƯƠNG THỨC CHO TÍNH NĂNG QUÊN MẬT KHẨU (ĐƠN GIẢN: RESET BẰNG USERNAME) ---
        Task<(bool Success, string? ErrorMessage)> ResetPasswordByUsernameAsync(ResetPasswordByUsernameDto request); // <-- THÊM DÒNG NÀY

        // Các hàm kiểm tra tồn tại (Exists) cần thiết cho Service này
        Task<bool> AccountExistsAsync(string id);
        Task<bool> RoleExistsAsync(string id);

        Task<(bool Success, string? ErrorMessage)> UpdateAccountAsync(string id, UpdateAccountDto updateDto);

    }
}
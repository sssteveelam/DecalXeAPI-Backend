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
        Task<bool> UpdateAccountAsync(string id, Account account);
        Task<bool> DeleteAccountAsync(string id);


  // --- PHƯƠNG THỨC MỚI CHO TÍNH NĂNG QUÊN MẬT KHẨU ---
        Task<(bool Success, string? ErrorMessage)> ForgotPasswordAsync(ForgotPasswordRequestDto request);
        Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(ResetPasswordRequestDto request);


        // Các hàm kiểm tra tồn tại (Exists) cần thiết cho Service này
        Task<bool> AccountExistsAsync(string id);
        Task<bool> RoleExistsAsync(string id); // Cần để kiểm tra FK
    }
}
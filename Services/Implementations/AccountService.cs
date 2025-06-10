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
using System; // Để sử dụng ArgumentException

namespace DecalXeAPI.Services.Implementations
{
    public class AccountService : IAccountService // <-- Kế thừa từ IAccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountService> _logger;

        public AccountService(ApplicationDbContext context, IMapper mapper, ILogger<AccountService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<AccountDto>> GetAccountsAsync()
        {
            _logger.LogInformation("Lấy danh sách tài khoản.");
            var accounts = await _context.Accounts.Include(a => a.Role).ToListAsync();
            var accountDtos = _mapper.Map<List<AccountDto>>(accounts);
            return accountDtos;
        }

        public async Task<AccountDto?> GetAccountByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy tài khoản với ID: {AccountID}", id);
            var account = await _context.Accounts.Include(a => a.Role).FirstOrDefaultAsync(a => a.AccountID == id);

            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản với ID: {AccountID}", id);
                return null;
            }

            var accountDto = _mapper.Map<AccountDto>(account);
            _logger.LogInformation("Đã trả về tài khoản với ID: {AccountID}", id);
            return accountDto;
        }

        public async Task<AccountDto> CreateAccountAsync(Account account)
        {
            _logger.LogInformation("Yêu cầu tạo tài khoản mới: {Username}", account.Username);

            // Kiểm tra username đã tồn tại chưa
            if (await _context.Accounts.AnyAsync(a => a.Username == account.Username))
            {
                _logger.LogWarning("Username đã tồn tại: {Username}", account.Username);
                throw new ArgumentException("Username đã tồn tại.");
            }

            // Kiểm tra RoleID có tồn tại không
            if (!string.IsNullOrEmpty(account.RoleID) && !await RoleExistsAsync(account.RoleID))
            {
                _logger.LogWarning("RoleID không tồn tại: {RoleID}", account.RoleID);
                throw new ArgumentException("RoleID không tồn tại.");
            }

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            await _context.Entry(account).Reference(a => a.Role).LoadAsync();

            var accountDto = _mapper.Map<AccountDto>(account);
            _logger.LogInformation("Đã tạo tài khoản mới với ID: {AccountID}", account.AccountID);
            return accountDto;
        }

        public async Task<bool> UpdateAccountAsync(string id, Account account)
        {
            _logger.LogInformation("Yêu cầu cập nhật tài khoản với ID: {AccountID}", id);

            if (id != account.AccountID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với AccountID trong body ({AccountIDBody})", id, account.AccountID);
                return false;
            }

            if (!await AccountExistsAsync(id))
            {
                _logger.LogWarning("Không tìm thấy tài khoản để cập nhật với ID: {AccountID}", id);
                return false;
            }

            // Kiểm tra username nếu thay đổi và trùng lặp
            if (await _context.Accounts.AnyAsync(a => a.Username == account.Username && a.AccountID != id))
            {
                _logger.LogWarning("Username đã tồn tại khi cập nhật: {Username}", account.Username);
                throw new ArgumentException("Username đã tồn tại.");
            }

            // Kiểm tra RoleID có tồn tại không
            if (!string.IsNullOrEmpty(account.RoleID) && !await RoleExistsAsync(account.RoleID))
            {
                _logger.LogWarning("RoleID không tồn tại: {RoleID}", account.RoleID);
                throw new ArgumentException("RoleID không tồn tại.");
            }

            _context.Entry(account).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật tài khoản với ID: {AccountID}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật tài khoản với ID: {AccountID}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAccountAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa tài khoản với ID: {AccountID}", id);
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản để xóa với ID: {AccountID}", id);
                return false;
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa tài khoản với ID: {AccountID}", id);
            return true;
        }

        // --- HÀM HỖ TRỢ: KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (PUBLIC CHO INTERFACE) ---
        public async Task<bool> AccountExistsAsync(string id)
        {
            return await _context.Accounts.AnyAsync(e => e.AccountID == id);
        }

        public async Task<bool> RoleExistsAsync(string id)
        {
            return await _context.Roles.AnyAsync(e => e.RoleID == id);
        }
    }
}
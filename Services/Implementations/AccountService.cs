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
// using System.Security.Cryptography; // <-- ĐÃ XÓA DÒNG NÀY VÌ KHÔNG TẠO TOKEN NỮA

namespace DecalXeAPI.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountService> _logger;
        // private readonly IEmailService _emailService; // <-- ĐÃ XÓA DÒNG NÀY VÌ KHÔNG DÙNG EMAIL NỮA

        public AccountService(ApplicationDbContext context, IMapper mapper, ILogger<AccountService> logger /*, IEmailService emailService */) // <-- BỎ TIÊM IEmailService
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            // _emailService = emailService;
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

            if (await _context.Accounts.AnyAsync(a => a.Username == account.Username))
            {
                _logger.LogWarning("Username đã tồn tại: {Username}", account.Username);
                throw new ArgumentException("Username đã tồn tại.");
            }

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


        public async Task<(bool Success, string? ErrorMessage)> UpdateAccountAsync(string id, UpdateAccountDto updateDto)
        {
            _logger.LogInformation("Service bắt đầu cập nhật tài khoản với ID: {AccountID}", id);

            var accountToUpdate = await _context.Accounts.FindAsync(id);
            if (accountToUpdate == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản để cập nhật với ID: {AccountID}", id);
                return (false, "Tài khoản không tồn tại.");
            }

            // Kiểm tra username có bị trùng với người khác không
            if (await _context.Accounts.AnyAsync(a => a.Username == updateDto.Username && a.AccountID != id))
            {
                _logger.LogWarning("Username đã tồn tại khi cập nhật: {Username}", updateDto.Username);
                return (false, "Username đã tồn tại.");
            }

            // Dùng AutoMapper để ánh xạ các trường từ DTO vào đối tượng đã tìm được
            // Bước này sẽ chỉ cập nhật các trường có trong UpdateAccountDto
            // và bỏ qua PasswordHash một cách an toàn!
            _mapper.Map(updateDto, accountToUpdate);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật tài khoản với ID: {AccountID} thành công.", id);
                return (true, null);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật tài khoản với ID: {AccountID}", id);
                throw;
            }
        }       

        // Trong file: DecalXeAPI/Services/Implementations/AccountService.cs
        public async Task<bool> DeleteAccountAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa tài khoản với ID: {AccountID}", id);
            var account = await _context.Accounts
                                        .Include(a => a.Employee) // Nạp thông tin Employee liên quan
                                        .FirstOrDefaultAsync(a => a.AccountID == id);

            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản để xóa với ID: {AccountID}", id);
                return false;
            }

            // --- LOGIC MỚI: KIỂM TRA CÔNG VIỆC DANG DỞ CỦA NHÂN VIÊN ---
            if (account.Employee != null)
            {
                var employeeId = account.Employee.EmployeeID;

                // 1. Kiểm tra các đơn hàng đang hoạt động (chưa hoàn thành hoặc chưa hủy)
                bool hasActiveOrders = await _context.Orders
                    .AnyAsync(o => o.AssignedEmployeeID == employeeId && o.OrderStatus != "Completed" && o.OrderStatus != "Cancelled");
                if (hasActiveOrders)
                {
                    _logger.LogWarning("Không thể xóa tài khoản {AccountID} vì nhân viên đang được giao các đơn hàng chưa hoàn thành.", id);
                    throw new InvalidOperationException("Không thể xóa tài khoản này vì nhân viên đang được giao các đơn hàng chưa hoàn thành.");
                }


                // 3. Kiểm tra các công việc thiết kế đang hoạt động
                bool hasActiveDesigns = await _context.Designs
                    .AnyAsync(d => d.DesignerID == employeeId && d.ApprovalStatus != "Approved" && d.ApprovalStatus != "Rejected");
                if (hasActiveDesigns)
                {
                    _logger.LogWarning("Không thể xóa tài khoản {AccountID} vì nhân viên đang có các công việc thiết kế chưa hoàn tất.", id);
                    throw new InvalidOperationException("Không thể xóa tài khoản này vì nhân viên đang có các công việc thiết kế chưa hoàn tất.");
                }
            }
            // --- KẾT THÚC LOGIC MỚI ---
            
            // Logic cũ kiểm tra ràng buộc khóa ngoại vẫn cần thiết cho các trường hợp khác
            if (await _context.Customers.AnyAsync(c => c.AccountID == id) || await _context.Employees.AnyAsync(e => e.AccountID == id) || await _context.DesignComments.AnyAsync(dc => dc.SenderAccountID == id))
            {
                _logger.LogWarning("Không thể xóa tài khoản {AccountID} vì có các ràng buộc dữ liệu khác.", id);
                throw new InvalidOperationException("Không thể xóa tài khoản này vì đang được liên kết với khách hàng, nhân viên hoặc các bình luận.");
            }


            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa tài khoản với ID: {AccountID}", id);
            return true;
        }

        // --- PHƯƠNG THỨC MỚI CHO TÍNH NĂNG ĐỔI MẬT KHẨU (CÓ XÁC MINH MẬT KHẨU CŨ) ---
        // Yêu cầu đổi mật khẩu (xác minh mật khẩu cũ và đặt mật khẩu mới)
        public async Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(string accountId, ChangePasswordRequestDto request)
        {
            _logger.LogInformation("Yêu cầu đổi mật khẩu cho AccountID: {AccountID}", accountId);

            // 1. Tìm tài khoản
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản để đổi mật khẩu với ID: {AccountID}", accountId);
                return (false, "Tài khoản không tồn tại.");
            }

            // 2. Xác minh mật khẩu cũ (So sánh mật khẩu đã hash trong thực tế)
            if (account.PasswordHash != request.OldPassword) // Tạm thời so sánh chuỗi trực tiếp
            {
                _logger.LogWarning("Mật khẩu cũ không đúng cho Account {AccountID}", accountId);
                return (false, "Mật khẩu cũ không đúng.");
            }

            // 3. Kiểm tra mật khẩu mới và xác nhận
            if (request.NewPassword != request.ConfirmNewPassword)
            {
                _logger.LogWarning("Mật khẩu mới và xác nhận mật khẩu không khớp cho Account {AccountID}", accountId);
                return (false, "Mật khẩu mới và xác nhận mật khẩu không khớp.");
            }

            // (Thêm kiểm tra độ mạnh mật khẩu nếu cần: MinLength, ContainsDigit, ContainsSpecialChar...)
            if (request.NewPassword.Length < 6) // Ví dụ kiểm tra độ dài tối thiểu
            {
                return (false, "Mật khẩu mới phải có ít nhất 6 ký tự.");
            }

            // 4. Hash mật khẩu mới (Nếu bạn đang hash mật khẩu khi tạo tài khoản, hãy hash ở đây)
            account.PasswordHash = request.NewPassword; // Tạm thời lưu plaintext, thực tế PHẢI HASH

            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Mật khẩu cho Account {AccountID} đã được đổi thành công.", account.AccountID);

            return (true, null);
        }

        // --- MỚI: PHƯƠNG THỨC CHO TÍNH NĂNG QUÊN MẬT KHẨU (ĐƠN GIẢN: RESET BẰNG USERNAME) ---
        public async Task<(bool Success, string? ErrorMessage)> ResetPasswordByUsernameAsync(ResetPasswordByUsernameDto request)
        {
            _logger.LogInformation("Yêu cầu đặt lại mật khẩu bằng Username: {Username}", request.Username);

            // 1. Tìm tài khoản bằng Username
            var account = await _context.Accounts
                                        .FirstOrDefaultAsync(a => a.Username == request.Username);

            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản cho yêu cầu đặt lại mật khẩu bằng Username: {Username}. (Để bảo mật, không tiết lộ tài khoản có tồn tại hay không).", request.Username);
                // Để bảo mật, luôn trả về thành công để tránh lộ thông tin người dùng.
                return (true, null);
            }

            // 2. Kiểm tra mật khẩu mới và xác nhận
            if (request.NewPassword != request.ConfirmNewPassword)
            {
                _logger.LogWarning("Mật khẩu mới và xác nhận mật khẩu không khớp cho Account {AccountID} khi reset bằng Username", account.AccountID);
                return (false, "Mật khẩu mới và xác nhận mật khẩu không khớp.");
            }

            // 3. Hash mật khẩu mới
            account.PasswordHash = request.NewPassword; // Tạm thời lưu plaintext, thực tế PHẢI HASH

            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Mật khẩu cho Account {AccountID} đã được đặt lại thành công bằng Username.", account.AccountID);

            return (true, null);
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
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces; // Cần cho IEmailService
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Security.Cryptography; // Cần cho việc tạo token ngẫu nhiên

namespace DecalXeAPI.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountService> _logger;
        private readonly IEmailService _emailService; // <-- KHAI BÁO BIẾN EMAIL SERVICE

        public AccountService(ApplicationDbContext context, IMapper mapper, ILogger<AccountService> logger, IEmailService emailService) // <-- TIÊM EMAIL SERVICE
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService; // Gán Email Service
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

            if (await _context.Accounts.AnyAsync(a => a.Username == account.Username && a.AccountID != id))
            {
                _logger.LogWarning("Username đã tồn tại khi cập nhật: {Username}", account.Username);
                throw new ArgumentException("Username đã tồn tại.");
            }

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

            if (await _context.Customers.AnyAsync(c => c.AccountID == id) || await _context.Employees.AnyAsync(e => e.AccountID == id) || await _context.DesignComments.AnyAsync(dc => dc.SenderAccountID == id))
            {
                _logger.LogWarning("Không thể xóa tài khoản {AccountID} vì đang được sử dụng bởi khách hàng, nhân viên hoặc bình luận thiết kế.", id);
                throw new InvalidOperationException("Không thể xóa tài khoản này vì đang được sử dụng bởi khách hàng, nhân viên hoặc bình luận thiết kế.");
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa tài khoản với ID: {AccountID}", id);
            return true;
        }

        // --- PHƯƠNG THỨC MỚI CHO TÍNH NĂNG QUÊN MẬT KHẨU ---
        // Yêu cầu đặt lại mật khẩu (tạo token và gửi email)
        public async Task<(bool Success, string? ErrorMessage)> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            _logger.LogInformation("Yêu cầu đặt lại mật khẩu cho Identifier: {Identifier}", request.Identifier);

            // 1. Tìm tài khoản bằng Username hoặc Email
            var account = await _context.Accounts
                                        .FirstOrDefaultAsync(a => a.Username == request.Identifier || a.Email == request.Identifier);

            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản cho yêu cầu đặt lại mật khẩu: {Identifier}. (Để bảo mật, không tiết lộ tài khoản có tồn tại hay không).", request.Identifier);
                // Để bảo mật, luôn trả về thành công ngay cả khi tài khoản không tồn tại
                // để tránh lộ thông tin người dùng. Tuy nhiên, không gửi email.
                return (true, null);
            }
            
            // Đảm bảo tài khoản có email để gửi (nếu dùng email làm kênh gửi)
            if (string.IsNullOrEmpty(account.Email))
            {
                 _logger.LogWarning("Tài khoản {Username} không có email để gửi đặt lại mật khẩu.", account.Username);
                 return (false, "Tài khoản không có email liên kết để gửi đặt lại mật khẩu.");
            }

            // 2. Tạo Token đặt lại mật khẩu (ví dụ: GUID hoặc chuỗi ngẫu nhiên)
            var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)); // Token ngẫu nhiên, dài
            account.PasswordResetToken = resetToken;
            account.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1); // Token hết hạn sau 1 giờ

            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã tạo Password Reset Token cho Account {AccountID}. Token hết hạn lúc {ExpiryTime}", account.AccountID, account.PasswordResetTokenExpiry);

            // 3. Gửi email chứa link đặt lại mật khẩu THẬT
            var resetLink = $"YOUR_FRONTEND_RESET_PASSWORD_URL?token={resetToken}"; // <-- ĐÂY LÀ LINK ĐẾN FRONTEND CỦA BẠN
            var emailSubject = "Yêu cầu đặt lại mật khẩu cho tài khoản DecalXeAPI của bạn";
            var emailBody = $@"
                <p>Xin chào {account.Username},</p>
                <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản của mình trên hệ thống DecalXeAPI.</p>
                <p>Vui lòng click vào đường link dưới đây để đặt lại mật khẩu của bạn:</p>
                <p><a href='{resetLink}'>Đặt lại mật khẩu của bạn</a></p>
                <p>Đường link này sẽ hết hạn trong 1 giờ.</p>
                <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                <p>Trân trọng,</p>
                <p>Hệ thống Decal Xe</p>";

            var emailSent = await _emailService.SendEmailAsync(account.Email, emailSubject, emailBody); // <-- GỌI EMAIL SERVICE THẬT

            if (!emailSent)
            {
                _logger.LogError("Không thể gửi email đặt lại mật khẩu đến {RecipientEmail}.", account.Email);
                return (false, "Không thể gửi email đặt lại mật khẩu. Vui lòng thử lại sau.");
            }

            return (true, null);
        }

        // Đặt lại mật khẩu (xác minh token và cập nhật mật khẩu)
        public async Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            _logger.LogInformation("Yêu cầu đặt lại mật khẩu bằng Token: {Token}", request.Token);

            // 1. Tìm tài khoản bằng Token và kiểm tra hết hạn
            var account = await _context.Accounts
                                        .FirstOrDefaultAsync(a => a.PasswordResetToken == request.Token && a.PasswordResetTokenExpiry > DateTime.UtcNow);

            if (account == null)
            {
                _logger.LogWarning("Token đặt lại mật khẩu không hợp lệ hoặc đã hết hạn: {Token}", request.Token);
                return (false, "Token đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
            }

            // 2. Kiểm tra xác nhận mật khẩu
            if (request.NewPassword != request.ConfirmNewPassword)
            {
                _logger.LogWarning("Mật khẩu mới và xác nhận mật khẩu không khớp cho Account {AccountID}", account.AccountID);
                return (false, "Mật khẩu mới và xác nhận mật khẩu không khớp.");
            }

            // 3. Hash mật khẩu mới (Nếu bạn đang hash mật khẩu khi tạo tài khoản, hãy hash ở đây)
            account.PasswordHash = request.NewPassword; // Tạm thời lưu plaintext, thực tế PHẢI HASH
            account.PasswordResetToken = null; // Hủy token sau khi sử dụng
            account.PasswordResetTokenExpiry = null; // Xóa thời gian hết hạn

            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Mật khẩu cho Account {AccountID} đã được đặt lại thành công.", account.AccountID);

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
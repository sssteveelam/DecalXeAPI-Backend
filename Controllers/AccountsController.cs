using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Vẫn cần DbContext cho các hàm Exists cơ bản
using DecalXeAPI.Data; // Vẫn cần ApplicationDbContext cho các hàm Exists
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // <-- THÊM DÒNG NÀY (Để sử dụng IAccountService)
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System; // Để sử dụng ArgumentException


namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
    public class AccountsController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists cơ bản
        private readonly IAccountService _accountService; // <-- KHAI BÁO BIẾN CHO SERVICE
        private readonly IMapper _mapper;
        private readonly ILogger<AccountsController> _logger;

        public AccountsController(ApplicationDbContext context, IAccountService accountService, IMapper mapper, ILogger<AccountsController> logger) // <-- TIÊM IAccountService
        {
            _context = context;
            _accountService = accountService;
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/Accounts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccounts()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách tài khoản.");
            var accounts = await _accountService.GetAccountsAsync();
            return Ok(accounts);
        }

        // API: GET api/Accounts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AccountDto>> GetAccount(string id)
        {
            _logger.LogInformation("Yêu cầu lấy tài khoản với ID: {AccountID}", id);
            var accountDto = await _accountService.GetAccountByIdAsync(id);

            if (accountDto == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản với ID: {AccountID}", id);
                return NotFound();
            }

            return Ok(accountDto);
        }

        // API: POST api/Accounts
        [HttpPost]
        public async Task<ActionResult<AccountDto>> PostAccount(Account account) // Vẫn nhận Account Model
        {
            _logger.LogInformation("Yêu cầu tạo tài khoản mới: {Username}", account.Username);

            // --- KIỂM TRA FKs CHÍNH TRƯỚC KHI GỬI VÀO SERVICE ---
            // Controller sẽ chịu trách nhiệm validate các FKs chính
            if (!string.IsNullOrEmpty(account.RoleID) && !RoleExists(account.RoleID))
            {
                return BadRequest("RoleID không tồn tại.");
            }

            try
            {
                var createdAccountDto = await _accountService.CreateAccountAsync(account);
                _logger.LogInformation("Đã tạo tài khoản mới với ID: {AccountID}", createdAccountDto.AccountID);
                return CreatedAtAction(nameof(GetAccount), new { id = createdAccountDto.AccountID }, createdAccountDto);
            }
            catch (ArgumentException ex) // Bắt lỗi từ Service nếu có (ví dụ: username đã tồn tại)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi tạo tài khoản: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/Accounts/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAccount(string id, UpdateAccountDto updateDto) // <-- Thay đổi quan trọng: Dùng UpdateAccountDto
        {
            _logger.LogInformation("Yêu cầu cập nhật tài khoản với ID: {AccountID}", id);

            // Giờ đây, chúng ta sẽ gọi đến service để kiểm tra, thay vì tự làm trong controller
            if (!await _accountService.RoleExistsAsync(updateDto.RoleID))
            {
                return BadRequest(new { message = "RoleID không tồn tại." });
            }

            try
            {
                // Truyền thẳng DTO vào service, service sẽ lo phần còn lại
                var (success, errorMessage) = await _accountService.UpdateAccountAsync(id, updateDto);

                if (!success)
                {
                    // Nếu không thành công, trả về lỗi mà service đã báo
                    // Dùng object error để frontend dễ xử lý hơn
                    return BadRequest(new { message = errorMessage });
                }

                // Nếu thành công, trả về 204 No Content, báo hiệu đã cập nhật xong
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Các xử lý lỗi khác vẫn giữ nguyên
                if (!await _accountService.AccountExistsAsync(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // Trong file: DecalXeAPI/Controllers/AccountsController.cs
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")] // Quyền cho RolesController
        public async Task<IActionResult> DeleteAccount(string id)
        {
            _logger.LogInformation("Yêu cầu xóa tài khoản với ID: {AccountID}", id);
            try
            {
                var success = await _accountService.DeleteAccountAsync(id);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy tài khoản để xóa với ID: {AccountID}", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex) // Bắt lỗi nghiệp vụ cụ thể từ Service
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi xóa tài khoản {AccountID}: {ErrorMessage}", id, ex.Message);
                // Trả về lỗi 400 Bad Request với thông báo từ Service
                return BadRequest(new { message = ex.Message });
            }
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        // Các hàm Exists này vẫn cần thiết nếu Controller thực hiện kiểm tra FKs trước khi gọi Service,
        // hoặc nếu Service trả về lỗi chung và Controller cần biết lỗi cụ thể để trả về NotFound.
        // Có thể bỏ đi nếu Service xử lý tất cả lỗi chi tiết và Controller chỉ cần trả về BadRequest/NotFound chung.
        private bool AccountExists(string id) { return _context.Accounts.Any(e => e.AccountID == id); }
        private bool RoleExists(string id) { return _context.Roles.Any(e => e.RoleID == id); }
    }
}
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
using Swashbuckle.AspNetCore.Filters; // <-- THÊM DÒNG NÀY
using DecalXeAPI.SwaggerExamples; 


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

        /// <summary>
        /// Lấy danh sách tất cả các tài khoản người dùng hiện có trong hệ thống.
        /// </summary>
        /// <returns>Danh sách các đối tượng AccountDto.</returns>
        /// <response code="200">Trả về danh sách tài khoản.</response>
        /// <response code="401">Không được ủy quyền (chưa đăng nhập hoặc token không hợp lệ).</response>
        /// <response code="403">Bị cấm (người dùng không có quyền truy cập).</response>
        // API: GET api/Accounts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccounts()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách tài khoản.");
            var accounts = await _accountService.GetAccountsAsync();
            return Ok(accounts);
        }



        /// <summary>
        /// Lấy thông tin chi tiết của một tài khoản cụ thể dựa trên AccountID.
        /// </summary>
        /// <param name="id">AccountID của tài khoản cần lấy.</param>
        /// <returns>Đối tượng AccountDto chứa thông tin chi tiết.</returns>
        /// <response code="200">Trả về thông tin tài khoản.</response>
        /// <response code="404">Không tìm thấy tài khoản với ID đã cho.</response>
        /// <response code="401">Không được ủy quyền.</response>
        /// <response code="403">Bị cấm.</response>
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

        /// <summary>
        /// Tạo một tài khoản người dùng mới.
        /// </summary>
        /// <remarks>
        /// Cần cung cấp đầy đủ thông tin để tạo tài khoản. Mật khẩu sẽ được lưu plaintext (thực tế cần hash).
        /// </remarks>
        /// <param name="account">Đối tượng Account chứa thông tin tài khoản cần tạo.</param>
        /// <returns>Đối tượng AccountDto của tài khoản vừa tạo.</returns>
        /// <response code="201">Tạo tài khoản thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ hoặc Username/RoleID đã tồn tại.</response>
        /// <response code="401">Không được ủy quyền.</response>
        /// <response code="403">Bị cấm.</response>
        // API: POST api/Accounts
        [HttpPost]
        [SwaggerRequestExample(typeof(Account), typeof(AccountRequestExample))] // <-- GIỮ NGUYÊN DÒNG NÀY CHO POST
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


        /// <summary>
        /// Cập nhật thông tin của một tài khoản hiện có dựa trên AccountID.
        /// </summary>
        /// <param name="id">AccountID của tài khoản cần cập nhật.</param>
        /// <param name="account">Đối tượng Account chứa thông tin cập nhật.</param>
        /// <returns>Không có nội dung nếu cập nhật thành công.</returns>
        /// <response code="204">Cập nhật thành công.</response>
        /// <response code="400">ID trong URL không khớp với AccountID trong body, dữ liệu không hợp lệ, hoặc RoleID/Username đã tồn tại.</response>
        /// <response code="404">Không tìm thấy tài khoản để cập nhật.</response>
        /// <response code="401">Không được ủy quyền.</response>
        /// <response code="403">Bị cấm.</response>
        [HttpPut("{id}")]
        [SwaggerRequestExample(typeof(Account), typeof(AccountUpdateRequestExample))] 
        public async Task<IActionResult> PutAccount(string id, Account account)
        {
            _logger.LogInformation("Yêu cầu cập nhật tài khoản với ID: {AccountID}", id);
            if (id != account.AccountID)
            {
                return BadRequest();
            }

            // Kiểm tra FKs chính
            if (!string.IsNullOrEmpty(account.RoleID) && !RoleExists(account.RoleID))
            {
                return BadRequest("RoleID không tồn tại.");
            }

            try
            {
                var success = await _accountService.UpdateAccountAsync(id, account);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy tài khoản để cập nhật với ID: {AccountID}", id);
                    return NotFound();
                }

                _logger.LogInformation("Đã cập nhật tài khoản với ID: {AccountID}", id);
                return NoContent();
            }
            catch (ArgumentException ex) // Bắt lỗi từ Service nếu có (ví dụ: username trùng lặp)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi cập nhật tài khoản: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (DbUpdateConcurrencyException) // Vẫn bắt riêng lỗi này ở Controller
            {
                if (!AccountExists(id)) // Vẫn dùng hàm hỗ trợ của Controller để kiểm tra tồn tại
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Xóa một tài khoản người dùng khỏi hệ thống dựa trên AccountID.
        /// </summary>
        /// <remarks>
        /// Chỉ Admin hoặc Manager có thể xóa tài khoản. Tài khoản không thể bị xóa nếu còn các liên kết (ví dụ: với Customer/Employee).
        /// </remarks>
        /// <param name="id">AccountID của tài khoản cần xóa.</param>
        /// <returns>Không có nội dung nếu xóa thành công.</returns>
        /// <response code="204">Xóa thành công.</response>
        /// <response code="400">Không thể xóa tài khoản vì đang được sử dụng.</response>
        /// <response code="404">Không tìm thấy tài khoản để xóa.</response>
        /// <response code="401">Không được ủy quyền.</response>
        /// <response code="403">Bị cấm.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(string id)
        {
            _logger.LogInformation("Yêu cầu xóa tài khoản với ID: {AccountID}", id);
            var success = await _accountService.DeleteAccountAsync(id);

            if (!success)
            {
                _logger.LogWarning("Không tìm thấy tài khoản để xóa với ID: {AccountID}", id);
                return NotFound();
            }

            return NoContent();
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        // Các hàm Exists này vẫn cần thiết nếu Controller thực hiện kiểm tra FKs trước khi gọi Service,
        // hoặc nếu Service trả về lỗi chung và Controller cần biết lỗi cụ thể để trả về NotFound.
        // Có thể bỏ đi nếu Service xử lý tất cả lỗi chi tiết và Controller chỉ cần trả về BadRequest/NotFound chung.
        private bool AccountExists(string id) { return _context.Accounts.Any(e => e.AccountID == id); }
        private bool RoleExists(string id) { return _context.Roles.Any(e => e.RoleID == id); }
    }
}
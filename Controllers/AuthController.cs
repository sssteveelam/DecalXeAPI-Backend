using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; // Cần để đọc cấu hình JWT
using DecalXeAPI.Services.Interfaces; // <-- THÊM DÒNG NÀY

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration; // Để đọc cấu hình từ appsettings.json
        private readonly IAccountService _accountService; // <-- KHAI BÁO BIẾN ACCOUNT SERVICE

        public AuthController(ApplicationDbContext context, IConfiguration configuration, IAccountService accountService) // <-- TIÊM ACCOUNT SERVICE VÀO CONSTRUCTOR
        {
            _context = context;
            _configuration = configuration;
            _accountService = accountService; // Gán Account Service
        }

        // API: POST api/Auth/register
        // Đăng ký tài khoản mới
        [HttpPost("register")]
        public async Task<ActionResult<string>> Register([FromBody] RegisterDto registerDto)
        {
            // Logic đăng ký sẽ được xử lý bởi AccountService
            var newAccount = new Account
            {
                Username = registerDto.Username,
                PasswordHash = registerDto.Password, // Thực tế cần hash mật khẩu ở đây
                RoleID = registerDto.RoleID,
                IsActive = true
            };

            try
            {
                await _accountService.CreateAccountAsync(newAccount);
                return Ok("Đăng ký thành công.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); // Bắt lỗi Username đã tồn tại, RoleID không tồn tại từ Service
            }
        }

        // API: POST api/Auth/login
        // Đăng nhập và trả về JWT Token
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] LoginDto loginDto)
        {
            var account = await _context.Accounts
                                        .Include(a => a.Role)
                                        .FirstOrDefaultAsync(a => a.Username == loginDto.Username);

            if (account == null)
            {
                return Unauthorized("Sai Username hoặc mật khẩu.");
            }

            // Thực tế: so sánh mật khẩu đã hash
            if (account.PasswordHash != loginDto.Password) // Tạm thời so sánh chuỗi trực tiếp
            {
                return Unauthorized("Sai Username hoặc mật khẩu.");
            }

            if (!account.IsActive)
            {
                return Unauthorized("Tài khoản của bạn đã bị khóa.");
            }

            var token = GenerateJwtToken(account);

            return Ok(token);
        }

        // --- API MỚI CHO TÍNH NĂNG QUÊN MẬT KHẨU ---

        // API: POST api/Auth/forgot-password
        // Yêu cầu đặt lại mật khẩu (gửi email chứa token)
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            // Ủy quyền logic cho AccountService
            // Service sẽ luôn trả về thành công để tránh tiết lộ thông tin tài khoản
            var (success, errorMessage) = await _accountService.ForgotPasswordAsync(request);

            if (!success && errorMessage != null)
            {
                return BadRequest(errorMessage); // Xảy ra nếu có lỗi nghiệp vụ khác
            }

            // Luôn trả về 200 OK để không tiết lộ liệu email/username có tồn tại hay không
            return Ok("Nếu tài khoản tồn tại, một email đặt lại mật khẩu đã được gửi.");
        }

        // API: POST api/Auth/reset-password
        // Đặt lại mật khẩu bằng token
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            // Ủy quyền logic cho AccountService
            var (success, errorMessage) = await _accountService.ResetPasswordAsync(request);

            if (!success)
            {
                return BadRequest(errorMessage); // Trả về lỗi nếu token không hợp lệ/hết hạn hoặc mật khẩu không khớp
            }

            return Ok("Mật khẩu đã được đặt lại thành công.");
        }

        // Hàm hỗ trợ: Tạo JWT Token (Không thay đổi)
        private string GenerateJwtToken(Account account)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key không được cấu hình."));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.AccountID),
                new Claim(ClaimTypes.Name, account.Username),
                new Claim(ClaimTypes.Role, account.Role?.RoleName ?? "Unknown")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Hàm hỗ trợ: Kiểm tra Role có tồn tại không (copy từ RolesController, có thể chuyển vào RoleService sau)
        private bool RoleExists(string id)
        {
            return _context.Roles.Any(e => e.RoleID == id);
        }
    }
}
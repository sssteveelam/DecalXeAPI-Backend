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
using Microsoft.Extensions.Configuration;
using DecalXeAPI.Services.Interfaces;
using AutoMapper; // Cần cho việc ánh xạ RegisterDto sang Account Model

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper; // <-- THÊM IMAPPER VÀO ĐÂY

        public AuthController(ApplicationDbContext context, IConfiguration configuration, IAccountService accountService, IMapper mapper) // <-- TIÊM IMAPPER
        {
            _context = context;
            _configuration = configuration;
            _accountService = accountService;
            _mapper = mapper; // Gán IMapper
        }

        // API: POST api/Auth/register
        [HttpPost("register")]
        public async Task<ActionResult<string>> Register([FromBody] RegisterDto registerDto) // Nhận vào RegisterDto
        {
            // Ánh xạ RegisterDto sang Account Model. Email sẽ được ánh xạ tự động.
            var newAccount = _mapper.Map<Account>(registerDto);
            // PasswordHash đã được ánh xạ từ Password trong MappingProfile.

            try
            {
                await _accountService.CreateAccountAsync(newAccount);
                return Ok("Đăng ký thành công.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // API: POST api/Auth/login
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

            if (account.PasswordHash != loginDto.Password)
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

        // API: POST api/Auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            var (success, errorMessage) = await _accountService.ForgotPasswordAsync(request);

            if (!success && errorMessage != null)
            {
                return BadRequest(errorMessage);
            }

            return Ok("Nếu tài khoản tồn tại, một email đặt lại mật khẩu đã được gửi.");
        }

        // API: POST api/Auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            var (success, errorMessage) = await _accountService.ResetPasswordAsync(request);

            if (!success)
            {
                return BadRequest(errorMessage);
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

        // Hàm hỗ trợ: Kiểm tra Role có tồn tại không (vẫn giữ ở đây hoặc chuyển vào RoleService)
        private bool RoleExists(string id)
        {
            return _context.Roles.Any(e => e.RoleID == id);
        }
    }
}
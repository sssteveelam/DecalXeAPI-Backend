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

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration; // Để đọc cấu hình từ appsettings.json

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // API: POST api/Auth/register
        // Đăng ký tài khoản mới
        [HttpPost("register")]
        public async Task<ActionResult<string>> Register([FromBody] RegisterDto registerDto)
        {
            // 1. Kiểm tra username đã tồn tại chưa
            if (await _context.Accounts.AnyAsync(a => a.Username == registerDto.Username))
            {
                return BadRequest("Username đã tồn tại.");
            }

            // 2. Kiểm tra RoleID có tồn tại không
            var role = await _context.Roles.FindAsync(registerDto.RoleID);
            if (role == null)
            {
                return BadRequest("RoleID không tồn tại.");
            }

            // 3. Hash mật khẩu (Sử dụng BCrypt hoặc PBKDF2 trong thực tế, ở đây dùng tạm SHA256 cho đơn giản)
            // Hướng dẫn: string hashedPassword = HashPassword(registerDto.Password);
            // Để đơn giản, tạm thời mình sẽ không hash mật khẩu ở đây để dễ test,
            // nhưng trong môi trường sản xuất BẮT BUỘC phải hash mật khẩu an toàn.
            string hashedPassword = registerDto.Password; // Tạm thời KHÔNG HASH MẬT KHẨU để dễ test

            // 4. Tạo Account mới
            var newAccount = new Account
            {
                AccountID = Guid.NewGuid().ToString(),
                Username = registerDto.Username,
                PasswordHash = hashedPassword,
                RoleID = registerDto.RoleID,
                IsActive = true
            };

            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();

            return Ok("Đăng ký thành công.");
        }

        // API: POST api/Auth/login
        // Đăng nhập và trả về JWT Token
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] LoginDto loginDto)
        {
            // 1. Tìm Account theo Username
            var account = await _context.Accounts
                                        .Include(a => a.Role) // Bao gồm Role để lấy RoleName
                                        .FirstOrDefaultAsync(a => a.Username == loginDto.Username);

            if (account == null)
            {
                return Unauthorized("Sai Username hoặc mật khẩu.");
            }

            // 2. Xác minh mật khẩu (So sánh mật khẩu đã hash trong thực tế)
            // Hướng dẫn: if (!VerifyPassword(loginDto.Password, account.PasswordHash))
            if (account.PasswordHash != loginDto.Password) // Tạm thời so sánh chuỗi trực tiếp vì không hash
            {
                return Unauthorized("Sai Username hoặc mật khẩu.");
            }

            // 3. Kiểm tra Account có Active không
            if (!account.IsActive)
            {
                return Unauthorized("Tài khoản của bạn đã bị khóa.");
            }

            // 4. Tạo JWT Token
            var token = GenerateJwtToken(account);

            return Ok(token); // Trả về Token
        }

        // Hàm hỗ trợ: Tạo JWT Token
        private string GenerateJwtToken(Account account)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key không được cấu hình."));
            // Định nghĩa các Claims (thông tin về người dùng trong token)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.AccountID), // ID của người dùng
                new Claim(ClaimTypes.Name, account.Username), // Username
                new Claim(ClaimTypes.Role, account.Role?.RoleName ?? "Unknown") // Role của người dùng
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1), // Token hết hạn sau 1 giờ
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token); // Trả về chuỗi token
        }

        // Hàm hỗ trợ: Hash mật khẩu (Cần thư viện BCrypt.Net.Core hoặc Microsoft.AspNetCore.Cryptography.KeyDerivation)
        // Trong môi trường sản xuất, bạn PHẢI sử dụng hàm hash mật khẩu an toàn
        /*
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password); // Ví dụ dùng BCrypt.Net
        }
        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword); // Ví dụ dùng BCrypt.Net
        }
        */

        // Hàm hỗ trợ: Kiểm tra Role có tồn tại không (copy từ RolesController)
        private bool RoleExists(string id)
        {
            return _context.Roles.Any(e => e.RoleID == id);
        }
    }
}
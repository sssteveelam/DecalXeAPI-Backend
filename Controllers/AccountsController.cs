using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs; // Để sử dụng AccountDto
using AutoMapper; // <-- THÊM DÒNG NÀY ĐỂ SỬ DỤNG AUTOMAPPER

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Khai báo biến IMapper để sử dụng AutoMapper

        // Constructor: Hàm khởi tạo của Controller, ASP.NET Core sẽ tự động "tiêm" (inject) DbContext và IMapper vào đây.
        public AccountsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper; // Gán giá trị IMapper được tiêm vào
        }

        // API: GET api/Accounts
        // Lấy tất cả các Account, trả về dưới dạng AccountDto
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccounts()
        {
            var accounts = await _context.Accounts.Include(a => a.Role).ToListAsync();

            // Ánh xạ TỰ ĐỘNG từ List<Account> sang List<AccountDto> bằng AutoMapper
            var accountDtos = _mapper.Map<List<AccountDto>>(accounts);

            return Ok(accountDtos);
        }

        // API: GET api/Accounts/{id}
        // Lấy thông tin một Account theo AccountID, trả về dưới dạng AccountDto
        [HttpGet("{id}")]
        public async Task<ActionResult<AccountDto>> GetAccount(string id)
        {
            var account = await _context.Accounts.Include(a => a.Role).FirstOrDefaultAsync(a => a.AccountID == id);

            if (account == null)
            {
                return NotFound();
            }

            // Ánh xạ TỰ ĐỘNG từ Account Model sang AccountDto bằng AutoMapper
            var accountDto = _mapper.Map<AccountDto>(account);

            return Ok(accountDto);
        }

        // API: POST api/Accounts
        // Tạo một Account mới, nhận vào Account Model, trả về AccountDto sau khi tạo
        [HttpPost]
        public async Task<ActionResult<AccountDto>> PostAccount(Account account)
        {
            // Kiểm tra xem RoleID có tồn tại không
            if (!string.IsNullOrEmpty(account.RoleID) && !RoleExists(account.RoleID))
            {
                return BadRequest("RoleID không tồn tại.");
            }

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Tải lại thông tin Role để có RoleName cho DTO
            // AutoMapper sẽ cần thông tin Role đã được tải để có thể ánh xạ RoleName
            await _context.Entry(account).Reference(a => a.Role).LoadAsync();

            // Ánh xạ TỰ ĐỘNG Account Model vừa tạo sang AccountDto để trả về
            var accountDto = _mapper.Map<AccountDto>(account);

            // Trả về 201 Created và thông tin của AccountDto vừa tạo
            return CreatedAtAction(nameof(GetAccount), new { id = accountDto.AccountID }, accountDto);
        }

        // API: PUT api/Accounts/{id}
        // Cập nhật thông tin một Account hiện có, vẫn nhận vào Account Model
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAccount(string id, Account account)
        {
            if (id != account.AccountID)
            {
                return BadRequest();
            }

            if (!string.IsNullOrEmpty(account.RoleID) && !RoleExists(account.RoleID))
            {
                return BadRequest("RoleID không tồn tại.");
            }

            _context.Entry(account).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccountExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // API: DELETE api/Accounts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(string id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AccountExists(string id)
        {
            return _context.Accounts.Any(e => e.AccountID == id);
        }

        private bool RoleExists(string id)
        {
            return _context.Roles.Any(e => e.RoleID == id);
        }
    }
}
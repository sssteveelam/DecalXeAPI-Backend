using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Để dùng .Include() để lấy dữ liệu liên quan
using DecalXeAPI.Data;
using DecalXeAPI.Models;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: GET api/Accounts
        // Lấy tất cả các Account, bao gồm thông tin Role liên quan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Account>>> GetAccounts()
        {
            // .Include(a => a.Role) là một phương thức của EF Core
            // giúp "tải" (load) dữ liệu của bảng Role liên quan đến mỗi Account.
            // Nếu không có .Include(), chỉ có thông tin Account được trả về, Role sẽ là null.
            return await _context.Accounts.Include(a => a.Role).ToListAsync();
        }

        // API: GET api/Accounts/{id}
        // Lấy thông tin một Account theo AccountID, bao gồm Role liên quan
        [HttpGet("{id}")]
        public async Task<ActionResult<Account>> GetAccount(string id)
        {
            // .Include(a => a.Role) ở đây cũng cần thiết để lấy thông tin Role
            var account = await _context.Accounts.Include(a => a.Role).FirstOrDefaultAsync(a => a.AccountID == id);

            if (account == null)
            {
                return NotFound();
            }

            return account;
        }

        // API: POST api/Accounts
        // Tạo một Account mới
        [HttpPost]
        public async Task<ActionResult<Account>> PostAccount(Account account)
        {
            // Trước khi thêm Account, kiểm tra xem RoleID có tồn tại không
            if (!string.IsNullOrEmpty(account.RoleID) && !RoleExists(account.RoleID))
            {
                return BadRequest("RoleID không tồn tại."); // Trả về lỗi nếu RoleID không hợp lệ
            }

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Để trả về thông tin Role vừa tạo, cần tải lại Account đó với Include
            // hoặc lấy Role từ context nếu biết chắc chắn RoleID hợp lệ
            await _context.Entry(account).Reference(a => a.Role).LoadAsync(); // Tải Role sau khi lưu Account

            return CreatedAtAction(nameof(GetAccount), new { id = account.AccountID }, account);
        }

        // API: PUT api/Accounts/{id}
        // Cập nhật thông tin một Account hiện có
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAccount(string id, Account account)
        {
            if (id != account.AccountID)
            {
                return BadRequest();
            }

            // Kiểm tra xem RoleID có tồn tại không trước khi cập nhật
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
        // Xóa một Account
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

        // Hàm hỗ trợ: Kiểm tra xem Account có tồn tại không
        private bool AccountExists(string id)
        {
            return _context.Accounts.Any(e => e.AccountID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem Role có tồn tại không (copy từ RolesController)
        private bool RoleExists(string id)
        {
            return _context.Roles.Any(e => e.RoleID == id);
        }
    }
}
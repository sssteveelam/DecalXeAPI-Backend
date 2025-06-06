using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Để dùng .Include()
using DecalXeAPI.Data;
using DecalXeAPI.Models;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: GET api/Customers
        // Lấy tất cả các Customer, bao gồm thông tin Account liên quan (nếu có)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            // .Include(c => c.Account) sẽ tải thông tin Account liên kết với mỗi Customer.
            return await _context.Customers.Include(c => c.Account).ToListAsync();
        }

        // API: GET api/Customers/{id}
        // Lấy thông tin một Customer theo CustomerID, bao gồm Account liên quan
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(string id)
        {
            // .FirstOrDefaultAsync() sẽ lấy Customer đầu tiên khớp với điều kiện,
            // hoặc null nếu không tìm thấy.
            var customer = await _context.Customers.Include(c => c.Account).FirstOrDefaultAsync(c => c.CustomerID == id);

            if (customer == null)
            {
                return NotFound(); // Trả về 404 Not Found
            }

            return customer;
        }

        // API: POST api/Customers
        // Tạo một Customer mới
        // Dữ liệu Customer được gửi trong body của request.
        [HttpPost]
        public async Task<ActionResult<Customer>> PostCustomer(Customer customer)
        {
            // Kiểm tra xem AccountID có tồn tại không nếu được cung cấp
            if (!string.IsNullOrEmpty(customer.AccountID) && !AccountExists(customer.AccountID))
            {
                return BadRequest("AccountID không tồn tại."); // Trả về lỗi 400 Bad Request
            }

            _context.Customers.Add(customer); // Thêm Customer vào DbContext
            await _context.SaveChangesAsync(); // Lưu thay đổi vào database

            // Sau khi lưu, tải lại thông tin Account để trả về đầy đủ cho client
            await _context.Entry(customer).Reference(c => c.Account).LoadAsync();

            // Trả về 201 Created và thông tin của Customer vừa tạo
            return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerID }, customer);
        }

        // API: PUT api/Customers/{id}
        // Cập nhật thông tin một Customer hiện có
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(string id, Customer customer)
        {
            if (id != customer.CustomerID)
            {
                return BadRequest();
            }

            // Kiểm tra xem AccountID có tồn tại không nếu được cung cấp
            if (!string.IsNullOrEmpty(customer.AccountID) && !AccountExists(customer.AccountID))
            {
                return BadRequest("AccountID không tồn tại.");
            }

            _context.Entry(customer).State = EntityState.Modified; // Đánh dấu Entity là đã được thay đổi

            try
            {
                await _context.SaveChangesAsync(); // Lưu thay đổi
            }
            catch (DbUpdateConcurrencyException) // Xử lý lỗi khi có xung đột dữ liệu
            {
                if (!CustomerExists(id)) // Kiểm tra xem Customer có tồn tại không
                {
                    return NotFound();
                }
                else
                {
                    throw; // Ném lại lỗi nếu là lỗi khác
                }
            }

            return NoContent(); // Trả về 204 No Content (cập nhật thành công, không có nội dung trả về)
        }

        // API: DELETE api/Customers/{id}
        // Xóa một Customer
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            var customer = await _context.Customers.FindAsync(id); // Tìm Customer theo ID
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer); // Xóa Customer khỏi DbContext
            await _context.SaveChangesAsync(); // Lưu thay đổi

            return NoContent(); // Trả về 204 No Content
        }

        // Hàm hỗ trợ: Kiểm tra xem Customer có tồn tại không
        private bool CustomerExists(string id)
        {
            return _context.Customers.Any(e => e.CustomerID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem Account có tồn tại không (copy từ AccountsController)
        private bool AccountExists(string id)
        {
            return _context.Accounts.Any(e => e.AccountID == id);
        }
    }
}
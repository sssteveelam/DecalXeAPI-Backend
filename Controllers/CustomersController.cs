using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Để dùng .Include()
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs; // Để sử dụng CustomerDto
using AutoMapper; // Để sử dụng AutoMapper
using System.Collections.Generic; // Để sử dụng IEnumerable

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Khai báo biến IMapper

        public CustomersController(ApplicationDbContext context, IMapper mapper) // Tiêm IMapper
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/Customers
        // Lấy tất cả các Customer, trả về dưới dạng CustomerDto
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers() // Kiểu trả về là CustomerDto
        {
            var customers = await _context.Customers
                                        .Include(c => c.Account)
                                            .ThenInclude(a => a.Role) // Tải thông tin Role của Account
                                        .ToListAsync();

            // Sử dụng AutoMapper để ánh xạ từ List<Customer> sang List<CustomerDto>
            var customerDtos = _mapper.Map<List<CustomerDto>>(customers);

            return Ok(customerDtos);
        }

        // API: GET api/Customers/{id}
        // Lấy thông tin một Customer theo CustomerID, trả về dưới dạng CustomerDto
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomer(string id) // Kiểu trả về là CustomerDto
        {
            var customer = await _context.Customers
                                        .Include(c => c.Account)
                                            .ThenInclude(a => a.Role)
                                        .FirstOrDefaultAsync(c => c.CustomerID == id);

            if (customer == null)
            {
                return NotFound();
            }

            // Sử dụng AutoMapper để ánh xạ từ Customer Model sang CustomerDto
            var customerDto = _mapper.Map<CustomerDto>(customer);

            return Ok(customerDto);
        }

        // API: POST api/Customers (Vẫn nhận vào Customer Model, trả về CustomerDto)
        [HttpPost]
        public async Task<ActionResult<CustomerDto>> PostCustomer(Customer customer) // Kiểu trả về là CustomerDto
        {
            if (!string.IsNullOrEmpty(customer.AccountID) && !AccountExists(customer.AccountID))
            {
                return BadRequest("AccountID không tồn tại.");
            }

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Tải lại thông tin liên quan để AutoMapper có thể ánh xạ đầy đủ
            await _context.Entry(customer).Reference(c => c.Account).LoadAsync();
            if (customer.Account != null)
            {
                await _context.Entry(customer.Account).Reference(a => a.Role).LoadAsync();
            }

            // Ánh xạ Customer Model vừa tạo sang CustomerDto để trả về
            var customerDto = _mapper.Map<CustomerDto>(customer);

            return CreatedAtAction(nameof(GetCustomer), new { id = customerDto.CustomerID }, customerDto);
        }

        // PUT và DELETE không thay đổi kiểu trả về là IActionResult
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(string id, Customer customer)
        {
            if (id != customer.CustomerID)
            {
                return BadRequest();
            }

            if (!string.IsNullOrEmpty(customer.AccountID) && !AccountExists(customer.AccountID))
            {
                return BadRequest("AccountID không tồn tại.");
            }

            _context.Entry(customer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CustomerExists(string id)
        {
            return _context.Customers.Any(e => e.CustomerID == id);
        }

        private bool AccountExists(string id)
        {
            return _context.Accounts.Any(e => e.AccountID == id);
        }
    }
}
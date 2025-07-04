using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Vẫn cần DbContext cho các hàm Exists cơ bản
using DecalXeAPI.Data; // Vẫn cần ApplicationDbContext cho các hàm Exists
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // <-- THÊM DÒNG NÀY (Để sử dụng ICustomerService)
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System; // Để sử dụng ArgumentException

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists cơ bản
        private readonly ICustomerService _customerService; // <-- KHAI BÁO BIẾN CHO SERVICE
        private readonly IMapper _mapper;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ApplicationDbContext context, ICustomerService customerService, IMapper mapper, ILogger<CustomersController> logger) // <-- TIÊM ICustomerService
        {
            _context = context;
            _customerService = customerService;
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/Customers
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Sales,Technician,Customer")] // Nới lỏng quyền cho GET
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách khách hàng.");
            var customers = await _customerService.GetCustomersAsync();
            return Ok(customers);
        }

        // API: GET api/Customers/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Sales,Technician,Customer")] // Nới lỏng quyền cho GET by ID
        public async Task<ActionResult<CustomerDto>> GetCustomer(string id)
        {
            _logger.LogInformation("Yêu cầu lấy khách hàng với ID: {CustomerID}", id);
            var customerDto = await _customerService.GetCustomerByIdAsync(id);

            if (customerDto == null)
            {
                _logger.LogWarning("Không tìm thấy khách hàng với ID: {CustomerID}", id);
                return NotFound();
            }

            return Ok(customerDto);
        }
// API: POST api/Customers (ĐÃ NÂNG CẤP)
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Customer,Sales")]
        public async Task<ActionResult<CustomerDto>> PostCustomer(CreateCustomerDto createDto)
        {
            _logger.LogInformation("Yêu cầu tạo khách hàng mới: {FirstName} {LastName}", createDto.FirstName, createDto.LastName);
            
            var customer = _mapper.Map<Customer>(createDto);

            try
            {
                var createdCustomerDto = await _customerService.CreateCustomerAsync(customer);
                return CreatedAtAction(nameof(GetCustomer), new { id = createdCustomerDto.CustomerID }, createdCustomerDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/Customers/{id} (ĐÃ NÂNG CẤP)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Sales")]
        public async Task<IActionResult> PutCustomer(string id, UpdateCustomerDto updateDto)
        {
            _logger.LogInformation("Yêu cầu cập nhật khách hàng với ID: {CustomerID}", id);

            var existingCustomer = await _context.Customers.FindAsync(id);
            if (existingCustomer == null)
            {
                return NotFound();
            }

            // Dùng AutoMapper để cập nhật các trường từ DTO vào Model đã tồn tại
            _mapper.Map(updateDto, existingCustomer);

            try
            {
                var success = await _customerService.UpdateCustomerAsync(id, existingCustomer);
                if (!success) return NotFound();
                
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // API: DELETE api/Customers/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")] // Chỉ Admin, Manager có thể xóa
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            _logger.LogInformation("Yêu cầu xóa khách hàng với ID: {CustomerID}", id);
            var success = await _customerService.DeleteCustomerAsync(id);

            if (!success)
            {
                _logger.LogWarning("Không tìm thấy khách hàng để xóa với ID: {CustomerID}", id);
                return NotFound();
            }

            return NoContent();
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool CustomerExists(string id) { return _context.Customers.Any(e => e.CustomerID == id); }
        private bool AccountExists(string id) { return _context.Accounts.Any(e => e.AccountID == id); }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs; // Để sử dụng EmployeeDto
using AutoMapper; // Để sử dụng AutoMapper
using System.Collections.Generic; // Để sử dụng IEnumerable
using Microsoft.AspNetCore.Authorization;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Khai báo biến IMapper

        public EmployeesController(ApplicationDbContext context, IMapper mapper) // Tiêm IMapper
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/Employees
        // Lấy tất cả các Employee, trả về dưới dạng EmployeeDto
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees() // Kiểu trả về là EmployeeDto
        {
            var employees = await _context.Employees
                                            .Include(e => e.Account)
                                                .ThenInclude(a => a.Role) // Tải thông tin Role của Account
                                            .Include(e => e.Store)
                                            .ToListAsync();

            // Sử dụng AutoMapper để ánh xạ từ List<Employee> sang List<EmployeeDto>
            var employeeDtos = _mapper.Map<List<EmployeeDto>>(employees);

            return Ok(employeeDtos);
        }

        // API: GET api/Employees/{id}
        // Lấy thông tin một Employee theo EmployeeID, trả về dưới dạng EmployeeDto
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> GetEmployee(string id) // Kiểu trả về là EmployeeDto
        {
            var employee = await _context.Employees
                                            .Include(e => e.Account)
                                                .ThenInclude(a => a.Role)
                                            .Include(e => e.Store)
                                            .FirstOrDefaultAsync(e => e.EmployeeID == id);

            if (employee == null)
            {
                return NotFound();
            }

            // Sử dụng AutoMapper để ánh xạ từ Employee Model sang EmployeeDto
            var employeeDto = _mapper.Map<EmployeeDto>(employee);

            return Ok(employeeDto);
        }

        // API: POST api/Employees (Vẫn nhận vào Employee Model, trả về EmployeeDto)
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<EmployeeDto>> PostEmployee(Employee employee) // Kiểu trả về là EmployeeDto
        {
            if (!string.IsNullOrEmpty(employee.StoreID) && !StoreExists(employee.StoreID))
            {
                return BadRequest("StoreID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(employee.AccountID) && !AccountExists(employee.AccountID))
            {
                return BadRequest("AccountID không tồn tại.");
            }

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Tải lại thông tin liên quan để AutoMapper có thể ánh xạ đầy đủ
            await _context.Entry(employee).Reference(e => e.Account).LoadAsync();
            if (employee.Account != null)
            {
                await _context.Entry(employee.Account).Reference(a => a.Role).LoadAsync();
            }
            await _context.Entry(employee).Reference(e => e.Store).LoadAsync();

            // Ánh xạ Employee Model vừa tạo sang EmployeeDto để trả về
            var employeeDto = _mapper.Map<EmployeeDto>(employee);

            return CreatedAtAction(nameof(GetEmployee), new { id = employeeDto.EmployeeID }, employeeDto);
        }

        // PUT và DELETE không thay đổi kiểu trả về là IActionResult
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> PutEmployee(string id, Employee employee)
        {
            if (id != employee.EmployeeID)
            {
                return BadRequest();
            }

            if (!string.IsNullOrEmpty(employee.StoreID) && !StoreExists(employee.StoreID))
            {
                return BadRequest("StoreID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(employee.AccountID) && !AccountExists(employee.AccountID))
            {
                return BadRequest("AccountID không tồn tại.");
            }

            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
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
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteEmployee(string id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmployeeExists(string id)
        {
            return _context.Employees.Any(e => e.EmployeeID == id);
        }

        private bool StoreExists(string id)
        {
            return _context.Stores.Any(e => e.StoreID == id);
        }

        private bool AccountExists(string id)
        {
            return _context.Accounts.Any(e => e.AccountID == id);
        }
    }
}
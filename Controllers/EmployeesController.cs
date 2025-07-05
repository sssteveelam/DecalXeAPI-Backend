using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Vẫn cần DbContext cho các hàm Exists cơ bản
using DecalXeAPI.Data; // Vẫn cần ApplicationDbContext cho các hàm Exists
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // <-- THÊM DÒNG NÀY (Để sử dụng IEmployeeService)
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
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists cơ bản
        private readonly IEmployeeService _employeeService; // <-- KHAI BÁO BIẾN CHO SERVICE
        private readonly IMapper _mapper;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(ApplicationDbContext context, IEmployeeService employeeService, IMapper mapper, ILogger<EmployeesController> logger) // <-- TIÊM IEmployeeService
        {
            _context = context;
            _employeeService = employeeService;
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/Employees
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Sales,Technician,Customer")] // Nới lỏng quyền cho GET
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách nhân viên.");
            var employees = await _employeeService.GetEmployeesAsync();
            return Ok(employees);
        }

        // API: GET api/Employees/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Sales,Technician,Customer")] // Nới lỏng quyền cho GET by ID
        public async Task<ActionResult<EmployeeDto>> GetEmployee(string id)
        {
            _logger.LogInformation("Yêu cầu lấy nhân viên với ID: {EmployeeID}", id);
            var employeeDto = await _employeeService.GetEmployeeByIdAsync(id);

            if (employeeDto == null)
            {
                _logger.LogWarning("Không tìm thấy nhân viên với ID: {EmployeeID}", id);
                return NotFound();
            }

            return Ok(employeeDto);
        }

        // API: POST api/Employees (ĐÃ NÂNG CẤP)
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<EmployeeDto>> PostEmployee(CreateEmployeeDto createDto)
        {
            _logger.LogInformation("Yêu cầu tạo nhân viên mới: {FirstName} {LastName}", createDto.FirstName, createDto.LastName);

            if (!string.IsNullOrEmpty(createDto.StoreID) && !StoreExists(createDto.StoreID))
            {
                return BadRequest("StoreID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(createDto.AccountID) && !AccountExists(createDto.AccountID))
            {
                return BadRequest("AccountID không tồn tại.");
            }

            var employee = _mapper.Map<Employee>(createDto);

            try
            {
                var createdEmployeeDto = await _employeeService.CreateEmployeeAsync(employee);
                return CreatedAtAction(nameof(GetEmployee), new { id = createdEmployeeDto.EmployeeID }, createdEmployeeDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/Employees/{id} (ĐÃ NÂNG CẤP)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> PutEmployee(string id, UpdateEmployeeDto updateDto)
        {
            _logger.LogInformation("Yêu cầu cập nhật nhân viên với ID: {EmployeeID}", id);
            
            if (!StoreExists(updateDto.StoreID))
            {
                return BadRequest("StoreID không tồn tại.");
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            _mapper.Map(updateDto, employee);

            try
            {
                var success = await _employeeService.UpdateEmployeeAsync(id, employee);
                if (!success) return NotFound();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // API: DELETE api/Employees/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteEmployee(string id)
        {
            _logger.LogInformation("Yêu cầu xóa nhân viên với ID: {EmployeeID}", id);
            var success = await _employeeService.DeleteEmployeeAsync(id);

            if (!success)
            {
                _logger.LogWarning("Không tìm thấy nhân viên để xóa với ID: {EmployeeID}", id);
                return NotFound();
            }

            return NoContent();
        }


        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool EmployeeExists(string id) { return _context.Employees.Any(e => e.EmployeeID == id); }
        private bool StoreExists(string id) { return _context.Stores.Any(e => e.StoreID == id); }
        private bool AccountExists(string id) { return _context.Accounts.Any(e => e.AccountID == id); }
    }
}
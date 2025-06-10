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

        // API: POST api/Employees
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<EmployeeDto>> PostEmployee(Employee employee) // Vẫn nhận Employee Model
        {
            _logger.LogInformation("Yêu cầu tạo nhân viên mới: {FirstName} {LastName}", employee.FirstName, employee.LastName);

            // --- KIỂM TRA FKs CHÍNH TRƯỚC KHI GỬI VÀO SERVICE ---
            if (!string.IsNullOrEmpty(employee.StoreID) && !StoreExists(employee.StoreID))
            {
                return BadRequest("StoreID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(employee.AccountID) && !AccountExists(employee.AccountID))
            {
                return BadRequest("AccountID không tồn tại.");
            }

            try
            {
                var createdEmployeeDto = await _employeeService.CreateEmployeeAsync(employee);
                _logger.LogInformation("Đã tạo nhân viên mới với ID: {EmployeeID}", createdEmployeeDto.EmployeeID);
                return CreatedAtAction(nameof(GetEmployee), new { id = createdEmployeeDto.EmployeeID }, createdEmployeeDto);
            }
            catch (ArgumentException ex) // Bắt lỗi từ Service nếu có (ví dụ: username trùng lặp)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi tạo nhân viên: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/Employees/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> PutEmployee(string id, Employee employee)
        {
            _logger.LogInformation("Yêu cầu cập nhật nhân viên với ID: {EmployeeID}", id);
            if (id != employee.EmployeeID)
            {
                return BadRequest();
            }

            // Kiểm tra FKs chính
            if (!string.IsNullOrEmpty(employee.StoreID) && !StoreExists(employee.StoreID))
            {
                return BadRequest("StoreID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(employee.AccountID) && !AccountExists(employee.AccountID))
            {
                return BadRequest("AccountID không tồn tại.");
            }

            try
            {
                var success = await _employeeService.UpdateEmployeeAsync(id, employee);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy nhân viên để cập nhật với ID: {EmployeeID}", id);
                    return NotFound();
                }

                _logger.LogInformation("Đã cập nhật nhân viên với ID: {EmployeeID}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi cập nhật nhân viên: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
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

        // API Thống kê Hiệu suất Nhân viên
        [HttpGet("statistics/performance")]
        [Authorize(Roles = "Admin,Manager")] // Chỉ Admin, Manager được xem thống kê
        public async Task<ActionResult<IEnumerable<EmployeePerformanceDto>>> GetEmployeePerformanceStatistics()
        {
            _logger.LogInformation("Yêu cầu thống kê hiệu suất nhân viên.");
            var performanceData = await _employeeService.GetEmployeePerformanceStatisticsAsync();
            return Ok(performanceData);
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool EmployeeExists(string id) { return _context.Employees.Any(e => e.EmployeeID == id); }
        private bool StoreExists(string id) { return _context.Stores.Any(e => e.StoreID == id); }
        private bool AccountExists(string id) { return _context.Accounts.Any(e => e.AccountID == id); }
    }
}
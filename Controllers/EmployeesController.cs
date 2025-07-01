// DecalXeAPI/Controllers/EmployeesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces;
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        // Bỏ _context và _mapper vì Controller giờ chỉ làm việc với Service
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(IEmployeeService employeeService, ILogger<EmployeesController> logger)
        {
            _employeeService = employeeService;
            _logger = logger;
        }

        // GET: api/Employees
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Sales,Technician,Customer")]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách nhân viên.");
            var employees = await _employeeService.GetEmployeesAsync();
            return Ok(employees);
        }

        // GET: api/Employees/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Sales,Technician,Customer")]
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

        // API: POST api/Employees (ĐÃ SỬA)
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<EmployeeDto>> PostEmployee(EmployeeCreateUpdateDto employeeDto) // Nhận vào DTO mới
        {
            _logger.LogInformation("Yêu cầu tạo nhân viên mới: {FirstName} {LastName}", employeeDto.FirstName, employeeDto.LastName);
            
            try
            {
                // Truyền thẳng DTO vào service, không cần kiểm tra gì ở đây nữa
                var createdEmployeeDto = await _employeeService.CreateEmployeeAsync(employeeDto);
                _logger.LogInformation("Đã tạo nhân viên mới với ID: {EmployeeID}", createdEmployeeDto.EmployeeID);
                return CreatedAtAction(nameof(GetEmployee), new { id = createdEmployeeDto.EmployeeID }, createdEmployeeDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không mong muốn khi tạo nhân viên.");
                return StatusCode(500, "Lỗi hệ thống khi tạo nhân viên.");
            }
        }

        // API: PUT api/Employees/{id} (ĐÃ SỬA)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> PutEmployee(string id, EmployeeCreateUpdateDto employeeDto) // Nhận vào DTO mới
        {
            _logger.LogInformation("Yêu cầu cập nhật nhân viên với ID: {EmployeeID}", id);
            
            try
            {
                var updatedEmployee = await _employeeService.UpdateEmployeeAsync(id, employeeDto);
                if (updatedEmployee == null)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch(NotImplementedException)
            {
                return StatusCode(501, "Chức năng cập nhật nhân viên chưa được hoàn thiện.");
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
                return NotFound();
            }
            return NoContent();
        }

        // Các hàm hỗ trợ StoreExists, AccountExists đã được xóa vì không cần thiết nữa
    }
}
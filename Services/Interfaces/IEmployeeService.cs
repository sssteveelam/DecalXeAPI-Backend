// DecalXeAPI/Services/Interfaces/IEmployeeService.cs
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeDto>> GetEmployeesAsync();
        Task<EmployeeDto?> GetEmployeeByIdAsync(string id);
        // Sửa lại để nhận DTO mới
        Task<EmployeeDto> CreateEmployeeAsync(EmployeeCreateUpdateDto employeeDto); 
        Task<EmployeeDto?> UpdateEmployeeAsync(string id, EmployeeCreateUpdateDto employeeDto);
        Task<bool> DeleteEmployeeAsync(string id);
        Task<bool> EmployeeExistsAsync(string id);
        Task<bool> StoreExistsAsync(string id);
        Task<bool> AccountExistsAsync(string id);
    }
}
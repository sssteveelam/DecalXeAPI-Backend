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
        Task<EmployeeDto> CreateEmployeeAsync(Employee employee);
        Task<bool> UpdateEmployeeAsync(string id, Employee employee);
        Task<bool> DeleteEmployeeAsync(string id);


        Task<bool> EmployeeExistsAsync(string id);
        Task<bool> StoreExistsAsync(string id);
        Task<bool> AccountExistsAsync(string id);
    }
}
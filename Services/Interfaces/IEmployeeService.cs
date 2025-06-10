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

        // Phương thức lấy thống kê hiệu suất của nhân viên
        Task<IEnumerable<EmployeePerformanceDto>> GetEmployeePerformanceStatisticsAsync();

        // Các hàm kiểm tra tồn tại (Exists) cần thiết cho Service này
        Task<bool> EmployeeExistsAsync(string id);
        Task<bool> StoreExistsAsync(string id); // Cần để kiểm tra FK
        Task<bool> AccountExistsAsync(string id); // Cần để kiểm tra FK
    }
}
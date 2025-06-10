using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetCustomersAsync();
        Task<CustomerDto?> GetCustomerByIdAsync(string id);
        Task<CustomerDto> CreateCustomerAsync(Customer customer);
        Task<bool> UpdateCustomerAsync(string id, Customer customer);
        Task<bool> DeleteCustomerAsync(string id);

        // Hàm kiểm tra tồn tại (Exists) cần thiết cho Service này
        Task<bool> CustomerExistsAsync(string id);
        Task<bool> AccountExistsAsync(string id); // Cần để kiểm tra FK
    }
}
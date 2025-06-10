using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace DecalXeAPI.Services.Implementations
{
    public class CustomerService : ICustomerService // <-- Kế thừa từ ICustomerService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(ApplicationDbContext context, IMapper mapper, ILogger<CustomerService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<CustomerDto>> GetCustomersAsync()
        {
            _logger.LogInformation("Lấy danh sách khách hàng.");
            var customers = await _context.Customers.Include(c => c.Account).ThenInclude(a => a.Role).ToListAsync();
            var customerDtos = _mapper.Map<List<CustomerDto>>(customers);
            return customerDtos;
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy khách hàng với ID: {CustomerID}", id);
            var customer = await _context.Customers.Include(c => c.Account).ThenInclude(a => a.Role).FirstOrDefaultAsync(c => c.CustomerID == id);

            if (customer == null)
            {
                _logger.LogWarning("Không tìm thấy khách hàng với ID: {CustomerID}", id);
                return null;
            }

            var customerDto = _mapper.Map<CustomerDto>(customer);
            _logger.LogInformation("Đã trả về khách hàng với ID: {CustomerID}", id);
            return customerDto;
        }

        public async Task<CustomerDto> CreateCustomerAsync(Customer customer)
        {
            _logger.LogInformation("Yêu cầu tạo khách hàng mới: {FirstName} {LastName}", customer.FirstName, customer.LastName);

            // Kiểm tra AccountID có tồn tại không nếu được cung cấp
            if (!string.IsNullOrEmpty(customer.AccountID) && !await AccountExistsAsync(customer.AccountID))
            {
                _logger.LogWarning("AccountID không tồn tại khi tạo khách hàng: {AccountID}", customer.AccountID);
                throw new ArgumentException("AccountID không tồn tại.");
            }

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            await _context.Entry(customer).Reference(c => c.Account).LoadAsync();
            if (customer.Account != null)
            {
                await _context.Entry(customer.Account).Reference(a => a.Role).LoadAsync();
            }

            var customerDto = _mapper.Map<CustomerDto>(customer);
            _logger.LogInformation("Đã tạo khách hàng mới với ID: {CustomerID}", customer.CustomerID);
            return customerDto;
        }

        public async Task<bool> UpdateCustomerAsync(string id, Customer customer)
        {
            _logger.LogInformation("Yêu cầu cập nhật khách hàng với ID: {CustomerID}", id);

            if (id != customer.CustomerID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với CustomerID trong body ({CustomerIDBody})", id, customer.CustomerID);
                return false;
            }

            if (!await CustomerExistsAsync(id))
            {
                _logger.LogWarning("Không tìm thấy khách hàng để cập nhật với ID: {CustomerID}", id);
                return false;
            }

            // Kiểm tra AccountID có tồn tại không nếu được cung cấp
            if (!string.IsNullOrEmpty(customer.AccountID) && !await AccountExistsAsync(customer.AccountID))
            {
                _logger.LogWarning("AccountID không tồn tại khi cập nhật khách hàng: {AccountID}", customer.AccountID);
                throw new ArgumentException("AccountID không tồn tại.");
            }

            _context.Entry(customer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật khách hàng với ID: {CustomerID}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật khách hàng với ID: {CustomerID}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCustomerAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa khách hàng với ID: {CustomerID}", id);
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                _logger.LogWarning("Không tìm thấy khách hàng để xóa với ID: {CustomerID}", id);
                return false;
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa khách hàng với ID: {CustomerID}", id);
            return true;
        }

        // --- HÀM HỖ TRỢ: KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (PUBLIC CHO INTERFACE) ---
        public async Task<bool> CustomerExistsAsync(string id)
        {
            return await _context.Customers.AnyAsync(e => e.CustomerID == id);
        }

        public async Task<bool> AccountExistsAsync(string id)
        {
            return await _context.Accounts.AnyAsync(e => e.AccountID == id);
        }
    }
}
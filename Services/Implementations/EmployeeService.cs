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
    public class EmployeeService : IEmployeeService // <-- Kế thừa từ IEmployeeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(ApplicationDbContext context, IMapper mapper, ILogger<EmployeeService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<EmployeeDto>> GetEmployeesAsync()
        {
            _logger.LogInformation("Lấy danh sách nhân viên.");
            var employees = await _context.Employees
                                            .Include(e => e.Account)
                                                .ThenInclude(a => a.Role)
                                            .Include(e => e.Store)
                                            .ToListAsync();
            var employeeDtos = _mapper.Map<List<EmployeeDto>>(employees);
            return employeeDtos;
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy nhân viên với ID: {EmployeeID}", id);
            var employee = await _context.Employees
                                            .Include(e => e.Account)
                                                .ThenInclude(a => a.Role)
                                            .Include(e => e.Store)
                                            .FirstOrDefaultAsync(e => e.EmployeeID == id);

            if (employee == null)
            {
                _logger.LogWarning("Không tìm thấy nhân viên với ID: {EmployeeID}", id);
                return null;
            }

            var employeeDto = _mapper.Map<EmployeeDto>(employee);
            _logger.LogInformation("Đã trả về nhân viên với ID: {EmployeeID}", id);
            return employeeDto;
        }

        // Trong file: DecalXeAPI/Services/Implementations/EmployeeService.cs
        public async Task<EmployeeDto> CreateEmployeeAsync(Employee employee)
        {
            _logger.LogInformation("Yêu cầu tạo nhân viên mới: {FirstName} {LastName}", employee.FirstName, employee.LastName);

            // Kiểm tra các khóa ngoại trước
            if (!string.IsNullOrEmpty(employee.StoreID) && !await StoreExistsAsync(employee.StoreID))
            {
                _logger.LogWarning("StoreID không tồn tại khi tạo nhân viên: {StoreID}", employee.StoreID);
                throw new ArgumentException("StoreID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(employee.AccountID) && !await AccountExistsAsync(employee.AccountID))
            {
                _logger.LogWarning("AccountID không tồn tại khi tạo nhân viên: {AccountID}", employee.AccountID);
                throw new ArgumentException("AccountID không tồn tại.");
            }
            
            // --- BỔ SUNG LOGIC CHO IsActive TỪ ACCOUNT ---
            // Vì DTO đã gán giá trị vào Account, ta chỉ cần đảm bảo Account không null
            if (employee.Account != null)
            {
                // IsActive đã được gán vào đối tượng account từ trước khi gọi hàm này
                // (thông qua AutoMapper hoặc gán trực tiếp trong Controller)
                _logger.LogInformation("Tài khoản cho nhân viên mới sẽ có trạng thái IsActive = {IsActive}", employee.Account.IsActive);
            }
            // --- KẾT THÚC BỔ SUNG ---

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Tải lại các thực thể liên quan để AutoMapper có thể ánh xạ đầy đủ
            await _context.Entry(employee).Reference(e => e.Account).LoadAsync();
            if (employee.Account != null)
            {
                await _context.Entry(employee.Account).Reference(a => a.Role).LoadAsync();
            }
            await _context.Entry(employee).Reference(e => e.Store).LoadAsync();

            var employeeDto = _mapper.Map<EmployeeDto>(employee);
            _logger.LogInformation("Đã tạo nhân viên mới với ID: {EmployeeID}", employee.EmployeeID);
            return employeeDto;
        }

        public async Task<bool> UpdateEmployeeAsync(string id, Employee employee)
        {
            _logger.LogInformation("Yêu cầu cập nhật nhân viên với ID: {EmployeeID}", id);

            if (id != employee.EmployeeID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với EmployeeID trong body ({EmployeeIDBody})", id, employee.EmployeeID);
                return false;
            }

            if (!await EmployeeExistsAsync(id))
            {
                _logger.LogWarning("Không tìm thấy nhân viên để cập nhật với ID: {EmployeeID}", id);
                return false;
            }

            // Kiểm tra FKs
            if (!string.IsNullOrEmpty(employee.StoreID) && !await StoreExistsAsync(employee.StoreID))
            {
                _logger.LogWarning("StoreID không tồn tại khi cập nhật nhân viên: {StoreID}", employee.StoreID);
                throw new ArgumentException("StoreID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(employee.AccountID) && !await AccountExistsAsync(employee.AccountID))
            {
                _logger.LogWarning("AccountID không tồn tại khi cập nhật nhân viên: {AccountID}", employee.AccountID);
                throw new ArgumentException("AccountID không tồn tại.");
            }

            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật nhân viên với ID: {EmployeeID}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật nhân viên với ID: {EmployeeID}", id);
                throw;
            }
        }

        public async Task<bool> DeleteEmployeeAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa nhân viên với ID: {EmployeeID}", id);
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                _logger.LogWarning("Không tìm thấy nhân viên để xóa với ID: {EmployeeID}", id);
                return false;
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa nhân viên với ID: {EmployeeID}", id);
            return true;
        }
        

        // --- HÀM HỖ TRỢ: KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (PUBLIC CHO INTERFACE) ---
        public async Task<bool> EmployeeExistsAsync(string id)
        {
            return await _context.Employees.AnyAsync(e => e.EmployeeID == id);
        }

        public async Task<bool> StoreExistsAsync(string id)
        {
            return await _context.Stores.AnyAsync(e => e.StoreID == id);
        }

        public async Task<bool> AccountExistsAsync(string id)
        {
            return await _context.Accounts.AnyAsync(e => e.AccountID == id);
        }
    }
}
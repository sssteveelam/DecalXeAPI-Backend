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
    public class EmployeeService : IEmployeeService
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
                                            // .Include(e => e.TechnicianDailySchedules) // <-- ĐÃ XÓA INCLUDE NÀY
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

        public async Task<EmployeeDto> CreateEmployeeAsync(Employee employee)
        {
            _logger.LogInformation("Yêu cầu tạo nhân viên mới: {FirstName} {LastName}", employee.FirstName, employee.LastName);

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

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

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

            // Kiểm tra các mối quan hệ trước khi xóa Employee
            if (await _context.Orders.AnyAsync(o => o.AssignedEmployeeID == id) ||
                await _context.Designs.AnyAsync(d => d.DesignerID == id) ||
                await _context.CustomServiceRequests.AnyAsync(csr => csr.SalesEmployeeID == id) ||
                await _context.OrderStageHistories.AnyAsync(osh => osh.ChangedByEmployeeID == id) ||
                await _context.Payments.AnyAsync(p => p.PayerName == (employee.FirstName + " " + employee.LastName)) // Giữ lại nếu PayerName là tên nhân viên
            )
            {
                _logger.LogWarning("Không thể xóa nhân viên {EmployeeID} vì đang được sử dụng bởi Orders, Designs, CustomServiceRequests, OrderStageHistories hoặc Payments.", id);
                throw new InvalidOperationException("Không thể xóa nhân viên này vì đang được sử dụng bởi các đơn hàng, thiết kế, yêu cầu tùy chỉnh, lịch sử giai đoạn hoặc thanh toán.");
            }

            // Nếu bạn có các bảng AdminDetail, ManagerDetail, SalesPersonDetail, DesignerDetail, TechnicianDetail
            // thì cần thêm kiểm tra ràng buộc ở đây:
            if (await _context.AdminDetails.AnyAsync(ad => ad.EmployeeID == id) ||
                await _context.ManagerDetails.AnyAsync(md => md.EmployeeID == id) ||
                await _context.SalesPersonDetails.AnyAsync(spd => spd.EmployeeID == id) ||
                await _context.DesignerDetails.AnyAsync(dd => dd.EmployeeID == id) ||
                await _context.TechnicianDetails.AnyAsync(td => td.EmployeeID == id))
            {
                _logger.LogWarning("Không thể xóa nhân viên {EmployeeID} vì đang được sử dụng bởi một vai trò chi tiết (Admin, Manager, Sales, Designer, Technician).", id);
                throw new InvalidOperationException("Không thể xóa nhân viên này vì đang được sử dụng bởi một vai trò chi tiết.");
            }


            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa nhân viên với ID: {EmployeeID}", id);
            return true;
        }

        // BỎ PHƯƠNG THỨC THỐNG KÊ HIỆU SUẤT VÌ PHỤ THUỘC BẢNG ĐÃ XÓA
        // public async Task<IEnumerable<EmployeePerformanceDto>> GetEmployeePerformanceStatisticsAsync()
        // {
        //     // Logic này phụ thuộc vào TechnicianDailySchedules và ScheduledWorkUnits (đã bị xóa)
        //     // Nên phương thức này đã được loại bỏ
        //     throw new NotImplementedException("Phương thức GetEmployeePerformanceStatisticsAsync không còn được hỗ trợ với cấu trúc database hiện tại.");
        // }


        // Hàm hỗ trợ: Kiểm tra sự tồn tại của các đối tượng (PUBLIC CHO INTERFACE)
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
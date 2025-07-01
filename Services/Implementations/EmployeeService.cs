// DecalXeAPI/Services/Implementations/EmployeeService.cs
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
                .Include(e => e.Account).ThenInclude(a => a.Role)
                .Include(e => e.Store)
                .Include(e => e.AdminDetail)
                .Include(e => e.ManagerDetail)
                .Include(e => e.SalesPersonDetail)
                .Include(e => e.DesignerDetail)
                .Include(e => e.TechnicianDetail)
                .ToListAsync();
            return _mapper.Map<List<EmployeeDto>>(employees);
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(string id)
        {
             var employee = await _context.Employees
                .Include(e => e.Account).ThenInclude(a => a.Role)
                .Include(e => e.Store)
                .Include(e => e.AdminDetail)
                .Include(e => e.ManagerDetail)
                .Include(e => e.SalesPersonDetail)
                .Include(e => e.DesignerDetail)
                .Include(e => e.TechnicianDetail)
                .FirstOrDefaultAsync(e => e.EmployeeID == id);
            return _mapper.Map<EmployeeDto>(employee);
        }

        public async Task<EmployeeDto> CreateEmployeeAsync(EmployeeCreateUpdateDto employeeDto)
        {
            // Bắt đầu một transaction để đảm bảo tất cả hành động hoặc thành công, hoặc thất bại
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tạo đối tượng Employee cơ bản
                var employee = _mapper.Map<Employee>(employeeDto);
                employee.EmployeeID = Guid.NewGuid().ToString(); // Tạo ID mới
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync(); // Lưu để lấy EmployeeID

                // 2. Dựa vào vai trò để tạo bảng Detail tương ứng
                var role = await _context.Accounts
                    .Where(a => a.AccountID == employee.AccountID)
                    .Select(a => a.Role.RoleName)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(role)) throw new Exception("Không tìm thấy vai trò cho tài khoản này.");

                switch (role.ToLower())
                {
                    case "admin":
                        if (employeeDto.AdminDetail == null) throw new ArgumentException("Thông tin chi tiết của Admin là bắt buộc.");
                        var adminDetail = _mapper.Map<AdminDetail>(employeeDto.AdminDetail);
                        adminDetail.EmployeeID = employee.EmployeeID;
                        _context.AdminDetails.Add(adminDetail);
                        break;
                    case "manager":
                        // Tương tự cho các vai trò khác
                        break;
                    case "sales":
                         if (employeeDto.SalesPersonDetail == null) throw new ArgumentException("Thông tin chi tiết của Sales là bắt buộc.");
                        var salesDetail = _mapper.Map<SalesPersonDetail>(employeeDto.SalesPersonDetail);
                        salesDetail.EmployeeID = employee.EmployeeID;
                        _context.SalesPersonDetails.Add(salesDetail);
                        break;
                    case "designer":
                         if (employeeDto.DesignerDetail == null) throw new ArgumentException("Thông tin chi tiết của Designer là bắt buộc.");
                        var designerDetail = _mapper.Map<DesignerDetail>(employeeDto.DesignerDetail);
                        designerDetail.EmployeeID = employee.EmployeeID;
                        _context.DesignerDetails.Add(designerDetail);
                        break;
                    case "technician":
                        if (employeeDto.TechnicianDetail == null) throw new ArgumentException("Thông tin chi tiết của Technician là bắt buộc.");
                        var techDetail = _mapper.Map<TechnicianDetail>(employeeDto.TechnicianDetail);
                        techDetail.EmployeeID = employee.EmployeeID;
                        _context.TechnicianDetails.Add(techDetail);
                        break;
                }

                await _context.SaveChangesAsync(); // Lưu các bảng Detail
                await transaction.CommitAsync(); // Hoàn tất giao dịch

                return await GetEmployeeByIdAsync(employee.EmployeeID) ?? throw new Exception("Lỗi khi lấy lại nhân viên vừa tạo.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Nếu có lỗi, hoàn tác mọi thay đổi
                _logger.LogError(ex, "Lỗi khi tạo nhân viên mới.");
                throw;
            }
        }

        public async Task<EmployeeDto?> UpdateEmployeeAsync(string id, EmployeeCreateUpdateDto employeeDto)
        {
            // Logic cập nhật sẽ phức tạp hơn, cần phải xóa detail cũ, tạo detail mới nếu vai trò thay đổi
            // Tạm thời để trống, chúng ta sẽ hoàn thiện sau.
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteEmployeeAsync(string id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return false;
            _context.Employees.Remove(employee); // EF Core sẽ tự động xóa các bảng Detail liên quan do có quan hệ 1-1
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EmployeeExistsAsync(string id) => await _context.Employees.AnyAsync(e => e.EmployeeID == id);
        public async Task<bool> StoreExistsAsync(string id) => await _context.Stores.AnyAsync(e => e.StoreID == id);
        public async Task<bool> AccountExistsAsync(string id) => await _context.Accounts.AnyAsync(e => e.AccountID == id);
    }
}
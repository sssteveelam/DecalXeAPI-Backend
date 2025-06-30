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
    public class CustomerVehicleService : ICustomerVehicleService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerVehicleService> _logger;

        public CustomerVehicleService(ApplicationDbContext context, IMapper mapper, ILogger<CustomerVehicleService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<CustomerVehicleDto>> GetCustomerVehiclesAsync()
        {
            _logger.LogInformation("Lấy danh sách xe khách hàng.");
            var vehicles = await _context.CustomerVehicles
                                            .Include(cv => cv.Customer)
                                            .Include(cv => cv.VehicleModel!) // Đổi tên từ CarModel
                                                .ThenInclude(vm => vm.VehicleBrand) // Đổi tên từ CarBrand
                                            .ToListAsync();
            return _mapper.Map<List<CustomerVehicleDto>>(vehicles);
        }

        public async Task<CustomerVehicleDto?> GetCustomerVehicleByIdAsync(string id)
        {
            _logger.LogInformation("Lấy xe khách hàng với ID: {ID}", id);
            var vehicle = await _context.CustomerVehicles
                                            .Include(cv => cv.Customer)
                                            .Include(cv => cv.VehicleModel!)
                                                .ThenInclude(vm => vm.VehicleBrand)
                                            .FirstOrDefaultAsync(cv => cv.VehicleID == id);
            return _mapper.Map<CustomerVehicleDto>(vehicle);
        }

        public async Task<CustomerVehicleDto> CreateCustomerVehicleAsync(CustomerVehicle customerVehicle)
        {
            _logger.LogInformation("Tạo xe khách hàng mới: {ChassisNumber}", customerVehicle.ChassisNumber); // Đổi từ LicensePlate

            if (!await CustomerExistsAsync(customerVehicle.CustomerID))
            {
                throw new ArgumentException("CustomerID không tồn tại.");
            }
            if (!await VehicleModelExistsAsync(customerVehicle.VehicleModelID)) // Đổi từ CarModelID
            {
                throw new ArgumentException("VehicleModelID không tồn tại.");
            }
            if (await _context.CustomerVehicles.AnyAsync(cv => cv.ChassisNumber == customerVehicle.ChassisNumber)) // Kiểm tra trùng số khung
            {
                throw new ArgumentException("Số khung xe đã tồn tại.");
            }

            _context.CustomerVehicles.Add(customerVehicle);
            await _context.SaveChangesAsync();

            await _context.Entry(customerVehicle).Reference(cv => cv.Customer).LoadAsync();
            await _context.Entry(customerVehicle).Reference(cv => cv.VehicleModel).LoadAsync(); // Đổi từ CarModel
            if (customerVehicle.VehicleModel != null)
            {
                await _context.Entry(customerVehicle.VehicleModel).Reference(vm => vm.VehicleBrand).LoadAsync(); // Đổi từ CarBrand
            }
            return _mapper.Map<CustomerVehicleDto>(customerVehicle);
        }

        public async Task<bool> UpdateCustomerVehicleAsync(string id, CustomerVehicle customerVehicle)
        {
            _logger.LogInformation("Cập nhật xe khách hàng với ID: {ID}", id);
            if (id != customerVehicle.VehicleID) return false;
            if (!await CustomerVehicleExistsAsync(id)) return false;
            if (!await CustomerExistsAsync(customerVehicle.CustomerID))
            {
                throw new ArgumentException("CustomerID không tồn tại.");
            }
            if (!await VehicleModelExistsAsync(customerVehicle.VehicleModelID)) // Đổi từ CarModelID
            {
                throw new ArgumentException("VehicleModelID không tồn tại.");
            }
            if (await _context.CustomerVehicles.AnyAsync(cv => cv.ChassisNumber == customerVehicle.ChassisNumber && cv.VehicleID != id))
            {
                throw new ArgumentException("Số khung xe đã tồn tại.");
            }

            _context.Entry(customerVehicle).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); return true; }
            catch (DbUpdateConcurrencyException ex) { _logger.LogError(ex, "Lỗi xung đột khi cập nhật xe khách hàng."); throw; }
        }

        public async Task<bool> DeleteCustomerVehicleAsync(string id)
        {
            _logger.LogInformation("Xóa xe khách hàng với ID: {ID}", id);
            var vehicle = await _context.CustomerVehicles.FindAsync(id);
            if (vehicle == null) return false;
            // Kiểm tra các mối quan hệ trước khi xóa CustomerVehicle
            if (await _context.Orders.AnyAsync(o => o.VehicleID == id) ||
                await _context.Warranties.AnyAsync(w => w.VehicleID == id))
            {
                throw new InvalidOperationException("Không thể xóa xe khách hàng vì có đơn hàng hoặc bảo hành liên kết.");
            }
            _context.CustomerVehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CustomerVehicleExistsAsync(string id)
        {
            return await _context.CustomerVehicles.AnyAsync(e => e.VehicleID == id);
        }

        public async Task<bool> CustomerExistsAsync(string id)
        {
            return await _context.Customers.AnyAsync(e => e.CustomerID == id);
        }

        public async Task<bool> VehicleModelExistsAsync(string id) // Đổi từ CarModelExists
        {
            return await _context.VehicleModels.AnyAsync(e => e.ModelID == id); // Trỏ đến VehicleModels
        }
    }
}
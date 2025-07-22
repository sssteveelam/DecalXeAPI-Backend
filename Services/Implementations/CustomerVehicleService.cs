using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

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

        public async Task<IEnumerable<CustomerVehicleDto>> GetAllAsync()
        {
            _logger.LogInformation("Lấy danh sách tất cả xe khách hàng.");
            var vehicles = await _context.CustomerVehicles
                .Include(v => v.Customer)
                .Include(v => v.VehicleModel)
                .ThenInclude(vm => vm.VehicleBrand)
                .ToListAsync();
            
            return _mapper.Map<IEnumerable<CustomerVehicleDto>>(vehicles);
        }

        public async Task<CustomerVehicleDto?> GetByIdAsync(string id)
        {
            _logger.LogInformation("Lấy xe với ID: {VehicleID}", id);
            var vehicle = await _context.CustomerVehicles
                .Include(v => v.Customer)
                .Include(v => v.VehicleModel)
                .ThenInclude(vm => vm.VehicleBrand)
                .FirstOrDefaultAsync(v => v.VehicleID == id);

            if (vehicle == null)
            {
                _logger.LogWarning("Không tìm thấy xe với ID: {VehicleID}", id);
                return null;
            }

            return _mapper.Map<CustomerVehicleDto>(vehicle);
        }

        public async Task<CustomerVehicleDto?> GetByLicensePlateAsync(string licensePlate)
        {
            _logger.LogInformation("Lấy xe với biển số: {LicensePlate}", licensePlate);
            var vehicle = await _context.CustomerVehicles
                .Include(v => v.Customer)
                .Include(v => v.VehicleModel)
                .ThenInclude(vm => vm.VehicleBrand)
                .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);

            if (vehicle == null)
            {
                _logger.LogWarning("Không tìm thấy xe với biển số: {LicensePlate}", licensePlate);
                return null;
            }

            return _mapper.Map<CustomerVehicleDto>(vehicle);
        }

        public async Task<IEnumerable<CustomerVehicleDto>> GetByCustomerIdAsync(string customerId)
        {
            _logger.LogInformation("Lấy danh sách xe của khách hàng: {CustomerID}", customerId);
            var vehicles = await _context.CustomerVehicles
                .Include(v => v.Customer)
                .Include(v => v.VehicleModel)
                .ThenInclude(vm => vm.VehicleBrand)
                .Where(v => v.CustomerID == customerId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CustomerVehicleDto>>(vehicles);
        }

        public async Task<CustomerVehicleDto> CreateAsync(CreateCustomerVehicleDto createDto)
        {
            _logger.LogInformation("Tạo xe mới cho khách hàng: {CustomerID}", createDto.CustomerID);
            
            // Kiểm tra số khung đã tồn tại
            if (await ChassisNumberExistsAsync(createDto.ChassisNumber))
            {
                throw new InvalidOperationException($"Số khung {createDto.ChassisNumber} đã tồn tại.");
            }

            // Kiểm tra biển số đã tồn tại (nếu có)
            if (!string.IsNullOrEmpty(createDto.LicensePlate) && await LicensePlateExistsAsync(createDto.LicensePlate))
            {
                throw new InvalidOperationException($"Biển số {createDto.LicensePlate} đã tồn tại.");
            }

            var vehicle = _mapper.Map<CustomerVehicle>(createDto);
            vehicle.VehicleID = Guid.NewGuid().ToString();

            _context.CustomerVehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            // Lấy lại với thông tin đầy đủ
            var createdVehicle = await _context.CustomerVehicles
                .Include(v => v.Customer)
                .Include(v => v.VehicleModel)
                .ThenInclude(vm => vm.VehicleBrand)
                .FirstOrDefaultAsync(v => v.VehicleID == vehicle.VehicleID);

            _logger.LogInformation("Đã tạo xe với ID: {VehicleID}", vehicle.VehicleID);
            return _mapper.Map<CustomerVehicleDto>(createdVehicle!);
        }

        public async Task<CustomerVehicleDto?> UpdateAsync(string id, UpdateCustomerVehicleDto updateDto)
        {
            _logger.LogInformation("Cập nhật xe với ID: {VehicleID}", id);
            var vehicle = await _context.CustomerVehicles.FindAsync(id);

            if (vehicle == null)
            {
                _logger.LogWarning("Không tìm thấy xe với ID: {VehicleID}", id);
                return null;
            }

            // Kiểm tra số khung mới (nếu có thay đổi)
            if (!string.IsNullOrEmpty(updateDto.ChassisNumber) && 
                updateDto.ChassisNumber != vehicle.ChassisNumber && 
                await ChassisNumberExistsAsync(updateDto.ChassisNumber))
            {
                throw new InvalidOperationException($"Số khung {updateDto.ChassisNumber} đã tồn tại.");
            }

            // Kiểm tra biển số mới (nếu có thay đổi)
            if (!string.IsNullOrEmpty(updateDto.LicensePlate) && 
                updateDto.LicensePlate != vehicle.LicensePlate && 
                await LicensePlateExistsAsync(updateDto.LicensePlate))
            {
                throw new InvalidOperationException($"Biển số {updateDto.LicensePlate} đã tồn tại.");
            }

            _mapper.Map(updateDto, vehicle);
            await _context.SaveChangesAsync();

            // Lấy lại với thông tin đầy đủ
            var updatedVehicle = await _context.CustomerVehicles
                .Include(v => v.Customer)
                .Include(v => v.VehicleModel)
                .ThenInclude(vm => vm.VehicleBrand)
                .FirstOrDefaultAsync(v => v.VehicleID == id);

            _logger.LogInformation("Đã cập nhật xe với ID: {VehicleID}", id);
            return _mapper.Map<CustomerVehicleDto>(updatedVehicle!);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            _logger.LogInformation("Xóa xe với ID: {VehicleID}", id);
            var vehicle = await _context.CustomerVehicles.FindAsync(id);

            if (vehicle == null)
            {
                _logger.LogWarning("Không tìm thấy xe với ID: {VehicleID}", id);
                return false;
            }

            _context.CustomerVehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Đã xóa xe với ID: {VehicleID}", id);
            return true;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.CustomerVehicles.AnyAsync(v => v.VehicleID == id);
        }

        public async Task<bool> LicensePlateExistsAsync(string licensePlate)
        {
            return await _context.CustomerVehicles.AnyAsync(v => v.LicensePlate == licensePlate);
        }

        public async Task<bool> ChassisNumberExistsAsync(string chassisNumber)
        {
            return await _context.CustomerVehicles.AnyAsync(v => v.ChassisNumber == chassisNumber);
        }
    }
}

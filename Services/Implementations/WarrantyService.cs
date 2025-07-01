// DecalXeAPI/Services/Implementations/WarrantyService.cs
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
    public class WarrantyService : IWarrantyService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<WarrantyService> _logger;

        public WarrantyService(ApplicationDbContext context, IMapper mapper, ILogger<WarrantyService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<WarrantyDto>> GetWarrantiesAsync()
        {
            _logger.LogInformation("Lấy danh sách bảo hành.");
            // Thay Include(w => w.Order) bằng Include cho xe và khách hàng
            var warranties = await _context.Warranties
                                           .Include(w => w.CustomerVehicle)
                                                .ThenInclude(cv => cv.Customer) 
                                           .ToListAsync();
            return _mapper.Map<List<WarrantyDto>>(warranties);
        }

        public async Task<WarrantyDto?> GetWarrantyByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy bảo hành với ID: {WarrantyID}", id);
             // Thay Include(w => w.Order) bằng Include cho xe và khách hàng
            var warranty = await _context.Warranties
                                           .Include(w => w.CustomerVehicle)
                                                .ThenInclude(cv => cv.Customer)
                                           .FirstOrDefaultAsync(w => w.WarrantyID == id);

            if (warranty == null) return null;

            return _mapper.Map<WarrantyDto>(warranty);
        }

        public async Task<WarrantyDto> CreateWarrantyAsync(Warranty warranty)
        {
            _logger.LogInformation("Yêu cầu tạo bảo hành mới cho VehicleID: {VehicleID}", warranty.VehicleID);

            // Kiểm tra FK mới: VehicleID
            if (string.IsNullOrEmpty(warranty.VehicleID) || !await CustomerVehicleExistsAsync(warranty.VehicleID))
            {
                throw new ArgumentException("VehicleID không tồn tại hoặc không được cung cấp.");
            }

            _context.Warranties.Add(warranty);
            await _context.SaveChangesAsync();

            // Tải lại thông tin liên quan mới
            await _context.Entry(warranty).Reference(w => w.CustomerVehicle).LoadAsync();
            if (warranty.CustomerVehicle != null)
            {
                await _context.Entry(warranty.CustomerVehicle).Reference(cv => cv.Customer).LoadAsync();
            }

            return _mapper.Map<WarrantyDto>(warranty);
        }

        public async Task<bool> UpdateWarrantyAsync(string id, Warranty warranty)
        {
            if (id != warranty.WarrantyID) return false;
            if (!await WarrantyExistsAsync(id)) return false;

             // Kiểm tra FK mới: VehicleID
            if (string.IsNullOrEmpty(warranty.VehicleID) || !await CustomerVehicleExistsAsync(warranty.VehicleID))
            {
                throw new ArgumentException("VehicleID không tồn tại hoặc không được cung cấp.");
            }

            _context.Entry(warranty).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật bảo hành với ID: {WarrantyID}", id);
                throw;
            }
        }

        public async Task<bool> DeleteWarrantyAsync(string id)
        {
            var warranty = await _context.Warranties.FindAsync(id);
            if (warranty == null) return false;

            _context.Warranties.Remove(warranty);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> WarrantyExistsAsync(string id) => await _context.Warranties.AnyAsync(e => e.WarrantyID == id);

        // Hàm hỗ trợ mới
        public async Task<bool> CustomerVehicleExistsAsync(string vehicleId) => await _context.CustomerVehicles.AnyAsync(e => e.VehicleID == vehicleId);
    }
}
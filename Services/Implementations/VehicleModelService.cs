// DecalXeAPI/Services/Implementations/VehicleModelService.cs
using AutoMapper;
using DecalXeAPI.Data;
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Implementations
{
    public class VehicleModelService : IVehicleModelService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<VehicleModelService> _logger;

        public VehicleModelService(ApplicationDbContext context, IMapper mapper, ILogger<VehicleModelService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<VehicleModelDto>> GetAllModelsAsync()
        {
            _logger.LogInformation("Đang lấy danh sách tất cả mẫu xe.");
            var models = await _context.VehicleModels
                                       .Include(m => m.VehicleBrand) // Nạp thông tin hãng xe liên quan
                                       .ToListAsync();
            return _mapper.Map<List<VehicleModelDto>>(models);
        }

        public async Task<VehicleModelDto?> GetModelByIdAsync(string id)
        {
            _logger.LogInformation("Đang tìm mẫu xe với ID: {ModelID}", id);
            var model = await _context.VehicleModels
                                      .Include(m => m.VehicleBrand) // Nạp thông tin hãng xe liên quan
                                      .FirstOrDefaultAsync(m => m.ModelID == id);
            if (model == null)
            {
                _logger.LogWarning("Không tìm thấy mẫu xe với ID: {ModelID}", id);
                return null;
            }
            return _mapper.Map<VehicleModelDto>(model);
        }

        public async Task<(VehicleModelDto?, string?)> CreateModelAsync(VehicleModel model)
        {
            // Kiểm tra xem BrandID có tồn tại không
            if (!await _context.VehicleBrands.AnyAsync(b => b.BrandID == model.BrandID))
            {
                var errorMessage = $"Hãng xe với BrandID '{model.BrandID}' không tồn tại.";
                _logger.LogWarning(errorMessage);
                return (null, errorMessage);
            }

            _logger.LogInformation("Đang tạo mẫu xe mới: {ModelName}", model.ModelName);
            _context.VehicleModels.Add(model);
            await _context.SaveChangesAsync();

            // Nạp lại thông tin brand để mapper có thể lấy BrandName
            await _context.Entry(model).Reference(m => m.VehicleBrand).LoadAsync();

            var createdDto = _mapper.Map<VehicleModelDto>(model);
            return (createdDto, null);
        }

        public async Task<(bool, string?)> UpdateModelAsync(string id, VehicleModel model)
        {
            if (id != model.ModelID)
            {
                return (false, "ID không khớp.");
            }

            // Kiểm tra xem BrandID có tồn tại không
            if (!await _context.VehicleBrands.AnyAsync(b => b.BrandID == model.BrandID))
            {
                var errorMessage = $"Hãng xe với BrandID '{model.BrandID}' không tồn tại.";
                _logger.LogWarning(errorMessage);
                return (false, errorMessage);
            }

            _context.Entry(model).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật mẫu xe với ID: {ModelID}", id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.VehicleModels.AnyAsync(e => e.ModelID == id))
                {
                    return (false, "Không tìm thấy mẫu xe này.");
                }
                else { throw; }
            }
            return (true, null);
        }

        public async Task<bool> DeleteModelAsync(string id)
        {
            var model = await _context.VehicleModels.FindAsync(id);
            if (model == null)
            {
                return false;
            }
            _context.VehicleModels.Remove(model);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa mẫu xe với ID: {ModelID}", id);
            return true;
        }
    }
}
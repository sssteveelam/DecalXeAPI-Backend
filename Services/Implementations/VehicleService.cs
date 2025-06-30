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
    public class VehicleService : IVehicleService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<VehicleService> _logger;

        public VehicleService(ApplicationDbContext context, IMapper mapper, ILogger<VehicleService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // --- API CHO VEHICLEBRAND ---
        public async Task<IEnumerable<VehicleBrandDto>> GetVehicleBrandsAsync()
        {
            _logger.LogInformation("Lấy danh sách hãng xe.");
            var brands = await _context.VehicleBrands.ToListAsync();
            return _mapper.Map<List<VehicleBrandDto>>(brands);
        }

        public async Task<VehicleBrandDto?> GetVehicleBrandByIdAsync(string id)
        {
            _logger.LogInformation("Lấy hãng xe với ID: {ID}", id);
            var brand = await _context.VehicleBrands.FindAsync(id);
            return _mapper.Map<VehicleBrandDto>(brand);
        }

        public async Task<VehicleBrandDto> CreateVehicleBrandAsync(VehicleBrand brand)
        {
            _logger.LogInformation("Tạo hãng xe mới: {Name}", brand.BrandName);
            if (await _context.VehicleBrands.AnyAsync(b => b.BrandName == brand.BrandName))
            {
                throw new ArgumentException("Tên hãng xe đã tồn tại.");
            }
            _context.VehicleBrands.Add(brand);
            await _context.SaveChangesAsync();
            return _mapper.Map<VehicleBrandDto>(brand);
        }

        public async Task<bool> UpdateVehicleBrandAsync(string id, VehicleBrand brand)
        {
            _logger.LogInformation("Cập nhật hãng xe với ID: {ID}", id);
            if (id != brand.BrandID) return false;
            if (!await VehicleBrandExistsAsync(id)) return false;
            if (await _context.VehicleBrands.AnyAsync(b => b.BrandName == brand.BrandName && b.BrandID != id))
            {
                throw new ArgumentException("Tên hãng xe đã tồn tại.");
            }
            _context.Entry(brand).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); return true; }
            catch (DbUpdateConcurrencyException ex) { _logger.LogError(ex, "Lỗi xung đột khi cập nhật hãng xe."); throw; }
        }

        public async Task<bool> DeleteVehicleBrandAsync(string id)
        {
            _logger.LogInformation("Xóa hãng xe với ID: {ID}", id);
            var brand = await _context.VehicleBrands.FindAsync(id);
            if (brand == null) return false;
            if (await _context.VehicleModels.AnyAsync(m => m.BrandID == id))
            {
                throw new InvalidOperationException("Không thể xóa hãng xe vì có mẫu xe liên kết.");
            }
            _context.VehicleBrands.Remove(brand);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VehicleBrandExistsAsync(string id)
        {
            return await _context.VehicleBrands.AnyAsync(e => e.BrandID == id);
        }

        // --- API CHO VEHICLEMODEL ---
        public async Task<IEnumerable<VehicleModelDto>> GetVehicleModelsAsync()
        {
            _logger.LogInformation("Lấy danh sách mẫu xe.");
            var models = await _context.VehicleModels.Include(m => m.VehicleBrand).ToListAsync();
            return _mapper.Map<List<VehicleModelDto>>(models);
        }

        public async Task<VehicleModelDto?> GetVehicleModelByIdAsync(string id)
        {
            _logger.LogInformation("Lấy mẫu xe với ID: {ID}", id);
            var model = await _context.VehicleModels.Include(m => m.VehicleBrand).FirstOrDefaultAsync(m => m.ModelID == id);
            return _mapper.Map<VehicleModelDto>(model);
        }

        public async Task<VehicleModelDto> CreateVehicleModelAsync(VehicleModel model)
        {
            _logger.LogInformation("Tạo mẫu xe mới: {Name}", model.ModelName);
            if (!await VehicleBrandExistsAsync(model.BrandID))
            {
                throw new ArgumentException("BrandID không tồn tại.");
            }
            if (await _context.VehicleModels.AnyAsync(m => m.ModelName == model.ModelName && m.BrandID == model.BrandID))
            {
                throw new ArgumentException("Mẫu xe này đã tồn tại trong hãng này.");
            }
            _context.VehicleModels.Add(model);
            await _context.SaveChangesAsync();
            await _context.Entry(model).Reference(m => m.VehicleBrand).LoadAsync();
            return _mapper.Map<VehicleModelDto>(model);
        }

        public async Task<bool> UpdateVehicleModelAsync(string id, VehicleModel model)
        {
            _logger.LogInformation("Cập nhật mẫu xe với ID: {ID}", id);
            if (id != model.ModelID) return false;
            if (!await VehicleModelExistsAsync(id)) return false;
            if (!await VehicleBrandExistsAsync(model.BrandID))
            {
                throw new ArgumentException("BrandID không tồn tại.");
            }
            if (await _context.VehicleModels.AnyAsync(m => m.ModelName == model.ModelName && m.BrandID == model.BrandID && m.ModelID != id))
            {
                throw new ArgumentException("Mẫu xe này đã tồn tại trong hãng này.");
            }
            _context.Entry(model).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); return true; }
            catch (DbUpdateConcurrencyException ex) { _logger.LogError(ex, "Lỗi xung đột khi cập nhật mẫu xe."); throw; }
        }

        public async Task<bool> DeleteVehicleModelAsync(string id)
        {
            _logger.LogInformation("Xóa mẫu xe với ID: {ID}", id);
            var model = await _context.VehicleModels.FindAsync(id);
            if (model == null) return false;
            if (await _context.CustomerVehicles.AnyAsync(cv => cv.VehicleModelID == id))
            {
                throw new InvalidOperationException("Không thể xóa mẫu xe vì có xe khách hàng liên kết.");
            }
            if (await _context.VehicleModelDecalTemplates.AnyAsync(vmdt => vmdt.ModelID == id))
            {
                throw new InvalidOperationException("Không thể xóa mẫu xe vì có mẫu decal liên kết.");
            }
            if (await _context.TechLaborPrices.AnyAsync(tlp => tlp.VehicleModelID == id))
            {
                throw new InvalidOperationException("Không thể xóa mẫu xe vì có giá công kỹ thuật liên kết.");
            }
           
            _context.VehicleModels.Remove(model);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VehicleModelExistsAsync(string id)
        {
            return await _context.VehicleModels.AnyAsync(e => e.ModelID == id);
        }

        // --- API CHO VEHICLEMODELDECALTEMPLATE ---
        public async Task<IEnumerable<VehicleModelDecalTemplateDto>> GetVehicleModelDecalTemplatesAsync()
        {
            _logger.LogInformation("Lấy danh sách liên kết mẫu xe - mẫu decal.");
            var templates = await _context.VehicleModelDecalTemplates
                                            .Include(vmdt => vmdt.VehicleModel!)
                                                .ThenInclude(vm => vm.VehicleBrand)
                                            .Include(vmdt => vmdt.DecalTemplate)
                                            .ToListAsync();
            return _mapper.Map<List<VehicleModelDecalTemplateDto>>(templates);
        }

        public async Task<VehicleModelDecalTemplateDto?> GetVehicleModelDecalTemplateByIdAsync(string id)
        {
            _logger.LogInformation("Lấy liên kết mẫu xe - mẫu decal với ID: {ID}", id);
            var template = await _context.VehicleModelDecalTemplates
                                            .Include(vmdt => vmdt.VehicleModel!)
                                                .ThenInclude(vm => vm.VehicleBrand)
                                            .Include(vmdt => vmdt.DecalTemplate)
                                            .FirstOrDefaultAsync(vmdt => vmdt.VehicleModelDecalTemplateID == id);
            return _mapper.Map<VehicleModelDecalTemplateDto>(template);
        }

        public async Task<VehicleModelDecalTemplateDto> CreateVehicleModelDecalTemplateAsync(VehicleModelDecalTemplate template)
        {
            _logger.LogInformation("Tạo liên kết mẫu xe - mẫu decal mới cho ModelID: {ModelID}, TemplateID: {TemplateID}", template.ModelID, template.TemplateID);
            if (!await VehicleModelExistsAsync(template.ModelID))
            {
                throw new ArgumentException("ModelID không tồn tại.");
            }
            if (!await _context.DecalTemplates.AnyAsync(dt => dt.TemplateID == template.TemplateID))
            {
                throw new ArgumentException("TemplateID không tồn tại.");
            }
            if (await _context.VehicleModelDecalTemplates.AnyAsync(vmdt => vmdt.ModelID == template.ModelID && vmdt.TemplateID == template.TemplateID))
            {
                throw new ArgumentException("Liên kết mẫu xe - mẫu decal này đã tồn tại.");
            }
            _context.VehicleModelDecalTemplates.Add(template);
            await _context.SaveChangesAsync();
            await _context.Entry(template).Reference(vmdt => vmdt.VehicleModel).LoadAsync();
            if (template.VehicleModel != null)
            {
                await _context.Entry(template.VehicleModel).Reference(vm => vm.VehicleBrand).LoadAsync();
            }
            await _context.Entry(template).Reference(vmdt => vmdt.DecalTemplate).LoadAsync();
            return _mapper.Map<VehicleModelDecalTemplateDto>(template);
        }

        public async Task<bool> UpdateVehicleModelDecalTemplateAsync(string id, VehicleModelDecalTemplate template)
        {
            _logger.LogInformation("Cập nhật liên kết mẫu xe - mẫu decal với ID: {ID}", id);
            if (id != template.VehicleModelDecalTemplateID) return false;
            if (!await VehicleModelDecalTemplateExistsAsync(id)) return false;
            if (!await VehicleModelExistsAsync(template.ModelID))
            {
                throw new ArgumentException("ModelID không tồn tại.");
            }
            if (!await _context.DecalTemplates.AnyAsync(dt => dt.TemplateID == template.TemplateID))
            {
                throw new ArgumentException("TemplateID không tồn tại.");
            }
            if (await _context.VehicleModelDecalTemplates.AnyAsync(vmdt => vmdt.ModelID == template.ModelID && vmdt.TemplateID == template.TemplateID && vmdt.VehicleModelDecalTemplateID != id))
            {
                throw new ArgumentException("Liên kết mẫu xe - mẫu decal này đã tồn tại.");
            }
            _context.Entry(template).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); return true; }
            catch (DbUpdateConcurrencyException ex) { _logger.LogError(ex, "Lỗi xung đột khi cập nhật liên kết."); throw; }
        }

        public async Task<bool> DeleteVehicleModelDecalTemplateAsync(string id)
        {
            _logger.LogInformation("Xóa liên kết mẫu xe - mẫu decal với ID: {ID}", id);
            var template = await _context.VehicleModelDecalTemplates.FindAsync(id);
            if (template == null) return false;
            _context.VehicleModelDecalTemplates.Remove(template);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VehicleModelDecalTemplateExistsAsync(string id)
        {
            return await _context.VehicleModelDecalTemplates.AnyAsync(e => e.VehicleModelDecalTemplateID == id);
        }
    }
}
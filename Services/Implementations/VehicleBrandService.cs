// DecalXeAPI/Services/Implementations/VehicleBrandService.cs
using AutoMapper;
using DecalXeAPI.Data;
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Implementations
{
    public class VehicleBrandService : IVehicleBrandService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<VehicleBrandService> _logger;

        public VehicleBrandService(ApplicationDbContext context, IMapper mapper, ILogger<VehicleBrandService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<VehicleBrandDto>> GetAllBrandsAsync()
        {
            _logger.LogInformation("Đang lấy danh sách tất cả hãng xe.");
            var brands = await _context.VehicleBrands.ToListAsync();
            return _mapper.Map<List<VehicleBrandDto>>(brands);
        }

        public async Task<VehicleBrandDto?> GetBrandByIdAsync(string id)
        {
            _logger.LogInformation("Đang tìm hãng xe với ID: {BrandID}", id);
            var brand = await _context.VehicleBrands.FindAsync(id);
            if (brand == null)
            {
                _logger.LogWarning("Không tìm thấy hãng xe với ID: {BrandID}", id);
                return null;
            }
            return _mapper.Map<VehicleBrandDto>(brand);
        }

        public async Task<VehicleBrandDto> CreateBrandAsync(VehicleBrand brand)
        {
            _logger.LogInformation("Đang tạo hãng xe mới: {BrandName}", brand.BrandName);
            _context.VehicleBrands.Add(brand);
            await _context.SaveChangesAsync();
            return _mapper.Map<VehicleBrandDto>(brand);
        }

        public async Task<bool> UpdateBrandAsync(string id, VehicleBrand brand)
        {
            if (id != brand.BrandID)
            {
                _logger.LogWarning("ID không khớp khi cập nhật: {UrlId} vs {BodyId}", id, brand.BrandID);
                return false;
            }
            _context.Entry(brand).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật hãng xe với ID: {BrandID}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.VehicleBrands.AnyAsync(e => e.BrandID == id))
                {
                    _logger.LogWarning("Không tìm thấy hãng xe để cập nhật với ID: {BrandID}", id);
                    return false;
                }
                else { throw; }
            }
        }

        public async Task<bool> DeleteBrandAsync(string id)
        {
            var brand = await _context.VehicleBrands.FindAsync(id);
            if (brand == null)
            {
                _logger.LogWarning("Không tìm thấy hãng xe để xóa với ID: {BrandID}", id);
                return false;
            }
            _context.VehicleBrands.Remove(brand);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa hãng xe với ID: {BrandID}", id);
            return true;
        }
    }
}
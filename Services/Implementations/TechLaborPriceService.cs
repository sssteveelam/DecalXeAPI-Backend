// DecalXeAPI/Services/Implementations/TechLaborPriceService.cs
using AutoMapper;
using DecalXeAPI.Data;
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Implementations
{
    public class TechLaborPriceService : ITechLaborPriceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TechLaborPriceService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<TechLaborPriceDto>> GetAllAsync()
        {
            var prices = await _context.TechLaborPrices
                                       .Include(p => p.DecalService)
                                       .Include(p => p.VehicleModel)
                                       .ToListAsync();
            return _mapper.Map<List<TechLaborPriceDto>>(prices);
        }

        public async Task<TechLaborPriceDto?> GetByIdAsync(string serviceId, string vehicleModelId)
        {
            var price = await _context.TechLaborPrices
                                      .Include(p => p.DecalService)
                                      .Include(p => p.VehicleModel)
                                      .FirstOrDefaultAsync(p => p.ServiceID == serviceId && p.VehicleModelID == vehicleModelId);
            return _mapper.Map<TechLaborPriceDto>(price);
        }

        public async Task<TechLaborPriceDto> CreateAsync(TechLaborPrice techLaborPrice)
        {
            if (!await _context.DecalServices.AnyAsync(s => s.ServiceID == techLaborPrice.ServiceID))
                throw new ArgumentException($"Dịch vụ với ID '{techLaborPrice.ServiceID}' không tồn tại.");
            if (!await _context.VehicleModels.AnyAsync(m => m.ModelID == techLaborPrice.VehicleModelID))
                throw new ArgumentException($"Mẫu xe với ID '{techLaborPrice.VehicleModelID}' không tồn tại.");

            _context.TechLaborPrices.Add(techLaborPrice);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(techLaborPrice.ServiceID, techLaborPrice.VehicleModelID) ?? throw new Exception("Không thể lấy lại bản ghi vừa tạo.");
        }

        public async Task<TechLaborPriceDto?> UpdateAsync(string serviceId, string vehicleModelId, decimal newPrice)
        {
            var price = await _context.TechLaborPrices.FindAsync(serviceId, vehicleModelId);
            if (price == null) return null;

            price.LaborPrice = newPrice;
            await _context.SaveChangesAsync();
            return await GetByIdAsync(serviceId, vehicleModelId);
        }

        public async Task<bool> DeleteAsync(string serviceId, string vehicleModelId)
        {
            var price = await _context.TechLaborPrices.FindAsync(serviceId, vehicleModelId);
            if (price == null) return false;

            _context.TechLaborPrices.Remove(price);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
// DecalXeAPI/Services/Implementations/ServiceVehicleModelProductService.cs
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
    public class ServiceVehicleModelProductService : IServiceVehicleModelProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ServiceVehicleModelProductService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ServiceVehicleModelProductDto>> GetAllAsync()
        {
            var links = await _context.ServiceVehicleModelProducts
                                      .Include(l => l.DecalService)
                                      .Include(l => l.VehicleModel)
                                      .Include(l => l.Product)
                                      .ToListAsync();
            return _mapper.Map<List<ServiceVehicleModelProductDto>>(links);
        }

        public async Task<ServiceVehicleModelProductDto?> GetByIdAsync(string serviceId, string vehicleModelId, string productId)
        {
            var link = await _context.ServiceVehicleModelProducts
                                     .Include(l => l.DecalService)
                                     .Include(l => l.VehicleModel)
                                     .Include(l => l.Product)
                                     .FirstOrDefaultAsync(l => l.ServiceID == serviceId && l.VehicleModelID == vehicleModelId && l.ProductID == productId);
            return _mapper.Map<ServiceVehicleModelProductDto>(link);
        }

        public async Task<ServiceVehicleModelProductDto> CreateAsync(ServiceVehicleModelProduct link)
        {
            // Kiểm tra sự tồn tại của 3 khóa ngoại
            if (!await _context.DecalServices.AnyAsync(s => s.ServiceID == link.ServiceID))
                throw new ArgumentException($"Dịch vụ ID '{link.ServiceID}' không tồn tại.");
            if (!await _context.VehicleModels.AnyAsync(m => m.ModelID == link.VehicleModelID))
                throw new ArgumentException($"Mẫu xe ID '{link.VehicleModelID}' không tồn tại.");
            if (!await _context.Products.AnyAsync(p => p.ProductID == link.ProductID))
                throw new ArgumentException($"Sản phẩm ID '{link.ProductID}' không tồn tại.");

            _context.ServiceVehicleModelProducts.Add(link);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(link.ServiceID, link.VehicleModelID, link.ProductID) ?? throw new Exception("Lỗi khi tạo mới.");
        }

        public async Task<ServiceVehicleModelProductDto?> UpdateQuantityAsync(string serviceId, string vehicleModelId, string productId, int newQuantity)
        {
            var link = await _context.ServiceVehicleModelProducts.FindAsync(serviceId, vehicleModelId, productId);
            if (link == null) return null;

            link.Quantity = newQuantity;
            await _context.SaveChangesAsync();
            return await GetByIdAsync(serviceId, vehicleModelId, productId);
        }

        public async Task<bool> DeleteAsync(string serviceId, string vehicleModelId, string productId)
        {
            var link = await _context.ServiceVehicleModelProducts.FindAsync(serviceId, vehicleModelId, productId);
            if (link == null) return false;

            _context.ServiceVehicleModelProducts.Remove(link);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
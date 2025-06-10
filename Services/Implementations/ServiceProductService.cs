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
    public class ServiceProductService : IServiceProductService // <-- Kế thừa từ IServiceProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ServiceProductService> _logger;

        public ServiceProductService(ApplicationDbContext context, IMapper mapper, ILogger<ServiceProductService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ServiceProductDto>> GetServiceProductsAsync()
        {
            _logger.LogInformation("Lấy danh sách sản phẩm dịch vụ.");
            var serviceProducts = await _context.ServiceProducts
                                                .Include(sp => sp.DecalService)
                                                .Include(sp => sp.Product)
                                                .ToListAsync();
            var serviceProductDtos = _mapper.Map<List<ServiceProductDto>>(serviceProducts);
            return serviceProductDtos;
        }

        public async Task<ServiceProductDto?> GetServiceProductByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy sản phẩm dịch vụ với ID: {ServiceProductID}", id);
            var serviceProduct = await _context.ServiceProducts
                                                    .Include(sp => sp.DecalService)
                                                    .Include(sp => sp.Product)
                                                    .FirstOrDefaultAsync(sp => sp.ServiceProductID == id);

            if (serviceProduct == null)
            {
                _logger.LogWarning("Không tìm thấy sản phẩm dịch vụ với ID: {ServiceProductID}", id);
                return null;
            }

            var serviceProductDto = _mapper.Map<ServiceProductDto>(serviceProduct);
            _logger.LogInformation("Đã trả về sản phẩm dịch vụ với ID: {ServiceProductID}", id);
            return serviceProductDto;
        }

        public async Task<ServiceProductDto> CreateServiceProductAsync(ServiceProduct serviceProduct)
        {
            _logger.LogInformation("Yêu cầu tạo sản phẩm dịch vụ mới cho ServiceID: {ServiceID}, ProductID: {ProductID}", serviceProduct.ServiceID, serviceProduct.ProductID);

            // Kiểm tra FKs
            if (!string.IsNullOrEmpty(serviceProduct.ServiceID) && !await DecalServiceExistsAsync(serviceProduct.ServiceID))
            {
                _logger.LogWarning("ServiceID không tồn tại khi tạo ServiceProduct: {ServiceID}", serviceProduct.ServiceID);
                throw new ArgumentException("ServiceID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(serviceProduct.ProductID) && !await ProductExistsAsync(serviceProduct.ProductID))
            {
                _logger.LogWarning("ProductID không tồn tại khi tạo ServiceProduct: {ProductID}", serviceProduct.ProductID);
                throw new ArgumentException("ProductID không tồn tại.");
            }

            _context.ServiceProducts.Add(serviceProduct);
            await _context.SaveChangesAsync();

            // Tải lại thông tin liên quan để AutoMapper có thể ánh xạ đầy đủ
            await _context.Entry(serviceProduct).Reference(sp => sp.DecalService).LoadAsync();
            await _context.Entry(serviceProduct).Reference(sp => sp.Product).LoadAsync();

            var serviceProductDto = _mapper.Map<ServiceProductDto>(serviceProduct);
            _logger.LogInformation("Đã tạo sản phẩm dịch vụ mới với ID: {ServiceProductID}", serviceProduct.ServiceProductID);
            return serviceProductDto;
        }

        public async Task<bool> UpdateServiceProductAsync(string id, ServiceProduct serviceProduct)
        {
            _logger.LogInformation("Yêu cầu cập nhật sản phẩm dịch vụ với ID: {ServiceProductID}", id);

            if (id != serviceProduct.ServiceProductID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với ServiceProductID trong body ({ServiceProductIDBody})", id, serviceProduct.ServiceProductID);
                return false;
            }

            if (!await ServiceProductExistsAsync(id))
            {
                _logger.LogWarning("Không tìm thấy sản phẩm dịch vụ để cập nhật với ID: {ServiceProductID}", id);
                return false;
            }

            // Kiểm tra FKs
            if (!string.IsNullOrEmpty(serviceProduct.ServiceID) && !await DecalServiceExistsAsync(serviceProduct.ServiceID))
            {
                _logger.LogWarning("ServiceID không tồn tại khi cập nhật ServiceProduct: {ServiceID}", serviceProduct.ServiceID);
                throw new ArgumentException("ServiceID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(serviceProduct.ProductID) && !await ProductExistsAsync(serviceProduct.ProductID))
            {
                _logger.LogWarning("ProductID không tồn tại khi cập nhật ServiceProduct: {ProductID}", serviceProduct.ProductID);
                throw new ArgumentException("ProductID không tồn tại.");
            }

            _context.Entry(serviceProduct).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật sản phẩm dịch vụ với ID: {ServiceProductID}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật sản phẩm dịch vụ với ID: {ServiceProductID}", id);
                throw;
            }
        }

        public async Task<bool> DeleteServiceProductAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa sản phẩm dịch vụ với ID: {ServiceProductID}", id);
            var serviceProduct = await _context.ServiceProducts.FindAsync(id);
            if (serviceProduct == null)
            {
                _logger.LogWarning("Không tìm thấy sản phẩm dịch vụ để xóa với ID: {ServiceProductID}", id);
                return false;
            }

            _context.ServiceProducts.Remove(serviceProduct);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa sản phẩm dịch vụ với ID: {ServiceProductID}", id);
            return true;
        }

        // --- HÀM HỖ TRỢ: KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (PUBLIC CHO INTERFACE) ---
        public async Task<bool> ServiceProductExistsAsync(string id)
        {
            return await _context.ServiceProducts.AnyAsync(e => e.ServiceProductID == id);
        }

        public async Task<bool> DecalServiceExistsAsync(string id)
        {
            return await _context.DecalServices.AnyAsync(e => e.ServiceID == id);
        }

        public async Task<bool> ProductExistsAsync(string id)
        {
            return await _context.Products.AnyAsync(e => e.ProductID == id);
        }
    }
}
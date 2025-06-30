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
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ApplicationDbContext context, IMapper mapper, ILogger<ProductService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductDto>> GetProductsAsync()
        {
            _logger.LogInformation("Lấy danh sách sản phẩm.");
            // Product giờ sẽ Include ServiceVehicleModelProducts thay vì ServiceProducts
            var products = await _context.Products
                                        .Include(p => p.ServiceVehicleModelProducts!) // <-- MỚI: Thêm include cho bảng mới
                                            .ThenInclude(svmp => svmp.DecalService) // Include DecalService
                                        .Include(p => p.ServiceVehicleModelProducts!) // Include lại để ThenInclude VehicleModel
                                            .ThenInclude(svmp => svmp.VehicleModel!) // Include VehicleModel
                                                .ThenInclude(vm => vm.VehicleBrand) // Include VehicleBrand
                                        .ToListAsync();
            var productDtos = _mapper.Map<List<ProductDto>>(products);
            return productDtos;
        }

        public async Task<ProductDto?> GetProductByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy sản phẩm với ID: {ProductID}", id);
            var product = await _context.Products
                                        .Include(p => p.ServiceVehicleModelProducts!) // <-- MỚI: Thêm include cho bảng mới
                                            .ThenInclude(svmp => svmp.DecalService)
                                        .Include(p => p.ServiceVehicleModelProducts!)
                                            .ThenInclude(svmp => svmp.VehicleModel!)
                                                .ThenInclude(vm => vm.VehicleBrand)
                                        .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null)
            {
                _logger.LogWarning("Không tìm thấy sản phẩm với ID: {ProductID}", id);
                return null;
            }

            var productDto = _mapper.Map<ProductDto>(product);
            _logger.LogInformation("Đã trả về sản phẩm với ID: {ProductID}", id);
            return productDto;
        }

        public async Task<ProductDto> CreateProductAsync(Product product)
        {
            _logger.LogInformation("Yêu cầu tạo sản phẩm mới: {ProductName}", product.ProductName);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Tải lại Navigation Properties để có dữ liệu cho DTO
            // Product không có nhiều Navigation Properties phức tạp để load lại ở đây
            // Nếu có thì cần include các ServiceVehicleModelProducts để load Product
            // await _context.Entry(product).Collection(p => p.ServiceVehicleModelProducts!).LoadAsync(); // Load lại collection

            var productDto = _mapper.Map<ProductDto>(product);
            _logger.LogInformation("Đã tạo sản phẩm mới với ID: {ProductID}", product.ProductID);
            return productDto;
        }

        public async Task<bool> UpdateProductAsync(string id, Product product)
        {
            _logger.LogInformation("Yêu cầu cập nhật sản phẩm với ID: {ProductID}", id);

            if (id != product.ProductID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với ProductID trong body ({ProductIDBody})", id, product.ProductID);
                return false;
            }

            if (!await ProductExistsAsync(id))
            {
                _logger.LogWarning("Không tìm thấy sản phẩm để cập nhật với ID: {ProductID}", id);
                return false;
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật sản phẩm với ID: {ProductID}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật sản phẩm với ID: {ProductID}", id);
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa sản phẩm với ID: {ProductID}", id);
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Không tìm thấy sản phẩm để xóa với ID: {ProductID}", id);
                return false;
            }

            // Kiểm tra các mối quan hệ trước khi xóa Product
            if (await _context.OrderDetails.AnyAsync(od => od.DecalService!.ServiceVehicleModelProducts!.Any(scm => scm.ProductID == id)) || // Kiểm tra OrderDetail qua ServiceVehicleModelProduct
                await _context.ServiceVehicleModelProducts.AnyAsync(scm => scm.ProductID == id)) // <-- MỚI: Kiểm tra ServiceVehicleModelProduct
            {
                _logger.LogWarning("Không thể xóa sản phẩm {ProductID} vì đang được sử dụng trong chi tiết đơn hàng hoặc liên kết dịch vụ/mẫu xe.", id);
                throw new InvalidOperationException("Không thể xóa sản phẩm này vì đang được sử dụng trong chi tiết đơn hàng hoặc liên kết dịch vụ/mẫu xe.");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa sản phẩm với ID: {ProductID}", id);
            return true;
        }

        public async Task<bool> ProductExistsAsync(string id)
        {
            return await _context.Products.AnyAsync(e => e.ProductID == id);
        }
    }
}
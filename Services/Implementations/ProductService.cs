using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces; // Cần cho ICategoryService
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
        private readonly ICategoryService _categoryService; // <-- MỚI: Khai báo ICategoryService

        public ProductService(ApplicationDbContext context, IMapper mapper, ILogger<ProductService> logger, ICategoryService categoryService) // <-- TIÊM ICategoryService vào constructor
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _categoryService = categoryService; // Gán ICategoryService
        }

        // Lấy danh sách sản phẩm
        public async Task<IEnumerable<ProductDto>> GetProductsAsync()
        {
            _logger.LogInformation("Lấy danh sách sản phẩm.");
            // Thêm .Include(p => p.Category) để tải thông tin Category
            var products = await _context.Products
                                        .Include(p => p.Category) // <-- THÊM DÒNG NÀY
                                        .ToListAsync();
            var productDtos = _mapper.Map<List<ProductDto>>(products);
            return productDtos;
        }

        // Lấy chi tiết sản phẩm theo ID
        public async Task<ProductDto?> GetProductByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy sản phẩm với ID: {ProductID}", id);
            // Thêm .Include(p => p.Category) vào đây
            var product = await _context.Products
                                        .Include(p => p.Category) // <-- THÊM DÒNG NÀY
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

        // Tạo sản phẩm mới
        public async Task<ProductDto> CreateProductAsync(Product product)
        {
            _logger.LogInformation("Yêu cầu tạo sản phẩm mới: {ProductName}", product.ProductName);

            // MỚI: Kiểm tra CategoryID có tồn tại không
            if (!string.IsNullOrEmpty(product.CategoryID) && !await _categoryService.CategoryExistsAsync(product.CategoryID))
            {
                throw new ArgumentException("CategoryID không tồn tại.");
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Tải lại thông tin Category để AutoMapper có thể ánh xạ CategoryName
            await _context.Entry(product).Reference(p => p.Category).LoadAsync(); // <-- THÊM DÒNG NÀY

            var productDto = _mapper.Map<ProductDto>(product);
            _logger.LogInformation("Đã tạo sản phẩm mới với ID: {ProductID}", product.ProductID);
            return productDto;
        }

        // Cập nhật sản phẩm
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

            // MỚI: Kiểm tra CategoryID có tồn tại không khi cập nhật
            if (!string.IsNullOrEmpty(product.CategoryID) && !await _categoryService.CategoryExistsAsync(product.CategoryID))
            {
                throw new ArgumentException("CategoryID không tồn tại.");
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

        // Xóa sản phẩm
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
            // Đảm bảo các kiểm tra này khớp với cấu trúc mới của bạn
            if (await _context.ServiceVehicleModelProducts.AnyAsync(scm => scm.ProductID == id)) // Kiểm tra ServiceVehicleModelProducts
            {
                _logger.LogWarning("Không thể xóa sản phẩm {ProductID} vì đang được sử dụng trong liên kết dịch vụ/mẫu xe.", id);
                throw new InvalidOperationException("Không thể xóa sản phẩm này vì đang được sử dụng trong liên kết dịch vụ/mẫu xe.");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa sản phẩm với ID: {ProductID}", id);
            return true;
        }

        // Hàm hỗ trợ: Kiểm tra sự tồn tại của Product
        public async Task<bool> ProductExistsAsync(string id)
        {
            return await _context.Products.AnyAsync(e => e.ProductID == id);
        }
    }
}
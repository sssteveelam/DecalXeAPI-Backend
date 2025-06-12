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
using System; // Để sử dụng ArgumentException

namespace DecalXeAPI.Services.Implementations
{
    public class OrderCompletionImageService : IOrderCompletionImageService // <-- Kế thừa từ IOrderCompletionImageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderCompletionImageService> _logger;

        public OrderCompletionImageService(ApplicationDbContext context, IMapper mapper, ILogger<OrderCompletionImageService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<OrderCompletionImageDto>> GetOrderCompletionImagesAsync()
        {
            _logger.LogInformation("Lấy danh sách hình ảnh hoàn tất đơn hàng.");
            var images = await _context.OrderCompletionImages.Include(oci => oci.Order).ToListAsync();
            var imageDtos = _mapper.Map<List<OrderCompletionImageDto>>(images);
            return imageDtos;
        }

        public async Task<OrderCompletionImageDto?> GetOrderCompletionImageByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy hình ảnh hoàn tất đơn hàng với ID: {ImageID}", id);
            var image = await _context.OrderCompletionImages.Include(oci => oci.Order).FirstOrDefaultAsync(oci => oci.ImageID == id);

            if (image == null)
            {
                _logger.LogWarning("Không tìm thấy hình ảnh hoàn tất đơn hàng với ID: {ImageID}", id);
                return null;
            }

            var imageDto = _mapper.Map<OrderCompletionImageDto>(image);
            _logger.LogInformation("Đã trả về hình ảnh hoàn tất đơn hàng với ID: {ImageID}", id);
            return imageDto;
        }

        public async Task<OrderCompletionImageDto> CreateOrderCompletionImageAsync(OrderCompletionImage image)
        {
            _logger.LogInformation("Yêu cầu tạo hình ảnh hoàn tất đơn hàng mới cho OrderID: {OrderID}", image.OrderID);

            // Kiểm tra OrderID có tồn tại không
            if (!string.IsNullOrEmpty(image.OrderID) && !await OrderExistsAsync(image.OrderID))
            {
                _logger.LogWarning("OrderID không tồn tại khi tạo OrderCompletionImage: {OrderID}", image.OrderID);
                throw new ArgumentException("OrderID không tồn tại.");
            }

            _context.OrderCompletionImages.Add(image);
            await _context.SaveChangesAsync();

            // Tải lại thông tin Order để AutoMapper có thể ánh xạ OrderStatus
            await _context.Entry(image).Reference(oci => oci.Order).LoadAsync();

            var imageDto = _mapper.Map<OrderCompletionImageDto>(image);
            _logger.LogInformation("Đã tạo hình ảnh hoàn tất đơn hàng mới với ID: {ImageID}", image.ImageID);
            return imageDto;
        }

        public async Task<bool> UpdateOrderCompletionImageAsync(string id, OrderCompletionImage image)
        {
            _logger.LogInformation("Yêu cầu cập nhật hình ảnh hoàn tất đơn hàng với ID: {ImageID}", id);

            if (id != image.ImageID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với ImageID trong body ({ImageIDBody})", id, image.ImageID);
                return false;
            }

            if (!await OrderCompletionImageExistsAsync(id))
            {
                _logger.LogWarning("Không tìm thấy hình ảnh hoàn tất đơn hàng để cập nhật với ID: {ImageID}", id);
                return false;
            }

            // Kiểm tra OrderID
            if (!string.IsNullOrEmpty(image.OrderID) && !await OrderExistsAsync(image.OrderID))
            {
                _logger.LogWarning("OrderID không tồn tại khi cập nhật OrderCompletionImage: {OrderID}", image.OrderID);
                throw new ArgumentException("OrderID không tồn tại.");
            }

            _context.Entry(image).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật hình ảnh hoàn tất đơn hàng với ID: {ImageID}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật hình ảnh hoàn tất đơn hàng với ID: {ImageID}", id);
                throw;
            }
        }

        public async Task<bool> DeleteOrderCompletionImageAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa hình ảnh hoàn tất đơn hàng với ID: {ImageID}", id);
            var image = await _context.OrderCompletionImages.FindAsync(id);
            if (image == null)
            {
                _logger.LogWarning("Không tìm thấy hình ảnh hoàn tất đơn hàng để xóa với ID: {ImageID}", id);
                return false;
            }

            _context.OrderCompletionImages.Remove(image);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa hình ảnh hoàn tất đơn hàng với ID: {ImageID}", id);
            return true;
        }

        // --- HÀM HỖ TRỢ: KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (PUBLIC CHO INTERFACE) ---
        public async Task<bool> OrderCompletionImageExistsAsync(string id)
        {
            return await _context.OrderCompletionImages.AnyAsync(e => e.ImageID == id);
        }

        public async Task<bool> OrderExistsAsync(string id)
        {
            return await _context.Orders.AnyAsync(e => e.OrderID == id);
        }
    }
}
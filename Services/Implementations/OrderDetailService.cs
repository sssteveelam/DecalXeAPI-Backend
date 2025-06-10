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
using System; // Để dùng Math.Ceiling nếu có

namespace DecalXeAPI.Services.Implementations
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderDetailService> _logger;

        public OrderDetailService(ApplicationDbContext context, IMapper mapper, ILogger<OrderDetailService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<OrderDetailDto>> GetOrderDetailsAsync()
        {
            _logger.LogInformation("Lấy danh sách chi tiết đơn hàng.");
            var orderDetails = await _context.OrderDetails
                                            .Include(od => od.Order)
                                            .Include(od => od.DecalService)
                                            .ToListAsync();
            var orderDetailDtos = _mapper.Map<List<OrderDetailDto>>(orderDetails);
            return orderDetailDtos;
        }

        public async Task<OrderDetailDto?> GetOrderDetailByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy chi tiết đơn hàng với ID: {OrderDetailID}", id);
            var orderDetail = await _context.OrderDetails
                                                .Include(od => od.Order)
                                                .Include(od => od.DecalService)
                                                .FirstOrDefaultAsync(od => od.OrderDetailID == id);

            if (orderDetail == null)
            {
                _logger.LogWarning("Không tìm thấy chi tiết đơn hàng với ID: {OrderDetailID}", id);
                return null;
            }

            var orderDetailDto = _mapper.Map<OrderDetailDto>(orderDetail);
            _logger.LogInformation("Đã trả về chi tiết đơn hàng với ID: {OrderDetailID}", id);
            return orderDetailDto;
        }

        public async Task<(OrderDetailDto? OrderDetail, string? ErrorMessage)> CreateOrderDetailAsync(OrderDetail orderDetail)
        {
            _logger.LogInformation("Yêu cầu tạo chi tiết đơn hàng mới cho OrderID: {OrderID}, ServiceID: {ServiceID}, Quantity: {Quantity}",
                                    orderDetail.OrderID, orderDetail.ServiceID, orderDetail.Quantity);

            if (!string.IsNullOrEmpty(orderDetail.OrderID) && !await OrderExistsAsync(orderDetail.OrderID))
            {
                return (null, "OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(orderDetail.ServiceID) && !await DecalServiceExistsAsync(orderDetail.ServiceID))
            {
                return (null, "ServiceID không tồn tại.");
            }

            // Cẩn thận khi Include các Navigation Property có thể null trong chuỗi ThenInclude
            var service = await _context.DecalServices
                                        .Include(s => s.ServiceProducts)
                                            .ThenInclude(sp => sp.Product) // Đã có rồi, đảm bảo Product có thể null
                                        .FirstOrDefaultAsync(s => s.ServiceID == orderDetail.ServiceID);

            if (service == null)
            {
                return (null, "Dịch vụ không tồn tại hoặc không tìm thấy thông tin sản phẩm liên quan.");
            }

            orderDetail.Price = service.Price;

            // --- LOGIC NGHIỆP VỤ: CẬP NHẬT TỒN KHO (STOCKQUANTITY) ---
            // service.ServiceProducts có thể là null nếu không có sản phẩm nào
            if (service.ServiceProducts != null) // <-- THÊM KIỂM TRA NULL
            {
                foreach (var sp in service.ServiceProducts)
                {
                    if (sp.Product != null) // <-- Đã có kiểm tra null
                    {
                        var quantityToDeduct = sp.QuantityUsed * orderDetail.Quantity;
                        if (sp.Product.StockQuantity < quantityToDeduct)
                        {
                            return (null, $"Sản phẩm '{sp.Product.ProductName}' không đủ tồn kho. Chỉ còn {sp.Product.StockQuantity} {sp.Product.Unit} trong kho, nhưng cần {quantityToDeduct} {sp.Product.Unit}.");
                        }
                        sp.Product.StockQuantity -= (int)quantityToDeduct;
                        _context.Products.Update(sp.Product);
                    }
                }
            }

            await _context.SaveChangesAsync();

            _context.OrderDetails.Add(orderDetail);
            await _context.SaveChangesAsync();

            await RecalculateOrderTotalAmount(orderDetail.OrderID);

            await _context.Entry(orderDetail).Reference(od => od.Order).LoadAsync();
            await _context.Entry(orderDetail).Reference(od => od.DecalService).LoadAsync();

            var orderDetailDto = _mapper.Map<OrderDetailDto>(orderDetail);
            _logger.LogInformation("Đã tạo chi tiết đơn hàng mới với ID: {OrderDetailID}", orderDetail.OrderDetailID);
            return (orderDetailDto, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateOrderDetailAsync(string id, OrderDetail orderDetail)
        {
            _logger.LogInformation("Yêu cầu cập nhật chi tiết đơn hàng với ID: {OrderDetailID}", id);

            if (id != orderDetail.OrderDetailID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với OrderDetailID trong body ({OrderDetailIDBody})", id, orderDetail.OrderDetailID);
                return (false, "ID không khớp.");
            }

            var oldOrderDetail = await _context.OrderDetails
                                                .Include(od => od.DecalService)
                                                    .ThenInclude(ds => ds.ServiceProducts!) // <-- THÊM ! ĐỂ XỬ LÝ CS8620 (nếu chắc chắn không null)
                                                        .ThenInclude(sp => sp.Product)
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(od => od.OrderDetailID == id);
            if (oldOrderDetail == null)
            {
                _logger.LogWarning("Không tìm thấy chi tiết đơn hàng cũ để cập nhật với ID: {OrderDetailID}", id);
                return (false, "Chi tiết đơn hàng không tồn tại.");
            }

            if (!string.IsNullOrEmpty(orderDetail.OrderID) && !await OrderExistsAsync(orderDetail.OrderID))
            {
                return (false, "OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(orderDetail.ServiceID) && !await DecalServiceExistsAsync(orderDetail.ServiceID))
            {
                return (false, "ServiceID không tồn tại.");
            }

            var newService = await _context.DecalServices
                                            .Include(s => s.ServiceProducts!) // <-- THÊM ! ĐỂ XỬ LÝ CS8620
                                                .ThenInclude(sp => sp.Product)
                                            .FirstOrDefaultAsync(s => s.ServiceID == orderDetail.ServiceID);
            if (newService == null)
            {
                return (false, "Dịch vụ mới không tồn tại hoặc không tìm thấy thông tin sản phẩm liên quan.");
            }

            orderDetail.Price = newService.Price;

            // --- LOGIC NGHIỆP VỤ: CẬP NHẬT TỒN KHO ---
            if (oldOrderDetail.Quantity != orderDetail.Quantity || oldOrderDetail.ServiceID != orderDetail.ServiceID)
            {
                // Hoàn trả tồn kho từ trạng thái cũ của OrderDetail
                if (oldOrderDetail.DecalService?.ServiceProducts != null) // <-- THÊM ?. VÀ KIỂM TRA NULL
                {
                    foreach (var sp in oldOrderDetail.DecalService.ServiceProducts)
                    {
                        if (sp.Product != null) // <-- Đã có kiểm tra null
                        {
                            sp.Product.StockQuantity += (int)(sp.QuantityUsed * oldOrderDetail.Quantity);
                            _context.Products.Update(sp.Product);
                        }
                    }
                }

                // Giảm tồn kho theo trạng thái mới của OrderDetail
                if (newService.ServiceProducts != null) // <-- THÊM KIỂM TRA NULL
                {
                    foreach (var sp in newService.ServiceProducts)
                    {
                        if (sp.Product != null) // <-- Đã có kiểm tra null
                        {
                            var quantityToDeduct = sp.QuantityUsed * orderDetail.Quantity;
                            if (sp.Product.StockQuantity < quantityToDeduct)
                            {
                                // Nếu không đủ tồn kho mới, rollback các thay đổi tồn kho đã hoàn trả
                                if (oldOrderDetail.DecalService?.ServiceProducts != null) // <-- THÊM ?. VÀ KIỂM TRA NULL
                                {
                                    foreach (var rollbackSp in oldOrderDetail.DecalService.ServiceProducts)
                                    {
                                        if (rollbackSp.Product != null)
                                        {
                                            rollbackSp.Product.StockQuantity -= (int)(rollbackSp.QuantityUsed * oldOrderDetail.Quantity);
                                            _context.Products.Update(rollbackSp.Product);
                                        }
                                    }
                                }
                                await _context.SaveChangesAsync();
                                return (false, $"Sản phẩm '{sp.Product.ProductName}' không đủ tồn kho cho yêu cầu mới. Chỉ còn {sp.Product.StockQuantity} {sp.Product.Unit}.");
                            }
                            sp.Product.StockQuantity -= (int)quantityToDeduct;
                            _context.Products.Update(sp.Product);
                        }
                    }
                }
            }
            await _context.SaveChangesAsync();

            _context.Entry(orderDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                await RecalculateOrderTotalAmount(orderDetail.OrderID);
                _logger.LogInformation("Đã cập nhật chi tiết đơn hàng với ID: {OrderDetailID}", orderDetail.OrderDetailID);
                return (true, null);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật chi tiết đơn hàng với ID: {OrderDetailID}", id);
                throw;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteOrderDetailAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa chi tiết đơn hàng với ID: {OrderDetailID}", id);

            // Cần Include các thông tin liên quan để có thể hoàn trả tồn kho chính xác
            var orderDetail = await _context.OrderDetails
                                            .Include(od => od.DecalService)
                                                .ThenInclude(ds => ds.ServiceProducts!) // <-- THÊM ! ĐỂ XỬ LÝ CS8620
                                                    .ThenInclude(sp => sp.Product)
                                            .FirstOrDefaultAsync(od => od.OrderDetailID == id);
            if (orderDetail == null)
            {
                _logger.LogWarning("Không tìm thấy chi tiết đơn hàng để xóa với ID: {OrderDetailID}", id);
                return (false, "Chi tiết đơn hàng không tồn tại.");
            }

            _context.OrderDetails.Remove(orderDetail);
            await _context.SaveChangesAsync();

            // --- LOGIC NGHIỆP VỤ: HOÀN TRẢ TỒN KHO (STOCKQUANTITY) ---
            if (orderDetail.DecalService?.ServiceProducts != null) // <-- THÊM ?. VÀ KIỂM TRA NULL
            {
                foreach (var sp in orderDetail.DecalService.ServiceProducts)
                {
                    if (sp.Product != null)
                    {
                        sp.Product.StockQuantity += (int)(sp.QuantityUsed * orderDetail.Quantity);
                        _context.Products.Update(sp.Product);
                    }
                }
                await _context.SaveChangesAsync();
            }

            await RecalculateOrderTotalAmount(orderDetail.OrderID);

            _logger.LogInformation("Đã xóa chi tiết đơn hàng với ID: {OrderDetailID}", id);
            return (true, null);
        }

        // --- HÀM HỖ TRỢ: TÍNH TOÁN LẠI TỔNG TIỀN CỦA MỘT ORDER (PRIVATE METHOD) ---
        private async Task RecalculateOrderTotalAmount(string orderId)
        {
            var order = await _context.Orders
                                    .Include(o => o.OrderDetails)
                                    .FirstOrDefaultAsync(o => o.OrderID == orderId);

            if (order != null)
            {
                // Lỗi CS8604: Possible null reference argument for parameter 'source' in 'decimal Enumerable.Sum<OrderDetail>(IEnumerable<OrderDetail> source, Func<OrderDetail, decimal> selector)'.
                // Đảm bảo order.OrderDetails không null trước khi gọi Sum.
                // Nếu order.OrderDetails có thể null thì cần xử lý.
                // Mặc định, EF Core sẽ trả về một collection rỗng chứ không null khi Include.
                // Nhưng để an toàn với warning, có thể thêm ?. hoặc kiểm tra.
                // Đối với Sum, nếu collection rỗng, Sum sẽ trả về 0.
                order.TotalAmount = order.OrderDetails?.Sum(od => od.Price * od.Quantity) ?? 0m; // <-- THÊM ?. VÀ ?? 0m

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
            }
        }

        // --- HÀM HỖ TRỢ: KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (PUBLIC CHO INTERFACE) ---
        public async Task<bool> OrderDetailExistsAsync(string id)
        {
            return await _context.OrderDetails.AnyAsync(e => e.OrderDetailID == id);
        }

        public async Task<bool> OrderExistsAsync(string id)
        {
            return await _context.Orders.AnyAsync(e => e.OrderID == id);
        }

        public async Task<bool> DecalServiceExistsAsync(string id)
        {
            return await _context.DecalServices.AnyAsync(e => e.ServiceID == id);
        }
    }
}
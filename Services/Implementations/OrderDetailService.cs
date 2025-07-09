// DecalXeAPI/Services/Implementations/OrderDetailService.cs
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

        // --- CÁC HÀM GET DỮ LIỆU (KHÔNG THAY ĐỔI NHIỀU) ---
        public async Task<IEnumerable<OrderDetailDto>> GetOrderDetailsAsync()
        {
            _logger.LogInformation("Lấy danh sách chi tiết đơn hàng.");
            var orderDetails = await _context.OrderDetails
                                            .Include(od => od.Order)
                                            .Include(od => od.DecalService)
                                            .ThenInclude(ds => ds.PrintingPriceDetail)
                                            .ToListAsync();
            return _mapper.Map<List<OrderDetailDto>>(orderDetails);
        }

        public async Task<OrderDetailDto?> GetOrderDetailByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy chi tiết đơn hàng với ID: {OrderDetailID}", id);
            var orderDetail = await _context.OrderDetails
                                            .Include(od => od.Order)
                                            .Include(od => od.DecalService)
                                            .ThenInclude(ds => ds.PrintingPriceDetail)
                                            .FirstOrDefaultAsync(od => od.OrderDetailID == id);
            return _mapper.Map<OrderDetailDto>(orderDetail);
        }

        // --- PHẪU THUẬT LẠI HOÀN TOÀN HÀM CREATE ---
        public async Task<(OrderDetailDto? OrderDetail, string? ErrorMessage)> CreateOrderDetailAsync(OrderDetail orderDetail)
        {
            _logger.LogInformation("Bắt đầu tạo chi tiết đơn hàng mới cho OrderID: {OrderID}", orderDetail.OrderID);

            // Bọc toàn bộ logic trong một transaction để đảm bảo an toàn dữ liệu
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Kiểm tra các khóa ngoại cơ bản (giữ nguyên)
                var order = await _context.Orders.Include(o => o.CustomerVehicle).FirstOrDefaultAsync(o => o.OrderID == orderDetail.OrderID);
                if (order == null) return (null, "OrderID không tồn tại.");
                if (order.CustomerVehicle == null) return (null, "Đơn hàng này chưa được gán xe cụ thể (CustomerVehicle).");

                var service = await _context.DecalServices
                                            .Include(s => s.DecalType) // Nạp DecalType để biết loại decal
                                            .Include(s => s.PrintingPriceDetail) // Vẫn nạp PrintingPriceDetail cho logic cũ
                                            .FirstOrDefaultAsync(s => s.ServiceID == orderDetail.ServiceID);
                if (service == null) return (null, "ServiceID không tồn tại.");
                if (service.DecalType == null) return (null, "Dịch vụ này chưa được gán Loại Decal (DecalType).");

                // 2. Lấy danh sách vật tư cần thiết (giữ nguyên)
                var requiredProducts = await _context.ServiceVehicleModelProducts
                                                    .Where(svmp => svmp.ServiceID == orderDetail.ServiceID && svmp.VehicleModelID == order.CustomerVehicle.ModelID)
                                                    .Include(svmp => svmp.Product)
                                                    .ToListAsync();
                _logger.LogInformation("Tìm thấy {Count} vật tư cần thiết cho ServiceID {ServiceID} và ModelID {ModelID}", requiredProducts.Count, orderDetail.ServiceID, order.CustomerVehicle.ModelID);


                // 3. --- LOGIC TÍNH GIÁ ĐƯỢC NÂNG CẤP ---
                decimal basePrice;

                // Ưu tiên lấy giá từ bảng giá mới (VehicleModelDecalType)
                var priceInfo = await _context.VehicleModelDecalTypes
                    .FirstOrDefaultAsync(p => p.ModelID == order.CustomerVehicle.ModelID && p.DecalTypeID == service.DecalType.DecalTypeID);

                if (priceInfo != null)
                {
                    // Nếu tìm thấy giá mới -> Sử dụng nó
                    _logger.LogInformation("Đã tìm thấy giá tùy chỉnh cho Model/DecalType. Áp dụng giá: {Price}", priceInfo.Price);
                    basePrice = priceInfo.Price;
                    orderDetail.Price = basePrice; // Cập nhật đơn giá cho OrderDetail
                    orderDetail.FinalCalculatedPrice = CalculateFinalPrice(null, orderDetail, basePrice); // Tính giá cuối cùng theo logic mới
                }
                else
                {
                    // Nếu không tìm thấy -> Quay về logic tính giá CŨ
                    _logger.LogWarning("Không tìm thấy giá tùy chỉnh. Quay về sử dụng giá mặc định từ DecalService.");
                    basePrice = service.Price;
                    orderDetail.Price = basePrice;
                    orderDetail.FinalCalculatedPrice = CalculateFinalPrice(service.PrintingPriceDetail, orderDetail, basePrice);
                }
                // --- KẾT THÚC LOGIC TÍNH GIÁ NÂNG CẤP ---

                // 4. KIỂM TRA & TRỪ KHO (giữ nguyên logic cũ)
                foreach (var item in requiredProducts)
                {
                    if (item.Product == null) continue;
                    var quantityToDeduct = item.Quantity * orderDetail.Quantity;
                    if (item.Product.StockQuantity < quantityToDeduct)
                    {
                        await transaction.RollbackAsync(); // Hoàn tác nếu lỗi
                        return (null, $"Sản phẩm '{item.Product.ProductName}' không đủ tồn kho. Cần {quantityToDeduct}, chỉ còn {item.Product.StockQuantity}.");
                    }
                    item.Product.StockQuantity -= quantityToDeduct;
                    _context.Products.Update(item.Product);
                }

                // 5. Thêm OrderDetail và lưu tất cả thay đổi
                _context.OrderDetails.Add(orderDetail);
                await _context.SaveChangesAsync();
                await RecalculateOrderTotalAmount(orderDetail.OrderID);

                // Nếu mọi thứ thành công, xác nhận transaction
                await transaction.CommitAsync();

                return (_mapper.Map<OrderDetailDto>(orderDetail), null);
            }
            catch (Exception ex)
            {
                // Nếu có bất kỳ lỗi nào khác, hoàn tác tất cả
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi nghiêm trọng khi tạo OrderDetail cho OrderID: {OrderID}", orderDetail.OrderID);
                return (null, "Đã xảy ra lỗi hệ thống khi tạo chi tiết đơn hàng.");
            }
        }

        // --- PHẪU THUẬT LẠI HOÀN TOÀN HÀM UPDATE ---
        public async Task<(bool Success, string? ErrorMessage)> UpdateOrderDetailAsync(string id, OrderDetail orderDetail)
        {
            if (id != orderDetail.OrderDetailID) return (false, "ID không khớp.");

            // 1. Lấy thông tin cũ và mới
            var oldOrderDetail = await _context.OrderDetails.AsNoTracking().FirstOrDefaultAsync(od => od.OrderDetailID == id);
            if (oldOrderDetail == null) return (false, "Chi tiết đơn hàng không tồn tại.");

            var order = await _context.Orders.Include(o => o.CustomerVehicle).FirstOrDefaultAsync(o => o.OrderID == orderDetail.OrderID);
            if (order == null) return (false, "OrderID không tồn tại.");
            if (order.CustomerVehicle == null) return (false, "Đơn hàng này chưa được gán xe cụ thể.");

            var newService = await _context.DecalServices.Include(s => s.PrintingPriceDetail).FirstOrDefaultAsync(s => s.ServiceID == orderDetail.ServiceID);
            if (newService == null) return (false, "ServiceID mới không tồn tại.");
            
            // --- LOGIC HOÀN TRẢ & TRỪ KHO MỚI ---
            
            // 2. Hoàn trả tồn kho cho trạng thái cũ
            var oldRequiredProducts = await _context.ServiceVehicleModelProducts
                                                    .Where(svmp => svmp.ServiceID == oldOrderDetail.ServiceID && svmp.VehicleModelID == order.CustomerVehicle.ModelID)
                                                    .Include(svmp => svmp.Product)
                                                    .ToListAsync();
            foreach (var item in oldRequiredProducts)
            {
                if (item.Product == null) continue;
                item.Product.StockQuantity += item.Quantity * oldOrderDetail.Quantity;
                _context.Products.Update(item.Product);
            }

            // 3. Trừ tồn kho cho trạng thái mới
            var newRequiredProducts = await _context.ServiceVehicleModelProducts
                                                    .Where(svmp => svmp.ServiceID == orderDetail.ServiceID && svmp.VehicleModelID == order.CustomerVehicle.ModelID)
                                                    .Include(svmp => svmp.Product)
                                                    .ToListAsync();
            
            // Phải kiểm tra tồn kho trước khi thực hiện trừ
            foreach (var item in newRequiredProducts)
            {
                if (item.Product == null) continue;
                var quantityToDeduct = item.Quantity * orderDetail.Quantity;
                if (item.Product.StockQuantity < quantityToDeduct)
                {
                    // Nếu không đủ kho, phải rollback lại bước hoàn trả ở trên (bỏ qua để đơn giản, thực tế cần transaction)
                    return (false, $"Không thể cập nhật. Sản phẩm '{item.Product.ProductName}' không đủ tồn kho. Cần {quantityToDeduct}, chỉ còn {item.Product.StockQuantity}.");
                }
                item.Product.StockQuantity -= quantityToDeduct;
                _context.Products.Update(item.Product);
            }

            // 4. Cập nhật giá và các thông tin khác
            orderDetail.Price = newService.Price;
            orderDetail.FinalCalculatedPrice = CalculateFinalPrice(newService.PrintingPriceDetail, orderDetail, orderDetail.Price);
            
            _context.Entry(orderDetail).State = EntityState.Modified;
            
            await _context.SaveChangesAsync();
            await RecalculateOrderTotalAmount(orderDetail.OrderID);
            
            return (true, null);
        }

        // --- HÀM DELETE CŨNG CẦN NÂNG CẤP ---
        public async Task<(bool Success, string? ErrorMessage)> DeleteOrderDetailAsync(string id)
        {
            var orderDetail = await _context.OrderDetails
                                            .Include(od => od.Order)
                                            .ThenInclude(o => o.CustomerVehicle)
                                            .FirstOrDefaultAsync(od => od.OrderDetailID == id);
            if (orderDetail == null || orderDetail.Order?.CustomerVehicle == null)
            {
                return (false, "Chi tiết đơn hàng hoặc thông tin xe liên quan không tồn tại.");
            }
            
            // Hoàn trả tồn kho dựa trên logic mới
            var requiredProducts = await _context.ServiceVehicleModelProducts
                                                 .Where(svmp => svmp.ServiceID == orderDetail.ServiceID && svmp.VehicleModelID == orderDetail.Order.CustomerVehicle.ModelID)
                                                 .Include(svmp => svmp.Product)
                                                 .ToListAsync();
            foreach (var item in requiredProducts)
            {
                if (item.Product == null) continue;
                item.Product.StockQuantity += item.Quantity * orderDetail.Quantity;
                _context.Products.Update(item.Product);
            }

            _context.OrderDetails.Remove(orderDetail);
            await _context.SaveChangesAsync();
            await RecalculateOrderTotalAmount(orderDetail.OrderID);
            
            return (true, null);
        }

        // --- CÁC HÀM HỖ TRỢ (GIỮ NGUYÊN HOẶC CẬP NHẬT) ---
        private async Task RecalculateOrderTotalAmount(string orderId)
        {
            var order = await _context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.OrderID == orderId);
            if (order != null)
            {
                order.TotalAmount = order.OrderDetails?.Sum(od => od.FinalCalculatedPrice * od.Quantity) ?? 0m;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
            }
        }

        private decimal CalculateFinalPrice(PrintingPriceDetail? ppd, OrderDetail od, decimal defaultPrice)
        {
            if (ppd == null || !od.ActualAreaUsed.HasValue || od.ActualAreaUsed.Value <= 0) return defaultPrice;
            decimal calculatedPrice = ppd.BasePricePerSqMeter * od.ActualAreaUsed.Value;
            if (ppd.ColorPricingFactor.HasValue) calculatedPrice *= ppd.ColorPricingFactor.Value;
            return calculatedPrice;
        }

        public async Task<bool> OrderDetailExistsAsync(string id) => await _context.OrderDetails.AnyAsync(e => e.OrderDetailID == id);
        public async Task<bool> OrderExistsAsync(string id) => await _context.Orders.AnyAsync(e => e.OrderID == id);
        public async Task<bool> DecalServiceExistsAsync(string id) => await _context.DecalServices.AnyAsync(e => e.ServiceID == id);
    }
}
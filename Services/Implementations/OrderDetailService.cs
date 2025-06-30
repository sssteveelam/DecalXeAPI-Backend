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

        // Lấy danh sách chi tiết đơn hàng
        public async Task<IEnumerable<OrderDetailDto>> GetOrderDetailsAsync()
        {
            _logger.LogInformation("Lấy danh sách chi tiết đơn hàng.");
            var orderDetails = await _context.OrderDetails
                                            .Include(od => od.Order)
                                            .Include(od => od.DecalService)
                                                .ThenInclude(ds => ds.PrintingPriceDetail)
                                            .ToListAsync();
            var orderDetailDtos = _mapper.Map<List<OrderDetailDto>>(orderDetails);
            return orderDetailDtos;
        }

        // Lấy chi tiết đơn hàng theo ID
        public async Task<OrderDetailDto?> GetOrderDetailByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy chi tiết đơn hàng với ID: {OrderDetailID}", id);
            var orderDetail = await _context.OrderDetails
                                                .Include(od => od.Order)
                                                .Include(od => od.DecalService)
                                                    .ThenInclude(ds => ds.PrintingPriceDetail)
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

        // Tạo chi tiết đơn hàng mới (có logic tồn kho và cập nhật tổng tiền)
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

            // --- LOGIC NGHIỆP VỤ: LẤY THÔNG TIN DỊCH VỤ VÀ SẢN PHẨM ĐỂ TÍNH GIÁ VÀ CẬP NHẬT TỒN KHO ---
            var service = await _context.DecalServices
                                        .Include(s => s.PrintingPriceDetail)
                                        .Include(s => s.ServiceVehicleModelProducts!) // <-- ĐỔI TÊN TỪ ServiceProducts
                                            .ThenInclude(scm => scm.Product)
                                        .FirstOrDefaultAsync(s => s.ServiceID == orderDetail.ServiceID);

            if (service == null)
            {
                return (null, "Dịch vụ không tồn tại hoặc không tìm thấy thông tin sản phẩm liên quan.");
            }

            orderDetail.Price = service.Price;

            orderDetail.FinalCalculatedPrice = CalculateFinalPrice(service.PrintingPriceDetail, orderDetail, orderDetail.Price);


            // Lấy VehicleModelID từ Order liên quan để lọc ServiceCarModelProducts
            var order = await _context.Orders.Include(o => o.CustomerVehicle).FirstOrDefaultAsync(o => o.OrderID == orderDetail.OrderID);
            string? vehicleModelId = order?.CustomerVehicle?.VehicleModelID; // ĐỔI TÊN TỪ carModelId

            // --- LOGIC NGHIỆP VỤ: CẬP NHẬT TỒN KHO (STOCKQUANTITY) ---
            if (service.ServiceVehicleModelProducts != null)
            {
                foreach (var scm in service.ServiceVehicleModelProducts)
                {
                    if (scm.Product != null && scm.VehicleModelID == vehicleModelId) // MỚI: Chỉ trừ tồn kho nếu khớp với VehicleModel của Order
                    {
                        var quantityToDeduct = scm.QuantityUsed * orderDetail.Quantity;
                        if (scm.Product.StockQuantity < quantityToDeduct)
                        {
                            return (null, $"Sản phẩm '{scm.Product.ProductName}' không đủ tồn kho. Chỉ còn {scm.Product.StockQuantity} {scm.Product.Unit} trong kho, nhưng cần {quantityToDeduct} {scm.Product.Unit} cho mẫu xe {scm.VehicleModelID}.");
                        }
                        scm.Product.StockQuantity -= (int)quantityToDeduct;
                        _context.Products.Update(scm.Product);
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

        // Cập nhật chi tiết đơn hàng (có logic tồn kho và cập nhật tổng tiền)
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
                                                    .ThenInclude(ds => ds.ServiceVehicleModelProducts!) // <-- ĐỔI TÊN TỪ ServiceProducts
                                                        .ThenInclude(scm => scm.Product)
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(od => od.OrderDetailID == id); // <-- LỖI CS1061 CỦA TEntity CŨNG Ở ĐÂY

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
                                            .Include(s => s.PrintingPriceDetail)
                                            .Include(s => s.ServiceVehicleModelProducts!) // <-- ĐỔI TÊN TỪ ServiceProducts
                                                .ThenInclude(scm => scm.Product)
                                            .FirstOrDefaultAsync(s => s.ServiceID == orderDetail.ServiceID);
            if (newService == null)
            {
                return (false, "Dịch vụ mới không tồn tại hoặc không tìm thấy thông tin sản phẩm liên quan.");
            }

            orderDetail.Price = newService.Price;

            orderDetail.FinalCalculatedPrice = CalculateFinalPrice(newService.PrintingPriceDetail, orderDetail, orderDetail.Price);


            // Lấy VehicleModelID từ Order liên quan để lọc ServiceCarModelProducts
            var order = await _context.Orders.Include(o => o.CustomerVehicle).FirstOrDefaultAsync(o => o.OrderID == orderDetail.OrderID);
            string? vehicleModelId = order?.CustomerVehicle?.VehicleModelID;

            // --- LOGIC NGHIỆP VỤ: CẬP NHẬT TỒN KHO ---
            if (oldOrderDetail.Quantity != orderDetail.Quantity || oldOrderDetail.ServiceID != orderDetail.ServiceID)
            {
                // Hoàn trả tồn kho từ trạng thái cũ của OrderDetail
                if (oldOrderDetail.DecalService?.ServiceVehicleModelProducts != null) // <-- ĐỔI TÊN
                {
                    foreach (var scm in oldOrderDetail.DecalService.ServiceVehicleModelProducts)
                    {
                        if (scm.Product != null && scm.VehicleModelID == vehicleModelId) // MỚI: Chỉ hoàn trả nếu khớp với VehicleModel
                        {
                            scm.Product.StockQuantity += (int)(scm.QuantityUsed * oldOrderDetail.Quantity);
                            _context.Products.Update(scm.Product);
                        }
                    }
                }

                // Giảm tồn kho theo trạng thái mới của OrderDetail
                if (newService.ServiceVehicleModelProducts != null)
                {
                    foreach (var scm in newService.ServiceVehicleModelProducts)
                    {
                        if (scm.Product != null && scm.VehicleModelID == vehicleModelId) // MỚI: Chỉ trừ tồn kho nếu khớp với VehicleModel
                        {
                            var quantityToDeduct = scm.QuantityUsed * orderDetail.Quantity;
                            if (scm.Product.StockQuantity < quantityToDeduct)
                            {
                                // Nếu không đủ tồn kho mới, rollback các thay đổi tồn kho đã hoàn trả
                                if (oldOrderDetail.DecalService?.ServiceVehicleModelProducts != null)
                                {
                                    foreach (var rollbackScm in oldOrderDetail.DecalService.ServiceVehicleModelProducts)
                                    {
                                        if (rollbackScm.Product != null && rollbackScm.VehicleModelID == vehicleModelId)
                                        {
                                            rollbackScm.Product.StockQuantity -= (int)(rollbackScm.QuantityUsed * oldOrderDetail.Quantity);
                                            _context.Products.Update(rollbackScm.Product);
                                        }
                                    }
                                }
                                await _context.SaveChangesAsync();
                                return (false, $"Sản phẩm '{scm.Product.ProductName}' không đủ tồn kho cho yêu cầu mới. Chỉ còn {scm.Product.StockQuantity} {scm.Product.Unit} cho mẫu xe {scm.VehicleModelID}.");
                            }
                            scm.Product.StockQuantity -= (int)quantityToDeduct;
                            _context.Products.Update(scm.Product);
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

            var orderDetail = await _context.OrderDetails
                                            .Include(od => od.DecalService)
                                                .ThenInclude(ds => ds.ServiceVehicleModelProducts!) // <-- ĐỔI TÊN
                                                    .ThenInclude(scm => scm.Product)
                                            .FirstOrDefaultAsync(od => od.OrderDetailID == id);
            if (orderDetail == null)
            {
                _logger.LogWarning("Không tìm thấy chi tiết đơn hàng để xóa với ID: {OrderDetailID}", id);
                return (false, "Chi tiết đơn hàng không tồn tại.");
            }

            _context.OrderDetails.Remove(orderDetail);
            await _context.SaveChangesAsync();

            // Lấy VehicleModelID từ Order liên quan
            var order = await _context.Orders.Include(o => o.CustomerVehicle).FirstOrDefaultAsync(o => o.OrderID == orderDetail.OrderID);
            string? vehicleModelId = order?.CustomerVehicle?.VehicleModelID;

            if (orderDetail.DecalService?.ServiceVehicleModelProducts != null)
            {
                foreach (var scm in orderDetail.DecalService.ServiceVehicleModelProducts)
                {
                    if (scm.Product != null && scm.VehicleModelID == vehicleModelId)
                    {
                        scm.Product.StockQuantity += (int)(scm.QuantityUsed * orderDetail.Quantity);
                        _context.Products.Update(scm.Product);
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
                order.TotalAmount = order.OrderDetails?.Sum(od => od.FinalCalculatedPrice * od.Quantity) ?? 0m;

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
            }
        }

        // --- HÀM HỖ TRỢ: TÍNH TOÁN FINALCALCULATEDPRICE ---
        private decimal CalculateFinalPrice(PrintingPriceDetail? ppd, OrderDetail od, decimal defaultPrice)
        {
            if (ppd == null)
            {
                _logger.LogInformation("Không có PrintingPriceDetail cho ServiceID {ServiceID}. Sử dụng giá mặc định {DefaultPrice}.", od.ServiceID, defaultPrice);
                return defaultPrice;
            }

            decimal calculatedPrice = 0m;

            if (ppd.BasePricePerSqMeter > 0 && od.ActualAreaUsed.HasValue && od.ActualAreaUsed.Value > 0)
            {
                calculatedPrice = ppd.BasePricePerSqMeter * od.ActualAreaUsed.Value;

                if (ppd.ColorPricingFactor.HasValue && ppd.ColorPricingFactor.Value > 0)
                {
                    calculatedPrice *= ppd.ColorPricingFactor.Value;
                }

                if (ppd.MinArea.HasValue && od.ActualAreaUsed.Value < ppd.MinArea.Value)
                {
                    calculatedPrice = ppd.BasePricePerSqMeter * ppd.MinArea.Value * (ppd.ColorPricingFactor ?? 1m);
                }

                _logger.LogInformation("Tính giá cho OrderDetail {OrderDetailID} từ PrintingPriceDetail {PrintingDetailID}. Giá tính toán: {CalculatedPrice}", od.OrderDetailID, ppd.ServiceID, calculatedPrice);
                return calculatedPrice;
            }
            else
            {
                _logger.LogWarning("PrintingPriceDetail {PrintingDetailID} không có BasePricePerSqMeter hoặc OrderDetail {OrderDetailID} không có ActualAreaUsed. Sử dụng giá mặc định: {DefaultPrice}", ppd.ServiceID, od.OrderDetailID, defaultPrice);
                return defaultPrice;
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
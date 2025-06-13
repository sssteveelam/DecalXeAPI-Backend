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
                                                .ThenInclude(ds => ds.PrintingPriceDetail) // Bao gồm PrintingPriceDetail
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
                                                    .ThenInclude(ds => ds.PrintingPriceDetail) // Bao gồm PrintingPriceDetail
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

            // Kiểm tra tồn tại FKs (Controller cũng kiểm tra nhưng Service nên kiểm tra lại để đảm bảo tính toàn vẹn)
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
                                        .Include(s => s.ServiceProducts!) // Sử dụng ! để bỏ qua warning nullability nếu chắc chắn ServiceProducts không null sau Include
                                            .ThenInclude(sp => sp.Product)
                                        .Include(s => s.PrintingPriceDetail) // Bao gồm chi tiết giá in
                                        .FirstOrDefaultAsync(s => s.ServiceID == orderDetail.ServiceID);

            if (service == null)
            {
                return (null, "Dịch vụ không tồn tại hoặc không tìm thấy thông tin sản phẩm liên quan.");
            }

            // Giá ban đầu từ dịch vụ (có thể là giá mặc định nếu không có công thức phức tạp)
            orderDetail.Price = service.Price;

            // --- MỚI: TÍNH TOÁN FINALCALCULATEDPRICE DỰA TRÊN PRINTINGPRICEDETAIL ---
            // Nếu có PrintingPriceDetail, tính toán giá dựa trên công thức, ngược lại dùng giá mặc định
            orderDetail.FinalCalculatedPrice = CalculateFinalPrice(service.PrintingPriceDetail, orderDetail, orderDetail.Price);


            // --- LOGIC NGHIỆP VỤ: CẬP NHẬT TỒN KHO (STOCKQUANTITY) ---
            if (service.ServiceProducts != null)
            {
                foreach (var sp in service.ServiceProducts)
                {
                    if (sp.Product != null)
                    {
                        var quantityToDeduct = sp.QuantityUsed * orderDetail.Quantity;
                        if (sp.Product.StockQuantity < quantityToDeduct)
                        {
                            return (null, $"Sản phẩm '{sp.Product.ProductName}' không đủ tồn kho. Chỉ còn {sp.Product.StockQuantity} {sp.Product.Unit} trong kho, nhưng cần {quantityToDeduct} {sp.Product.Unit}.");
                        }
                        sp.Product.StockQuantity -= (int)quantityToDeduct; // Giảm số lượng tồn kho
                        _context.Products.Update(sp.Product);
                    }
                }
            }
            await _context.SaveChangesAsync(); // Lưu các thay đổi về tồn kho

            // --- BƯỚC THÊM ORDERDETAIL VÀ TÍNH LẠI TỔNG TIỀN ---
            _context.OrderDetails.Add(orderDetail);
            await _context.SaveChangesAsync();

            await RecalculateOrderTotalAmount(orderDetail.OrderID); // Tính lại tổng tiền Order

            // Tải lại Navigation Properties để có dữ liệu cho DTO
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
                                                    .ThenInclude(ds => ds.ServiceProducts!)
                                                        .ThenInclude(sp => sp.Product)
                                                .AsNoTracking() // Dùng AsNoTracking để không theo dõi đối tượng cũ
                                                .FirstOrDefaultAsync(od => od.OrderDetailID == id);
            if (oldOrderDetail == null)
            {
                _logger.LogWarning("Không tìm thấy chi tiết đơn hàng cũ để cập nhật với ID: {OrderDetailID}", id);
                return (false, "Chi tiết đơn hàng không tồn tại.");
            }

            // Kiểm tra FKs
            if (!string.IsNullOrEmpty(orderDetail.OrderID) && !await OrderExistsAsync(orderDetail.OrderID))
            {
                return (false, "OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(orderDetail.ServiceID) && !await DecalServiceExistsAsync(orderDetail.ServiceID))
            {
                return (false, "ServiceID không tồn tại.");
            }

            var newService = await _context.DecalServices
                                            .Include(s => s.ServiceProducts!)
                                                .ThenInclude(sp => sp.Product)
                                            .Include(s => s.PrintingPriceDetail) // Bao gồm chi tiết giá in
                                            .FirstOrDefaultAsync(s => s.ServiceID == orderDetail.ServiceID);
            if (newService == null)
            {
                return (false, "Dịch vụ mới không tồn tại hoặc không tìm thấy thông tin sản phẩm liên quan.");
            }

            orderDetail.Price = newService.Price;

            // --- MỚI: TÍNH TOÁN FINALCALCULATEDPRICE DỰA TRÊN PRINTINGPRICEDETAIL (CHO UPDATE) ---
            orderDetail.FinalCalculatedPrice = CalculateFinalPrice(newService.PrintingPriceDetail, orderDetail, orderDetail.Price);


            // --- LOGIC NGHIỆP VỤ: CẬP NHẬT TỒN KHO ---
            if (oldOrderDetail.Quantity != orderDetail.Quantity || oldOrderDetail.ServiceID != orderDetail.ServiceID)
            {
                // Hoàn trả tồn kho từ trạng thái cũ của OrderDetail
                if (oldOrderDetail.DecalService?.ServiceProducts != null)
                {
                    foreach (var sp in oldOrderDetail.DecalService.ServiceProducts)
                    {
                        if (sp.Product != null)
                        {
                            sp.Product.StockQuantity += (int)(sp.QuantityUsed * oldOrderDetail.Quantity);
                            _context.Products.Update(sp.Product);
                        }
                    }
                }

                // Giảm tồn kho theo trạng thái mới của OrderDetail
                if (newService.ServiceProducts != null)
                {
                    foreach (var sp in newService.ServiceProducts)
                    {
                        if (sp.Product != null)
                        {
                            var quantityToDeduct = sp.QuantityUsed * orderDetail.Quantity;
                            if (sp.Product.StockQuantity < quantityToDeduct)
                            {
                                // Nếu không đủ tồn kho mới, rollback các thay đổi tồn kho đã hoàn trả
                                if (oldOrderDetail.DecalService?.ServiceProducts != null)
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
            await _context.SaveChangesAsync(); // Lưu các thay đổi về tồn kho

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

        // Xóa chi tiết đơn hàng (có logic tồn kho và cập nhật tổng tiền)
        public async Task<(bool Success, string? ErrorMessage)> DeleteOrderDetailAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa chi tiết đơn hàng với ID: {OrderDetailID}", id);

            var orderDetail = await _context.OrderDetails
                                            .Include(od => od.DecalService)
                                                .ThenInclude(ds => ds.ServiceProducts!)
                                                    .ThenInclude(sp => sp.Product)
                                            .FirstOrDefaultAsync(od => od.OrderDetailID == id);
            if (orderDetail == null)
            {
                _logger.LogWarning("Không tìm thấy chi tiết đơn hàng để xóa với ID: {OrderDetailID}", id);
                return (false, "Chi tiết đơn hàng không tồn tại.");
            }

            _context.OrderDetails.Remove(orderDetail);
            await _context.SaveChangesAsync();

            if (orderDetail.DecalService?.ServiceProducts != null)
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
                                    .Include(o => o.OrderDetails) // Cần OrderDetails để tính tổng
                                    .FirstOrDefaultAsync(o => o.OrderID == orderId);

            if (order != null)
            {
                // Sẽ tính tổng dựa trên FinalCalculatedPrice của OrderDetail
                order.TotalAmount = order.OrderDetails?.Sum(od => od.FinalCalculatedPrice * od.Quantity) ?? 0m;

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
            }
        }

        // --- MỚI: HÀM HỖ TRỢ TÍNH TOÁN FINALCALCULATEDPRICE ---
        // Đây là logic tính giá in dựa trên PrintingPriceDetail và thông tin thực tế từ OrderDetail
        // defaultPrice là giá mặc định từ DecalService nếu không áp dụng công thức phức tạp
        private decimal CalculateFinalPrice(PrintingPriceDetail? ppd, OrderDetail od, decimal defaultPrice)
        {
            if (ppd == null)
            {
                _logger.LogInformation("Không có PrintingPriceDetail cho ServiceID {ServiceID}. Sử dụng giá mặc định {DefaultPrice}.", od.ServiceID, defaultPrice);
                return defaultPrice;
            }

            decimal calculatedPrice = 0m;

            // 1. Tính giá dựa trên BasePricePerSqMeter và ActualAreaUsed
            // Giả định ActualAreaUsed phải có giá trị cho dịch vụ cần tính giá phức tạp
            if (ppd.BasePricePerSqMeter > 0 && od.ActualAreaUsed.HasValue && od.ActualAreaUsed.Value > 0)
            {
                calculatedPrice = ppd.BasePricePerSqMeter * od.ActualAreaUsed.Value;

                // 2. Áp dụng hệ số giá theo màu nếu có
                if (ppd.ColorPricingFactor.HasValue && ppd.ColorPricingFactor.Value > 0)
                {
                    calculatedPrice *= ppd.ColorPricingFactor.Value;
                }

                // 3. Áp dụng các quy tắc theo Min/Max Area/Length/Width nếu có
                // Ví dụ: Nếu diện tích thực tế nhỏ hơn min, tính theo min area
                if (ppd.MinArea.HasValue && od.ActualAreaUsed.Value < ppd.MinArea.Value)
                {
                    calculatedPrice = ppd.BasePricePerSqMeter * ppd.MinArea.Value * (ppd.ColorPricingFactor ?? 1m);
                }
                // Thêm các logic khác cho MinLength, MaxLength, MaxArea nếu cần theo yêu cầu cụ thể
                // Ví dụ: if (ppd.MinLength.HasValue && od.ActualLengthUsed.HasValue && od.ActualLengthUsed.Value < ppd.MinLength.Value) { calculatedPrice += ...; }
                // Hoặc xử lý các trường hợp MaxLength/MaxArea vượt quá giới hạn

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
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
                                            .ToListAsync();
            return _mapper.Map<List<OrderDetailDto>>(orderDetails);
        }

        public async Task<OrderDetailDto?> GetOrderDetailByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy chi tiết đơn hàng với ID: {OrderDetailID}", id);
            var orderDetail = await _context.OrderDetails
                                            .Include(od => od.Order)
                                            .Include(od => od.DecalService)
                                            .FirstOrDefaultAsync(od => od.OrderDetailID == id);
            return _mapper.Map<OrderDetailDto>(orderDetail);
        }

        // --- PHẪU THUẬT LẠI HOÀN TOÀN HÀM CREATE ---
        public async Task<(OrderDetailDto? OrderDetail, string? ErrorMessage)> CreateOrderDetailAsync(OrderDetail orderDetail)
        {
            _logger.LogInformation("Bắt đầu tạo chi tiết đơn hàng cho OrderID: {OrderID}", orderDetail.OrderID);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders.Include(o => o.CustomerVehicle).FirstOrDefaultAsync(o => o.OrderID == orderDetail.OrderID);
                if (order == null) return (null, "OrderID không tồn tại.");

                var service = await _context.DecalServices.Include(s => s.DecalType).FirstOrDefaultAsync(s => s.ServiceID == orderDetail.ServiceID);
                if (service == null) return (null, "ServiceID không tồn tại.");

                // LOGIC TÍNH GIÁ MỚI (TINH GỌN)
                decimal finalPrice;
                var priceInfo = await _context.VehicleModelDecalTypes
                    .FirstOrDefaultAsync(p => p.ModelID == order.CustomerVehicle.ModelID && p.DecalTypeID == service.DecalType.DecalTypeID);

                finalPrice = priceInfo?.Price ?? service.Price; // Ưu tiên giá tùy chỉnh, nếu không có thì lấy giá mặc định

                orderDetail.Price = finalPrice;
                orderDetail.FinalCalculatedPrice = finalPrice; // Giá cuối cùng giờ bằng giá đơn vị luôn

                _context.OrderDetails.Add(orderDetail);
                await _context.SaveChangesAsync();
                await RecalculateOrderTotalAmount(orderDetail.OrderID); // Tính lại tổng tiền đơn hàng

                await transaction.CommitAsync();
                return (_mapper.Map<OrderDetailDto>(orderDetail), null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi tạo OrderDetail cho OrderID: {OrderID}", orderDetail.OrderID);
                return (null, "Đã xảy ra lỗi hệ thống khi tạo chi tiết đơn hàng.");
            }
        }

        // HÀM UPDATE MỚI
        public async Task<(bool Success, string? ErrorMessage)> UpdateOrderDetailAsync(string id, OrderDetail orderDetail)
        {
            if (id != orderDetail.OrderDetailID) return (false, "ID không khớp.");

            var oldOrderDetail = await _context.OrderDetails.AsNoTracking().FirstOrDefaultAsync(od => od.OrderDetailID == id);
            if (oldOrderDetail == null) return (false, "Chi tiết đơn hàng không tồn tại.");

            _context.Entry(orderDetail).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await RecalculateOrderTotalAmount(orderDetail.OrderID);

            return (true, null);
        }

        // HÀM DELETE MỚI
        public async Task<(bool Success, string? ErrorMessage)> DeleteOrderDetailAsync(string id)
        {
            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail == null) return (false, "Chi tiết đơn hàng không tồn tại.");

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

        

        public async Task<bool> OrderDetailExistsAsync(string id) => await _context.OrderDetails.AnyAsync(e => e.OrderDetailID == id);
        public async Task<bool> OrderExistsAsync(string id) => await _context.Orders.AnyAsync(e => e.OrderID == id);
        public async Task<bool> DecalServiceExistsAsync(string id) => await _context.DecalServices.AnyAsync(e => e.ServiceID == id);
    }
}
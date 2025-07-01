// DecalXeAPI/Services/Implementations/PaymentService.cs
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
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(ApplicationDbContext context, IMapper mapper, ILogger<PaymentService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsAsync()
        {
            _logger.LogInformation("Lấy danh sách thanh toán.");
            // Bỏ Include(p => p.Promotion)
            var payments = await _context.Payments
                                        .Include(p => p.Order) 
                                        .ToListAsync();
            return _mapper.Map<List<PaymentDto>>(payments);
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy thanh toán với ID: {PaymentID}", id);
            // Bỏ Include(p => p.Promotion)
            var payment = await _context.Payments
                                        .Include(p => p.Order)
                                        .FirstOrDefaultAsync(p => p.PaymentID == id);
            if (payment == null)
            {
                _logger.LogWarning("Không tìm thấy thanh toán với ID: {PaymentID}", id);
                return null;
            }
            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<PaymentDto> CreatePaymentAsync(Payment payment)
        {
            _logger.LogInformation("Yêu cầu tạo thanh toán mới cho OrderID: {OrderID}", payment.OrderID);
            if (!string.IsNullOrEmpty(payment.OrderID) && !await OrderExistsAsync(payment.OrderID))
            {
                throw new ArgumentException("OrderID không tồn tại.");
            }
            // Bỏ logic kiểm tra PromotionID

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Bỏ việc tải lại Promotion
            await _context.Entry(payment).Reference(p => p.Order).LoadAsync();

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<bool> UpdatePaymentAsync(string id, Payment payment)
        {
            _logger.LogInformation("Yêu cầu cập nhật thanh toán với ID: {PaymentID}", id);
            if (id != payment.PaymentID) return false;
            if (!await PaymentExistsAsync(id)) return false;

            if (!string.IsNullOrEmpty(payment.OrderID) && !await OrderExistsAsync(payment.OrderID))
            {
                throw new ArgumentException("OrderID không tồn tại.");
            }
            // Bỏ logic kiểm tra PromotionID

            _context.Entry(payment).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật thanh toán với ID: {PaymentID}", id);
                throw;
            }
        }

        public async Task<bool> DeletePaymentAsync(string id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return false;

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PaymentExistsAsync(string id) => await _context.Payments.AnyAsync(e => e.PaymentID == id);
        public async Task<bool> OrderExistsAsync(string id) => await _context.Orders.AnyAsync(e => e.OrderID == id);
        // Bỏ hàm PromotionExistsAsync
    }
}
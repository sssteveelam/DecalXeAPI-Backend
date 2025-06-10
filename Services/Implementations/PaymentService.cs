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
using System; // Để dùng ArgumentException

namespace DecalXeAPI.Services.Implementations
{
    public class PaymentService : IPaymentService // <-- Kế thừa từ IPaymentService
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
            var payments = await _context.Payments
                                        .Include(p => p.Order)
                                        .Include(p => p.Promotion)
                                        .ToListAsync();
            var paymentDtos = _mapper.Map<List<PaymentDto>>(payments);
            return paymentDtos;
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy thanh toán với ID: {PaymentID}", id);
            var payment = await _context.Payments
                                        .Include(p => p.Order)
                                        .Include(p => p.Promotion)
                                        .FirstOrDefaultAsync(p => p.PaymentID == id);

            if (payment == null)
            {
                _logger.LogWarning("Không tìm thấy thanh toán với ID: {PaymentID}", id);
                return null;
            }

            var paymentDto = _mapper.Map<PaymentDto>(payment);
            _logger.LogInformation("Đã trả về thanh toán với ID: {PaymentID}", id);
            return paymentDto;
        }

        public async Task<PaymentDto> CreatePaymentAsync(Payment payment)
        {
            _logger.LogInformation("Yêu cầu tạo thanh toán mới cho OrderID: {OrderID}", payment.OrderID);

            // Kiểm tra FKs
            if (!string.IsNullOrEmpty(payment.OrderID) && !await OrderExistsAsync(payment.OrderID))
            {
                _logger.LogWarning("OrderID không tồn tại khi tạo thanh toán: {OrderID}", payment.OrderID);
                throw new ArgumentException("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(payment.PromotionID) && !await PromotionExistsAsync(payment.PromotionID))
            {
                _logger.LogWarning("PromotionID không tồn tại khi tạo thanh toán: {PromotionID}", payment.PromotionID);
                throw new ArgumentException("PromotionID không tồn tại.");
            }

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Tải lại thông tin liên quan để AutoMapper có thể ánh xạ đầy đủ
            await _context.Entry(payment).Reference(p => p.Order).LoadAsync();
            await _context.Entry(payment).Reference(p => p.Promotion).LoadAsync();

            var paymentDto = _mapper.Map<PaymentDto>(payment);
            _logger.LogInformation("Đã tạo thanh toán mới với ID: {PaymentID}", payment.PaymentID);
            return paymentDto;
        }

        public async Task<bool> UpdatePaymentAsync(string id, Payment payment)
        {
            _logger.LogInformation("Yêu cầu cập nhật thanh toán với ID: {PaymentID}", id);

            if (id != payment.PaymentID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với PaymentID trong body ({PaymentIDBody})", id, payment.PaymentID);
                return false;
            }

            if (!await PaymentExistsAsync(id))
            {
                _logger.LogWarning("Không tìm thấy thanh toán để cập nhật với ID: {PaymentID}", id);
                return false;
            }

            // Kiểm tra FKs
            if (!string.IsNullOrEmpty(payment.OrderID) && !await OrderExistsAsync(payment.OrderID))
            {
                _logger.LogWarning("OrderID không tồn tại khi cập nhật thanh toán: {OrderID}", payment.OrderID);
                throw new ArgumentException("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(payment.PromotionID) && !await PromotionExistsAsync(payment.PromotionID))
            {
                _logger.LogWarning("PromotionID không tồn tại khi cập nhật thanh toán: {PromotionID}", payment.PromotionID);
                throw new ArgumentException("PromotionID không tồn tại.");
            }

            _context.Entry(payment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật thanh toán với ID: {PaymentID}", id);
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
            _logger.LogInformation("Yêu cầu xóa thanh toán với ID: {PaymentID}", id);
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                _logger.LogWarning("Không tìm thấy thanh toán để xóa với ID: {PaymentID}", id);
                return false;
            }

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa thanh toán với ID: {PaymentID}", id);
            return true;
        }

        // --- HÀM HỖ TRỢ: KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (PUBLIC CHO INTERFACE) ---
        public async Task<bool> PaymentExistsAsync(string id)
        {
            return await _context.Payments.AnyAsync(e => e.PaymentID == id);
        }

        public async Task<bool> OrderExistsAsync(string id)
        {
            return await _context.Orders.AnyAsync(e => e.OrderID == id);
        }

        public async Task<bool> PromotionExistsAsync(string id)
        {
            return await _context.Promotions.AnyAsync(e => e.PromotionID == id);
        }
    }
}
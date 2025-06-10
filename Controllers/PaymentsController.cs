using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Vẫn cần DbContext cho các hàm Exists cơ bản
using DecalXeAPI.Data; // Vẫn cần ApplicationDbContext cho các hàm Exists
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // <-- THÊM DÒNG NÀY (Để sử dụng IPaymentService)
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System; // Để sử dụng ArgumentException

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Sales,Accountant")] // Quyền cho PaymentsController
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists cơ bản
        private readonly IPaymentService _paymentService; // <-- KHAI BÁO BIẾN CHO SERVICE
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(ApplicationDbContext context, IPaymentService paymentService, IMapper mapper, ILogger<PaymentsController> logger) // <-- TIÊM IPaymentService
        {
            _context = context;
            _paymentService = paymentService;
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/Payments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPayments()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách thanh toán.");
            var payments = await _paymentService.GetPaymentsAsync();
            return Ok(payments);
        }

        // API: GET api/Payments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDto>> GetPayment(string id)
        {
            _logger.LogInformation("Yêu cầu lấy thanh toán với ID: {PaymentID}", id);
            var paymentDto = await _paymentService.GetPaymentByIdAsync(id);

            if (paymentDto == null)
            {
                _logger.LogWarning("Không tìm thấy thanh toán với ID: {PaymentID}", id);
                return NotFound();
            }

            return Ok(paymentDto);
        }

        // API: POST api/Payments
        [HttpPost]
        public async Task<ActionResult<PaymentDto>> PostPayment(Payment payment) // Vẫn nhận Payment Model
        {
            _logger.LogInformation("Yêu cầu tạo thanh toán mới cho OrderID: {OrderID}", payment.OrderID);

            // --- KIỂM TRA FKs CHÍNH TRƯỚC KHI GỬI VÀO SERVICE ---
            if (!string.IsNullOrEmpty(payment.OrderID) && !OrderExists(payment.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(payment.PromotionID) && !PromotionExists(payment.PromotionID))
            {
                return BadRequest("PromotionID không tồn tại.");
            }

            try
            {
                var createdPaymentDto = await _paymentService.CreatePaymentAsync(payment);
                _logger.LogInformation("Đã tạo thanh toán mới với ID: {PaymentID}", createdPaymentDto.PaymentID);
                return CreatedAtAction(nameof(GetPayment), new { id = createdPaymentDto.PaymentID }, createdPaymentDto);
            }
            catch (ArgumentException ex) // Bắt lỗi từ Service nếu FK không hợp lệ
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi tạo thanh toán: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/Payments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPayment(string id, Payment payment)
        {
            _logger.LogInformation("Yêu cầu cập nhật thanh toán với ID: {PaymentID}", id);
            if (id != payment.PaymentID)
            {
                return BadRequest();
            }

            // Kiểm tra FKs chính
            if (!string.IsNullOrEmpty(payment.OrderID) && !OrderExists(payment.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(payment.PromotionID) && !PromotionExists(payment.PromotionID))
            {
                return BadRequest("PromotionID không tồn tại.");
            }

            try
            {
                var success = await _paymentService.UpdatePaymentAsync(id, payment);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy thanh toán để cập nhật với ID: {PaymentID}", id);
                    return NotFound();
                }

                _logger.LogInformation("Đã cập nhật thanh toán với ID: {PaymentID}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi cập nhật thanh toán: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // API: DELETE api/Payments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(string id)
        {
            _logger.LogInformation("Yêu cầu xóa thanh toán với ID: {PaymentID}", id);
            var success = await _paymentService.DeletePaymentAsync(id);

            if (!success)
            {
                _logger.LogWarning("Không tìm thấy thanh toán để xóa với ID: {PaymentID}", id);
                return NotFound();
            }

            return NoContent();
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool PaymentExists(string id) { return _context.Payments.Any(e => e.PaymentID == id); }
        private bool OrderExists(string id) { return _context.Orders.Any(e => e.OrderID == id); }
        private bool PromotionExists(string id) { return _context.Promotions.Any(e => e.PromotionID == id); }
    }
}
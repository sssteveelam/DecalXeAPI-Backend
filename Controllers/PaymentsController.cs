using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using System;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: GET api/Payments
        // Lấy tất cả các Payment, bao gồm thông tin Order và Promotion liên quan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPayments()
        {
            return await _context.Payments
                                .Include(p => p.Order)
                                .Include(p => p.Promotion)
                                .ToListAsync();
        }

        // API: GET api/Payments/{id}
        // Lấy thông tin một Payment theo PaymentID, bao gồm các thông tin liên quan
        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(string id)
        {
            var payment = await _context.Payments
                                        .Include(p => p.Order)
                                        .Include(p => p.Promotion)
                                        .FirstOrDefaultAsync(p => p.PaymentID == id);

            if (payment == null)
            {
                return NotFound();
            }

            return payment;
        }

        // API: POST api/Payments
        // Tạo một Payment mới
        [HttpPost]
        public async Task<ActionResult<Payment>> PostPayment(Payment payment)
        {
            // Kiểm tra FKs có tồn tại không
            if (!string.IsNullOrEmpty(payment.OrderID) && !OrderExists(payment.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(payment.PromotionID) && !PromotionExists(payment.PromotionID))
            {
                return BadRequest("PromotionID không tồn tại.");
            }

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Tải lại thông tin liên quan để trả về đầy đủ
            await _context.Entry(payment).Reference(p => p.Order).LoadAsync();
            await _context.Entry(payment).Reference(p => p.Promotion).LoadAsync();

            return CreatedAtAction(nameof(GetPayment), new { id = payment.PaymentID }, payment);
        }

        // API: PUT api/Payments/{id}
        // Cập nhật thông tin một Payment hiện có
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPayment(string id, Payment payment)
        {
            if (id != payment.PaymentID)
            {
                return BadRequest();
            }

            // Kiểm tra FKs trước khi cập nhật
            if (!string.IsNullOrEmpty(payment.OrderID) && !OrderExists(payment.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(payment.PromotionID) && !PromotionExists(payment.PromotionID))
            {
                return BadRequest("PromotionID không tồn tại.");
            }

            _context.Entry(payment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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

            return NoContent();
        }

        // API: DELETE api/Payments/{id}
        // Xóa một Payment
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(string id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Hàm hỗ trợ: Kiểm tra xem Payment có tồn tại không
        private bool PaymentExists(string id)
        {
            return _context.Payments.Any(e => e.PaymentID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem Order có tồn tại không (copy từ OrdersController)
        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem Promotion có tồn tại không (copy từ PromotionsController)
        private bool PromotionExists(string id)
        {
            return _context.Promotions.Any(e => e.PromotionID == id);
        }
    }
}
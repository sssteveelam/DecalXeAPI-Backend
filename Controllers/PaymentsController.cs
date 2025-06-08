using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs; // Để sử dụng PaymentDto
using AutoMapper; // Để sử dụng AutoMapper
using System.Collections.Generic; // Để sử dụng IEnumerable
using System; // Để sử dụng DateTime

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Khai báo biến IMapper

        public PaymentsController(ApplicationDbContext context, IMapper mapper) // Tiêm IMapper
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/Payments
        // Lấy tất cả các Payment, bao gồm thông tin Order và Promotion liên quan, trả về DTO
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPayments() // Kiểu trả về là PaymentDto
        {
            var payments = await _context.Payments
                                        .Include(p => p.Order)
                                        .Include(p => p.Promotion)
                                        .ToListAsync();
            // Sử dụng AutoMapper để ánh xạ từ List<Payment> sang List<PaymentDto>
            var paymentDtos = _mapper.Map<List<PaymentDto>>(payments);
            return Ok(paymentDtos);
        }

        // API: GET api/Payments/{id}
        // Lấy thông tin một Payment theo PaymentID, bao gồm các thông tin liên quan, trả về DTO
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDto>> GetPayment(string id) // Kiểu trả về là PaymentDto
        {
            var payment = await _context.Payments
                                        .Include(p => p.Order)
                                        .Include(p => p.Promotion)
                                        .FirstOrDefaultAsync(p => p.PaymentID == id);

            if (payment == null)
            {
                return NotFound();
            }

            // Sử dụng AutoMapper để ánh xạ từ Payment Model sang PaymentDto
            var paymentDto = _mapper.Map<PaymentDto>(payment);
            return Ok(paymentDto);
        }

        // API: POST api/Payments
        // Tạo một Payment mới, nhận vào Payment Model, trả về PaymentDto sau khi tạo
        [HttpPost]
        public async Task<ActionResult<PaymentDto>> PostPayment(Payment payment) // Kiểu trả về là PaymentDto
        {
            // Kiểm tra FKs có tồn tại không
            if (!string.IsNullOrEmpty(payment.OrderID) && !OrderExists(payment.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }
            // PromotionID có thể null, chỉ kiểm tra nếu có giá trị
            if (!string.IsNullOrEmpty(payment.PromotionID) && !PromotionExists(payment.PromotionID))
            {
                return BadRequest("PromotionID không tồn tại.");
            }

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Tải lại thông tin liên quan để AutoMapper có thể ánh xạ đầy đủ
            await _context.Entry(payment).Reference(p => p.Order).LoadAsync();
            await _context.Entry(payment).Reference(p => p.Promotion).LoadAsync();

            // Ánh xạ Payment Model vừa tạo sang PaymentDto để trả về
            var paymentDto = _mapper.Map<PaymentDto>(payment);
            return CreatedAtAction(nameof(GetPayment), new { id = paymentDto.PaymentID }, paymentDto);
        }

        // API: PUT api/Payments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPayment(string id, Payment payment)
        {
            if (id != payment.PaymentID)
            {
                return BadRequest();
            }

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

        private bool PaymentExists(string id)
        {
            return _context.Payments.Any(e => e.PaymentID == id);
        }

        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }

        private bool PromotionExists(string id)
        {
            return _context.Promotions.Any(e => e.PromotionID == id);
        }
    }
}
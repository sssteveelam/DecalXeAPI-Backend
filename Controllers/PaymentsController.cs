// DecalXeAPI/Controllers/PaymentsController.cs
using Microsoft.AspNetCore.Mvc;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.EntityFrameworkCore; // <-- "CHÌA KHÓA" MÀ HUYNH ĐỆ MÌNH CẦN LÀ ĐÂY!

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Sales,Accountant")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPayments()
        {
            var payments = await _paymentService.GetPaymentsAsync();
            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDto>> GetPayment(string id)
        {
            var paymentDto = await _paymentService.GetPaymentByIdAsync(id);
            if (paymentDto == null) return NotFound();
            return Ok(paymentDto);
        }

        [HttpPost]
        public async Task<ActionResult<PaymentDto>> PostPayment(Payment payment)
        {
            try
            {
                var createdPaymentDto = await _paymentService.CreatePaymentAsync(payment);
                return CreatedAtAction(nameof(GetPayment), new { id = createdPaymentDto.PaymentID }, createdPaymentDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi tạo thanh toán: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPayment(string id, Payment payment)
        {
            if (id != payment.PaymentID) return BadRequest();

            try
            {
                var success = await _paymentService.UpdatePaymentAsync(id, payment);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateConcurrencyException) // <-- Giờ thì trình biên dịch đã biết nó là ai rồi!
            {
                return Conflict("Lỗi xung đột dữ liệu, vui lòng thử lại.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(string id)
        {
            var success = await _paymentService.DeletePaymentAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
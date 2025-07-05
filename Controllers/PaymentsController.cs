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
using DecalXeAPI.Data;
using AutoMapper;
using System.Linq;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Sales,Accountant")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;
        private readonly ApplicationDbContext _context; // Vẫn cần để dùng các hàm hỗ trợ Exists
        private readonly IMapper _mapper; // Vẫn cần để ánh xạ DTOs nếu
        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger, ApplicationDbContext context, IMapper mapper)
        {
            _context = context; // Để dùng các hàm hỗ trợ Exists
            _mapper = mapper; // Để ánh xạ DTOs nếu cần
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

        // API: POST api/Payments (ĐÃ NÂNG CẤP)
        [HttpPost]
        public async Task<ActionResult<PaymentDto>> PostPayment(CreatePaymentDto createDto)
        {
            var payment = _mapper.Map<Payment>(createDto);
            
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

        // API: PUT api/Payments/{id} (ĐÃ NÂNG CẤP)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPayment(string id, UpdatePaymentDto updateDto)
        {
            // Cần inject ApplicationDbContext vào controller này để dùng FindAsync
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            _mapper.Map(updateDto, payment);
            
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
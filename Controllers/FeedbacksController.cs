using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using System;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbacksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FeedbacksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: GET api/Feedbacks
        // Lấy tất cả các Feedback, bao gồm thông tin Order và Customer liên quan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Feedback>>> GetFeedbacks()
        {
            return await _context.Feedbacks
                                .Include(f => f.Order)
                                .Include(f => f.Customer)
                                .ToListAsync();
        }

        // API: GET api/Feedbacks/{id}
        // Lấy thông tin một Feedback theo FeedbackID, bao gồm các thông tin liên quan
        [HttpGet("{id}")]
        public async Task<ActionResult<Feedback>> GetFeedback(string id)
        {
            var feedback = await _context.Feedbacks
                                        .Include(f => f.Order)
                                        .Include(f => f.Customer)
                                        .FirstOrDefaultAsync(f => f.FeedbackID == id);

            if (feedback == null)
            {
                return NotFound();
            }

            return feedback;
        }

        // API: POST api/Feedbacks
        // Tạo một Feedback mới
        [HttpPost]
        public async Task<ActionResult<Feedback>> PostFeedback(Feedback feedback)
        {
            // Kiểm tra FKs có tồn tại không
            if (!string.IsNullOrEmpty(feedback.OrderID) && !OrderExists(feedback.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(feedback.CustomerID) && !CustomerExists(feedback.CustomerID))
            {
                return BadRequest("CustomerID không tồn tại.");
            }

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            // Tải lại thông tin liên quan để trả về đầy đủ
            await _context.Entry(feedback).Reference(f => f.Order).LoadAsync();
            await _context.Entry(feedback).Reference(f => f.Customer).LoadAsync();

            return CreatedAtAction(nameof(GetFeedback), new { id = feedback.FeedbackID }, feedback);
        }

        // API: PUT api/Feedbacks/{id}
        // Cập nhật thông tin một Feedback hiện có
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFeedback(string id, Feedback feedback)
        {
            if (id != feedback.FeedbackID)
            {
                return BadRequest();
            }

            // Kiểm tra FKs trước khi cập nhật
            if (!string.IsNullOrEmpty(feedback.OrderID) && !OrderExists(feedback.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(feedback.CustomerID) && !CustomerExists(feedback.CustomerID))
            {
                return BadRequest("CustomerID không tồn tại.");
            }

            _context.Entry(feedback).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FeedbackExists(id))
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

        // API: DELETE api/Feedbacks/{id}
        // Xóa một Feedback
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeedback(string id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Hàm hỗ trợ: Kiểm tra xem Feedback có tồn tại không
        private bool FeedbackExists(string id)
        {
            return _context.Feedbacks.Any(e => e.FeedbackID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem Order có tồn tại không (copy từ OrdersController)
        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem Customer có tồn tại không (copy từ CustomersController)
        private bool CustomerExists(string id)
        {
            return _context.Customers.Any(e => e.CustomerID == id);
        }
    }
}
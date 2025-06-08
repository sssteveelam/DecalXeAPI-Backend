using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs; // Để sử dụng FeedbackDto
using AutoMapper; // Để sử dụng AutoMapper
using System.Collections.Generic; // Để sử dụng IEnumerable
using System; // Để sử dụng DateTime

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbacksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Khai báo biến IMapper

        public FeedbacksController(ApplicationDbContext context, IMapper mapper) // Tiêm IMapper
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/Feedbacks
        // Lấy tất cả các Feedback, bao gồm thông tin Order và Customer liên quan, trả về DTO
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetFeedbacks() // Kiểu trả về là FeedbackDto
        {
            var feedbacks = await _context.Feedbacks
                                        .Include(f => f.Order) // Tải thông tin Order
                                        .Include(f => f.Customer) // Tải thông tin Customer
                                        .ToListAsync();
            // Sử dụng AutoMapper để ánh xạ từ List<Feedback> sang List<FeedbackDto>
            var feedbackDtos = _mapper.Map<List<FeedbackDto>>(feedbacks);
            return Ok(feedbackDtos);
        }

        // API: GET api/Feedbacks/{id}
        // Lấy thông tin một Feedback theo FeedbackID, bao gồm các thông tin liên quan, trả về DTO
        [HttpGet("{id}")]
        public async Task<ActionResult<FeedbackDto>> GetFeedback(string id) // Kiểu trả về là FeedbackDto
        {
            var feedback = await _context.Feedbacks
                                        .Include(f => f.Order)
                                        .Include(f => f.Customer)
                                        .FirstOrDefaultAsync(f => f.FeedbackID == id);

            if (feedback == null)
            {
                return NotFound();
            }

            // Sử dụng AutoMapper để ánh xạ từ Feedback Model sang FeedbackDto
            var feedbackDto = _mapper.Map<FeedbackDto>(feedback);
            return Ok(feedbackDto);
        }

        // API: POST api/Feedbacks
        // Tạo một Feedback mới, nhận vào Feedback Model, trả về FeedbackDto sau khi tạo
        [HttpPost]
        public async Task<ActionResult<FeedbackDto>> PostFeedback(Feedback feedback) // Kiểu trả về là FeedbackDto
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

            // Tải lại thông tin liên quan để AutoMapper có thể ánh xạ đầy đủ
            await _context.Entry(feedback).Reference(f => f.Order).LoadAsync();
            await _context.Entry(feedback).Reference(f => f.Customer).LoadAsync();

            // Ánh xạ Feedback Model vừa tạo sang FeedbackDto để trả về
            var feedbackDto = _mapper.Map<FeedbackDto>(feedback);
            return CreatedAtAction(nameof(GetFeedback), new { id = feedbackDto.FeedbackID }, feedbackDto);
        }

        // API: PUT api/Feedbacks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFeedback(string id, Feedback feedback)
        {
            if (id != feedback.FeedbackID)
            {
                return BadRequest();
            }

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

        private bool FeedbackExists(string id)
        {
            return _context.Feedbacks.Any(e => e.FeedbackID == id);
        }

        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }

        private bool CustomerExists(string id)
        {
            return _context.Customers.Any(e => e.CustomerID == id);
        }
    }
}
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
    public class FeedbackService : IFeedbackService // <-- Kế thừa từ IFeedbackService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<FeedbackService> _logger;

        public FeedbackService(ApplicationDbContext context, IMapper mapper, ILogger<FeedbackService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<FeedbackDto>> GetFeedbacksAsync()
        {
            _logger.LogInformation("Lấy danh sách phản hồi.");
            var feedbacks = await _context.Feedbacks
                                        .Include(f => f.Order)
                                        .Include(f => f.Customer)
                                        .ToListAsync();
            var feedbackDtos = _mapper.Map<List<FeedbackDto>>(feedbacks);
            return feedbackDtos;
        }

        public async Task<FeedbackDto?> GetFeedbackByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy phản hồi với ID: {FeedbackID}", id);
            var feedback = await _context.Feedbacks
                                        .Include(f => f.Order)
                                        .Include(f => f.Customer)
                                        .FirstOrDefaultAsync(f => f.FeedbackID == id);

            if (feedback == null)
            {
                _logger.LogWarning("Không tìm thấy phản hồi với ID: {FeedbackID}", id);
                return null;
            }

            var feedbackDto = _mapper.Map<FeedbackDto>(feedback);
            _logger.LogInformation("Đã trả về phản hồi với ID: {FeedbackID}", id);
            return feedbackDto;
        }

        public async Task<FeedbackDto> CreateFeedbackAsync(Feedback feedback)
        {
            _logger.LogInformation("Yêu cầu tạo phản hồi mới cho OrderID: {OrderID}, CustomerID: {CustomerID}", feedback.OrderID, feedback.CustomerID);

            // Kiểm tra FKs
            if (!string.IsNullOrEmpty(feedback.OrderID) && !await OrderExistsAsync(feedback.OrderID))
            {
                _logger.LogWarning("OrderID không tồn tại khi tạo Feedback: {OrderID}", feedback.OrderID);
                throw new ArgumentException("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(feedback.CustomerID) && !await CustomerExistsAsync(feedback.CustomerID))
            {
                _logger.LogWarning("CustomerID không tồn tại khi tạo Feedback: {CustomerID}", feedback.CustomerID);
                throw new ArgumentException("CustomerID không tồn tại.");
            }

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            // Tải lại thông tin liên quan
            await _context.Entry(feedback).Reference(f => f.Order).LoadAsync();
            await _context.Entry(feedback).Reference(f => f.Customer).LoadAsync();

            var feedbackDto = _mapper.Map<FeedbackDto>(feedback);
            _logger.LogInformation("Đã tạo phản hồi mới với ID: {FeedbackID}", feedback.FeedbackID);
            return feedbackDto;
        }

        public async Task<bool> UpdateFeedbackAsync(string id, Feedback feedback)
        {
            _logger.LogInformation("Yêu cầu cập nhật phản hồi với ID: {FeedbackID}", id);

            if (id != feedback.FeedbackID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với FeedbackID trong body ({FeedbackIDBody})", id, feedback.FeedbackID);
                return false;
            }

            if (!await FeedbackExistsAsync(id))
            {
                _logger.LogWarning("Không tìm thấy phản hồi để cập nhật với ID: {FeedbackID}", id);
                return false;
            }

            // Kiểm tra FKs
            if (!string.IsNullOrEmpty(feedback.OrderID) && !await OrderExistsAsync(feedback.OrderID))
            {
                _logger.LogWarning("OrderID không tồn tại khi cập nhật Feedback: {OrderID}", feedback.OrderID);
                throw new ArgumentException("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(feedback.CustomerID) && !await CustomerExistsAsync(feedback.CustomerID))
            {
                _logger.LogWarning("CustomerID không tồn tại khi cập nhật Feedback: {CustomerID}", feedback.CustomerID);
                throw new ArgumentException("CustomerID không tồn tại.");
            }

            _context.Entry(feedback).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật phản hồi với ID: {FeedbackID}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật phản hồi với ID: {FeedbackID}", id);
                throw;
            }
        }

        public async Task<bool> DeleteFeedbackAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa phản hồi với ID: {FeedbackID}", id);
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                _logger.LogWarning("Không tìm thấy phản hồi để xóa với ID: {FeedbackID}", id);
                return false;
            }

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa phản hồi với ID: {FeedbackID}", id);
            return true;
        }

        // --- HÀM HỖ TRỢ: KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (PUBLIC CHO INTERFACE) ---
        public async Task<bool> FeedbackExistsAsync(string id)
        {
            return await _context.Feedbacks.AnyAsync(e => e.FeedbackID == id);
        }

        public async Task<bool> OrderExistsAsync(string id)
        {
            return await _context.Orders.AnyAsync(e => e.OrderID == id);
        }

        public async Task<bool> CustomerExistsAsync(string id)
        {
            return await _context.Customers.AnyAsync(e => e.CustomerID == id);
        }
    }
}
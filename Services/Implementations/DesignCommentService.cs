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
using System; // Để sử dụng ArgumentException

namespace DecalXeAPI.Services.Implementations
{
    public class DesignCommentService : IDesignCommentService // Kế thừa từ IDesignCommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DesignCommentService> _logger;

        public DesignCommentService(ApplicationDbContext context, IMapper mapper, ILogger<DesignCommentService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<DesignCommentDto>> GetDesignCommentsAsync()
        {
            _logger.LogInformation("Lấy danh sách bình luận thiết kế.");
            // Bao gồm Design, SenderAccount và Role của SenderAccount để ánh xạ DTO
            var designComments = await _context.DesignComments
                                                .Include(dc => dc.Design)
                                                .Include(dc => dc.SenderAccount!) // Sử dụng ! để bỏ qua warning nullability
                                                    .ThenInclude(acc => acc.Role)
                                                .ToListAsync();
            var designCommentDtos = _mapper.Map<List<DesignCommentDto>>(designComments);
            return designCommentDtos;
        }

        public async Task<DesignCommentDto?> GetDesignCommentByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy bình luận thiết kế với ID: {CommentID}", id);
            var designComment = await _context.DesignComments
                                                .Include(dc => dc.Design)
                                                .Include(dc => dc.SenderAccount!)
                                                    .ThenInclude(acc => acc.Role)
                                                .FirstOrDefaultAsync(dc => dc.CommentID == id);

            if (designComment == null)
            {
                _logger.LogWarning("Không tìm thấy bình luận thiết kế với ID: {CommentID}", id);
                return null;
            }

            var designCommentDto = _mapper.Map<DesignCommentDto>(designComment);
            _logger.LogInformation("Đã trả về bình luận thiết kế với ID: {CommentID}", id);
            return designCommentDto;
        }

        public async Task<DesignCommentDto> CreateDesignCommentAsync(DesignComment designComment)
        {
            _logger.LogInformation("Yêu cầu tạo bình luận thiết kế mới cho DesignID: {DesignID}, SenderAccountID: {SenderAccountID}",
                                    designComment.DesignID, designComment.SenderAccountID);

            // Kiểm tra FKs
            if (!string.IsNullOrEmpty(designComment.DesignID) && !await DesignExistsAsync(designComment.DesignID))
            {
                _logger.LogWarning("DesignID không tồn tại khi tạo DesignComment: {DesignID}", designComment.DesignID);
                throw new ArgumentException("DesignID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(designComment.SenderAccountID) && !await AccountExistsAsync(designComment.SenderAccountID))
            {
                _logger.LogWarning("SenderAccountID không tồn tại khi tạo DesignComment: {SenderAccountID}", designComment.SenderAccountID);
                throw new ArgumentException("SenderAccountID không tồn tại.");
            }
            // Kiểm tra ParentCommentID nếu có
            if (!string.IsNullOrEmpty(designComment.ParentCommentID) && !await DesignCommentExistsAsync(designComment.ParentCommentID))
            {
                _logger.LogWarning("ParentCommentID không tồn tại khi tạo DesignComment: {ParentCommentID}", designComment.ParentCommentID);
                throw new ArgumentException("ParentCommentID không tồn tại.");
            }

            _context.DesignComments.Add(designComment);
            await _context.SaveChangesAsync();

            // Tải lại thông tin liên quan để AutoMapper có thể ánh xạ đầy đủ
            await _context.Entry(designComment).Reference(dc => dc.Design).LoadAsync();
            await _context.Entry(designComment).Reference(dc => dc.SenderAccount).LoadAsync();
            if (designComment.SenderAccount != null)
            {
                await _context.Entry(designComment.SenderAccount).Reference(acc => acc.Role).LoadAsync();
            }

            var designCommentDto = _mapper.Map<DesignCommentDto>(designComment);
            _logger.LogInformation("Đã tạo bình luận thiết kế mới với ID: {CommentID}", designComment.CommentID);
            return designCommentDto;
        }

        public async Task<bool> UpdateDesignCommentAsync(string id, DesignComment designComment)
        {
            _logger.LogInformation("Yêu cầu cập nhật bình luận thiết kế với ID: {CommentID}", id);

            if (id != designComment.CommentID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với CommentID trong body ({CommentIDBody})", id, designComment.CommentID);
                return false;
            }

            if (!await DesignCommentExistsAsync(id))
            {
                _logger.LogWarning("Không tìm thấy bình luận thiết kế để cập nhật với ID: {CommentID}", id);
                return false;
            }

            // Kiểm tra FKs
            if (!string.IsNullOrEmpty(designComment.DesignID) && !await DesignExistsAsync(designComment.DesignID))
            {
                _logger.LogWarning("DesignID không tồn tại khi cập nhật DesignComment: {DesignID}", designComment.DesignID);
                throw new ArgumentException("DesignID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(designComment.SenderAccountID) && !await AccountExistsAsync(designComment.SenderAccountID))
            {
                _logger.LogWarning("SenderAccountID không tồn tại khi cập nhật DesignComment: {SenderAccountID}", designComment.SenderAccountID);
                throw new ArgumentException("SenderAccountID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(designComment.ParentCommentID) && !await DesignCommentExistsAsync(designComment.ParentCommentID))
            {
                _logger.LogWarning("ParentCommentID không tồn tại khi cập nhật DesignComment: {ParentCommentID}", designComment.ParentCommentID);
                throw new ArgumentException("ParentCommentID không tồn tại.");
            }

            _context.Entry(designComment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật bình luận thiết kế với ID: {CommentID}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật bình luận thiết kế với ID: {CommentID}", id);
                throw;
            }
        }

        public async Task<bool> DeleteDesignCommentAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa bình luận thiết kế với ID: {CommentID}", id);
            var designComment = await _context.DesignComments.FindAsync(id);
            if (designComment == null)
            {
                _logger.LogWarning("Không tìm thấy bình luận thiết kế để xóa với ID: {CommentID}", id);
                return false;
            }

            // Kiểm tra xem có comment con nào trỏ về comment này không
            if (await _context.DesignComments.AnyAsync(dc => dc.ParentCommentID == id))
            {
                _logger.LogWarning("Không thể xóa bình luận {CommentID} vì có các bình luận trả lời khác trỏ về nó.", id);
                throw new InvalidOperationException("Không thể xóa bình luận này vì có các bình luận trả lời khác trỏ về nó.");
            }


            _context.DesignComments.Remove(designComment);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa bình luận thiết kế với ID: {CommentID}", id);
            return true;
        }

        // Hàm hỗ trợ: Kiểm tra sự tồn tại của các đối tượng (PUBLIC CHO INTERFACE)
        public async Task<bool> DesignCommentExistsAsync(string id)
        {
            return await _context.DesignComments.AnyAsync(e => e.CommentID == id);
        }

        public async Task<bool> DesignExistsAsync(string id)
        {
            return await _context.Designs.AnyAsync(e => e.DesignID == id);
        }

        public async Task<bool> AccountExistsAsync(string id)
        {
            return await _context.Accounts.AnyAsync(e => e.AccountID == id);
        }
    }
}
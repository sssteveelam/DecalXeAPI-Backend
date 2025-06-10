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

namespace DecalXeAPI.Services.Implementations
{
    public class DesignService : IDesignService // <-- Kế thừa từ IDesignService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DesignService> _logger;

        public DesignService(ApplicationDbContext context, IMapper mapper, ILogger<DesignService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<DesignDto>> GetDesignsAsync()
        {
            _logger.LogInformation("Lấy danh sách thiết kế.");
            var designs = await _context.Designs
                                        .Include(d => d.Order)
                                        .Include(d => d.Designer)
                                        .ToListAsync();
            var designDtos = _mapper.Map<List<DesignDto>>(designs);
            return designDtos;
        }

        public async Task<DesignDto?> GetDesignByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy thiết kế với ID: {DesignID}", id);
            var design = await _context.Designs
                                        .Include(d => d.Order)
                                        .Include(d => d.Designer)
                                        .FirstOrDefaultAsync(d => d.DesignID == id);

            if (design == null)
            {
                _logger.LogWarning("Không tìm thấy thiết kế với ID: {DesignID}", id);
                return null;
            }

            var designDto = _mapper.Map<DesignDto>(design);
            _logger.LogInformation("Đã trả về thiết kế với ID: {DesignID}", id);
            return designDto;
        }

        public async Task<DesignDto> CreateDesignAsync(Design design)
        {
            _logger.LogInformation("Yêu cầu tạo thiết kế mới cho OrderID: {OrderID}", design.OrderID);

            // Kiểm tra FKs (Service sẽ kiểm tra lại để đảm bảo tính toàn vẹn)
            if (!string.IsNullOrEmpty(design.OrderID) && !await OrderExistsAsync(design.OrderID))
            {
                _logger.LogWarning("OrderID không tồn tại khi tạo thiết kế: {OrderID}", design.OrderID);
                throw new ArgumentException("OrderID không tồn tại."); // Ném lỗi để Controller bắt
            }
            if (!string.IsNullOrEmpty(design.DesignerID) && !await EmployeeExistsAsync(design.DesignerID))
            {
                _logger.LogWarning("DesignerID không tồn tại khi tạo thiết kế: {DesignerID}", design.DesignerID);
                throw new ArgumentException("DesignerID không tồn tại.");
            }

            _context.Designs.Add(design);
            await _context.SaveChangesAsync();

            // Tải lại thông tin liên quan để AutoMapper có thể ánh xạ đầy đủ
            await _context.Entry(design).Reference(d => d.Order).LoadAsync();
            await _context.Entry(design).Reference(d => d.Designer).LoadAsync();

            var designDto = _mapper.Map<DesignDto>(design);
            _logger.LogInformation("Đã tạo thiết kế mới với ID: {DesignID}", design.DesignID);
            return designDto;
        }

        public async Task<bool> UpdateDesignAsync(string id, Design design)
        {
            _logger.LogInformation("Yêu cầu cập nhật thiết kế với ID: {DesignID}", id);

            if (id != design.DesignID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với DesignID trong body ({DesignIDBody})", id, design.DesignID);
                return false;
            }

            if (!await DesignExistsAsync(id))
            {
                _logger.LogWarning("Không tìm thấy thiết kế để cập nhật với ID: {DesignID}", id);
                return false;
            }

            // Kiểm tra FKs
            if (!string.IsNullOrEmpty(design.OrderID) && !await OrderExistsAsync(design.OrderID))
            {
                _logger.LogWarning("OrderID không tồn tại khi cập nhật thiết kế: {OrderID}", design.OrderID);
                throw new ArgumentException("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(design.DesignerID) && !await EmployeeExistsAsync(design.DesignerID))
            {
                _logger.LogWarning("DesignerID không tồn tại khi cập nhật thiết kế: {DesignerID}", design.DesignerID);
                throw new ArgumentException("DesignerID không tồn tại.");
            }

            _context.Entry(design).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật thiết kế với ID: {DesignID}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật thiết kế với ID: {DesignID}", id);
                throw; // Ném lại để Controller bắt
            }
        }

        public async Task<bool> DeleteDesignAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa thiết kế với ID: {DesignID}", id);
            var design = await _context.Designs.FindAsync(id);
            if (design == null)
            {
                _logger.LogWarning("Không tìm thấy thiết kế để xóa với ID: {DesignID}", id);
                return false;
            }

            _context.Designs.Remove(design);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa thiết kế với ID: {DesignID}", id);
            return true;
        }

        // --- HÀM HỖ TRỢ: KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (PUBLIC CHO INTERFACE) ---
        public async Task<bool> DesignExistsAsync(string id)
        {
            return await _context.Designs.AnyAsync(e => e.DesignID == id);
        }

        public async Task<bool> OrderExistsAsync(string id)
        {
            return await _context.Orders.AnyAsync(e => e.OrderID == id);
        }

        public async Task<bool> EmployeeExistsAsync(string id)
        {
            return await _context.Employees.AnyAsync(e => e.EmployeeID == id);
        }
    }
}
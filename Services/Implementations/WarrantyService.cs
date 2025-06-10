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
    public class WarrantyService : IWarrantyService // <-- Kế thừa từ IWarrantyService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<WarrantyService> _logger;

        public WarrantyService(ApplicationDbContext context, IMapper mapper, ILogger<WarrantyService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<WarrantyDto>> GetWarrantiesAsync()
        {
            _logger.LogInformation("Lấy danh sách bảo hành.");
            var warranties = await _context.Warranties.Include(w => w.Order).ToListAsync();
            var warrantyDtos = _mapper.Map<List<WarrantyDto>>(warranties);
            return warrantyDtos;
        }

        public async Task<WarrantyDto?> GetWarrantyByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy bảo hành với ID: {WarrantyID}", id);
            var warranty = await _context.Warranties.Include(w => w.Order).FirstOrDefaultAsync(w => w.WarrantyID == id);

            if (warranty == null)
            {
                _logger.LogWarning("Không tìm thấy bảo hành với ID: {WarrantyID}", id);
                return null;
            }

            var warrantyDto = _mapper.Map<WarrantyDto>(warranty);
            _logger.LogInformation("Đã trả về bảo hành với ID: {WarrantyID}", id);
            return warrantyDto;
        }

        public async Task<WarrantyDto> CreateWarrantyAsync(Warranty warranty)
        {
            _logger.LogInformation("Yêu cầu tạo bảo hành mới cho OrderID: {OrderID}", warranty.OrderID);

            // Kiểm tra FKs
            if (!string.IsNullOrEmpty(warranty.OrderID) && !await OrderExistsAsync(warranty.OrderID))
            {
                _logger.LogWarning("OrderID không tồn tại khi tạo Warranty: {OrderID}", warranty.OrderID);
                throw new ArgumentException("OrderID không tồn tại.");
            }

            _context.Warranties.Add(warranty);
            await _context.SaveChangesAsync();

            // Tải lại thông tin liên quan
            await _context.Entry(warranty).Reference(w => w.Order).LoadAsync();

            var warrantyDto = _mapper.Map<WarrantyDto>(warranty);
            _logger.LogInformation("Đã tạo bảo hành mới với ID: {WarrantyID}", warranty.WarrantyID);
            return warrantyDto;
        }

        public async Task<bool> UpdateWarrantyAsync(string id, Warranty warranty)
        {
            _logger.LogInformation("Yêu cầu cập nhật bảo hành với ID: {WarrantyID}", id);

            if (id != warranty.WarrantyID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với WarrantyID trong body ({WarrantyIDBody})", id, warranty.WarrantyID);
                return false;
            }

            if (!await WarrantyExistsAsync(id))
            {
                _logger.LogWarning("Không tìm thấy bảo hành để cập nhật với ID: {WarrantyID}", id);
                return false;
            }

            // Kiểm tra FKs
            if (!string.IsNullOrEmpty(warranty.OrderID) && !await OrderExistsAsync(warranty.OrderID))
            {
                _logger.LogWarning("OrderID không tồn tại khi cập nhật Warranty: {OrderID}", warranty.OrderID);
                throw new ArgumentException("OrderID không tồn tại.");
            }

            _context.Entry(warranty).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật bảo hành với ID: {WarrantyID}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật bảo hành với ID: {WarrantyID}", id);
                throw;
            }
        }

        public async Task<bool> DeleteWarrantyAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa bảo hành với ID: {WarrantyID}", id);
            var warranty = await _context.Warranties.FindAsync(id);
            if (warranty == null)
            {
                _logger.LogWarning("Không tìm thấy bảo hành để xóa với ID: {WarrantyID}", id);
                return false;
            }

            _context.Warranties.Remove(warranty);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa bảo hành với ID: {WarrantyID}", id);
            return true;
        }

        // --- HÀM HỖ TRỢ: KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (PUBLIC CHO INTERFACE) ---
        public async Task<bool> WarrantyExistsAsync(string id)
        {
            return await _context.Warranties.AnyAsync(e => e.WarrantyID == id);
        }

        public async Task<bool> OrderExistsAsync(string id)
        {
            return await _context.Orders.AnyAsync(e => e.OrderID == id);
        }
    }
}
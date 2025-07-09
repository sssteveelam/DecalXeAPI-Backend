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
    public class StoreService : IStoreService // <-- Kế thừa từ IStoreService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<StoreService> _logger;

        public StoreService(ApplicationDbContext context, IMapper mapper, ILogger<StoreService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<StoreDto>> GetStoresAsync()
        {
            _logger.LogInformation("Lấy danh sách cửa hàng.");
            var stores = await _context.Stores.ToListAsync();
            var storeDtos = _mapper.Map<List<StoreDto>>(stores);
            return storeDtos;
        }

        public async Task<StoreDto?> GetStoreByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy cửa hàng với ID: {StoreID}", id);
            var store = await _context.Stores.FindAsync(id);

            if (store == null)
            {
                _logger.LogWarning("Không tìm thấy cửa hàng với ID: {StoreID}", id);
                return null;
            }

            var storeDto = _mapper.Map<StoreDto>(store);
            _logger.LogInformation("Đã trả về cửa hàng với ID: {StoreID}", id);
            return storeDto;
        }

        public async Task<StoreDto> CreateStoreAsync(Store store)
        {
            _logger.LogInformation("Yêu cầu tạo cửa hàng mới: {StoreName}", store.StoreName);
            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            var storeDto = _mapper.Map<StoreDto>(store);
            _logger.LogInformation("Đã tạo cửa hàng mới với ID: {StoreID}", store.StoreID);
            return storeDto;
        }

        public async Task<bool> UpdateStoreAsync(string id, Store store)
        {
            _logger.LogInformation("Yêu cầu cập nhật cửa hàng với ID: {StoreID}", id);

            if (id != store.StoreID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với StoreID trong body ({StoreIDBody})", id, store.StoreID);
                return false;
            }

            if (!await StoreExistsAsync(id))
            {
                _logger.LogWarning("Không tìm thấy cửa hàng để cập nhật với ID: {StoreID}", id);
                return false;
            }

            _context.Entry(store).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật cửa hàng với ID: {StoreID}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật cửa hàng với ID: {StoreID}", id);
                throw;
            }
        }

        // Trong file: DecalXeAPI/Services/Implementations/StoreService.cs
        public async Task<bool> DeleteStoreAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa cửa hàng với ID: {StoreID}", id);
            var store = await _context.Stores.FindAsync(id);
            if (store == null)
            {
                _logger.LogWarning("Không tìm thấy cửa hàng để xóa với ID: {StoreID}", id);
                return false;
            }

            // --- LOGIC MỚI: KIỂM TRA ĐƠN HÀNG ĐANG HOẠT ĐỘNG ---
            // Kiểm tra xem có bất kỳ nhân viên nào trong cửa hàng này
            // đang được giao một đơn hàng chưa hoàn thành hoặc chưa bị hủy không.
            bool hasActiveOrders = await _context.Orders
                .AnyAsync(order => 
                    order.AssignedEmployee != null &&
                    order.AssignedEmployee.StoreID == id &&
                    order.OrderStatus != "Completed" &&
                    order.OrderStatus != "Cancelled"
                );

            if (hasActiveOrders)
            {
                _logger.LogWarning("Không thể xóa cửa hàng {StoreID} vì vẫn còn các đơn hàng đang hoạt động.", id);
                throw new InvalidOperationException("Không thể xóa cửa hàng này vì vẫn còn các đơn hàng đang hoạt động được xử lý tại đây.");
            }
            // --- KẾT THÚC LOGIC MỚI ---

            _context.Stores.Remove(store);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa cửa hàng với ID: {StoreID}", id);
            return true;
        }

        // Hàm kiểm tra tồn tại (PUBLIC cho INTERFACE)
        public async Task<bool> StoreExistsAsync(string id)
        {
            return await _context.Stores.AnyAsync(e => e.StoreID == id);
        }
    }
}
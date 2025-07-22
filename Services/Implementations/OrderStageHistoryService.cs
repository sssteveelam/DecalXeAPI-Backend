using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces;
using DecalXeAPI.Services.Helpers;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace DecalXeAPI.Services.Implementations
{
    public class OrderStageHistoryService : IOrderStageHistoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderStageHistoryService> _logger;

        public OrderStageHistoryService(ApplicationDbContext context, IMapper mapper, ILogger<OrderStageHistoryService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<OrderStageHistoryDto>> GetAllAsync()
        {
            _logger.LogInformation("Lấy danh sách tất cả lịch sử giai đoạn đơn hàng.");
            var histories = await _context.OrderStageHistories
                .Include(osh => osh.Order)
                .Include(osh => osh.ChangedByEmployee)
                .OrderBy(osh => osh.ChangeDate)
                .ToListAsync();
            
            return histories.Select(MapToDto);
        }

        public async Task<OrderStageHistoryDto?> GetByIdAsync(string id)
        {
            _logger.LogInformation("Lấy lịch sử giai đoạn với ID: {HistoryID}", id);
            var history = await _context.OrderStageHistories
                .Include(osh => osh.Order)
                .Include(osh => osh.ChangedByEmployee)
                .FirstOrDefaultAsync(osh => osh.OrderStageHistoryID == id);

            if (history == null)
            {
                _logger.LogWarning("Không tìm thấy lịch sử giai đoạn với ID: {HistoryID}", id);
                return null;
            }

            return MapToDto(history);
        }

        public async Task<IEnumerable<OrderStageHistoryDto>> GetByOrderIdAsync(string orderId)
        {
            _logger.LogInformation("Lấy lịch sử giai đoạn cho đơn hàng: {OrderID}", orderId);
            var histories = await _context.OrderStageHistories
                .Include(osh => osh.Order)
                .Include(osh => osh.ChangedByEmployee)
                .Where(osh => osh.OrderID == orderId)
                .OrderBy(osh => osh.ChangeDate)
                .ToListAsync();

            return histories.Select(MapToDto);
        }

        public async Task<IEnumerable<OrderStageHistoryDto>> GetByStageAsync(OrderStage stage)
        {
            _logger.LogInformation("Lấy lịch sử giai đoạn theo stage: {Stage}", stage);
            var histories = await _context.OrderStageHistories
                .Include(osh => osh.Order)
                .Include(osh => osh.ChangedByEmployee)
                .Where(osh => osh.Stage == stage)
                .OrderBy(osh => osh.ChangeDate)
                .ToListAsync();

            return histories.Select(MapToDto);
        }

        public async Task<OrderStageHistoryDto?> GetLatestByOrderIdAsync(string orderId)
        {
            _logger.LogInformation("Lấy lịch sử giai đoạn mới nhất cho đơn hàng: {OrderID}", orderId);
            var latestHistory = await _context.OrderStageHistories
                .Include(osh => osh.Order)
                .Include(osh => osh.ChangedByEmployee)
                .Where(osh => osh.OrderID == orderId)
                .OrderByDescending(osh => osh.ChangeDate)
                .FirstOrDefaultAsync();

            if (latestHistory == null)
            {
                _logger.LogWarning("Không tìm thấy lịch sử giai đoạn cho đơn hàng: {OrderID}", orderId);
                return null;
            }

            return MapToDto(latestHistory);
        }

        public async Task<OrderStageHistoryDto> CreateAsync(CreateOrderStageHistoryDto createDto)
        {
            _logger.LogInformation("Tạo lịch sử giai đoạn mới cho đơn hàng: {OrderID}", createDto.OrderID);
            
            // Kiểm tra đơn hàng có tồn tại không
            if (!await OrderExistsAsync(createDto.OrderID))
            {
                throw new InvalidOperationException($"Đơn hàng với ID {createDto.OrderID} không tồn tại.");
            }

            var history = _mapper.Map<OrderStageHistory>(createDto);
            history.OrderStageHistoryID = Guid.NewGuid().ToString();
            history.ChangeDate = DateTime.UtcNow;

            _context.OrderStageHistories.Add(history);
            await _context.SaveChangesAsync();

            // Lấy lại với thông tin đầy đủ
            var createdHistory = await _context.OrderStageHistories
                .Include(osh => osh.Order)
                .Include(osh => osh.ChangedByEmployee)
                .FirstOrDefaultAsync(osh => osh.OrderStageHistoryID == history.OrderStageHistoryID);

            _logger.LogInformation("Đã tạo lịch sử giai đoạn với ID: {HistoryID}", history.OrderStageHistoryID);
            return MapToDto(createdHistory!);
        }

        public async Task<OrderStageHistoryDto?> UpdateAsync(string id, UpdateOrderStageHistoryDto updateDto)
        {
            _logger.LogInformation("Cập nhật lịch sử giai đoạn với ID: {HistoryID}", id);
            var history = await _context.OrderStageHistories.FindAsync(id);

            if (history == null)
            {
                _logger.LogWarning("Không tìm thấy lịch sử giai đoạn với ID: {HistoryID}", id);
                return null;
            }

            // Map các thuộc tính không null
            if (!string.IsNullOrEmpty(updateDto.StageName))
                history.StageName = updateDto.StageName;
            
            if (!string.IsNullOrEmpty(updateDto.ChangedByEmployeeID))
                history.ChangedByEmployeeID = updateDto.ChangedByEmployeeID;
            
            if (updateDto.Notes != null)
                history.Notes = updateDto.Notes;
            
            if (updateDto.Stage.HasValue)
                history.Stage = updateDto.Stage.Value;

            await _context.SaveChangesAsync();

            // Lấy lại với thông tin đầy đủ
            var updatedHistory = await _context.OrderStageHistories
                .Include(osh => osh.Order)
                .Include(osh => osh.ChangedByEmployee)
                .FirstOrDefaultAsync(osh => osh.OrderStageHistoryID == id);

            _logger.LogInformation("Đã cập nhật lịch sử giai đoạn với ID: {HistoryID}", id);
            return MapToDto(updatedHistory!);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            _logger.LogInformation("Xóa lịch sử giai đoạn với ID: {HistoryID}", id);
            var history = await _context.OrderStageHistories.FindAsync(id);

            if (history == null)
            {
                _logger.LogWarning("Không tìm thấy lịch sử giai đoạn với ID: {HistoryID}", id);
                return false;
            }

            _context.OrderStageHistories.Remove(history);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Đã xóa lịch sử giai đoạn với ID: {HistoryID}", id);
            return true;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.OrderStageHistories.AnyAsync(osh => osh.OrderStageHistoryID == id);
        }

        public async Task<bool> OrderExistsAsync(string orderId)
        {
            return await _context.Orders.AnyAsync(o => o.OrderID == orderId);
        }

        public async Task<bool> CanTransitionToStageAsync(string orderId, OrderStage newStage)
        {
            var currentStage = await GetCurrentStageAsync(orderId);
            
            if (currentStage == null)
            {
                // Nếu chưa có lịch sử, chỉ cho phép bắt đầu từ Survey
                return newStage == OrderStage.Survey;
            }

            return OrderStageHelper.CanTransitionTo(currentStage.Value, newStage);
        }

        public async Task<OrderStageHistoryDto> TransitionToNextStageAsync(string orderId, string? employeeId = null, string? notes = null)
        {
            _logger.LogInformation("Chuyển đơn hàng {OrderID} sang giai đoạn tiếp theo", orderId);
            
            var currentStage = await GetCurrentStageAsync(orderId);
            OrderStage nextStage;

            if (currentStage == null)
            {
                // Nếu chưa có lịch sử, bắt đầu từ Survey
                nextStage = OrderStage.Survey;
            }
            else
            {
                var next = OrderStageHelper.GetNextStage(currentStage.Value);
                if (next == null)
                {
                    throw new InvalidOperationException("Đơn hàng đã ở giai đoạn cuối cùng.");
                }
                nextStage = next.Value;
            }

            var createDto = new CreateOrderStageHistoryDto
            {
                OrderID = orderId,
                Stage = nextStage,
                StageName = OrderStageHelper.GetDescription(nextStage),
                ChangedByEmployeeID = employeeId,
                Notes = notes
            };

            return await CreateAsync(createDto);
        }

        public async Task<OrderStage?> GetCurrentStageAsync(string orderId)
        {
            var latestHistory = await _context.OrderStageHistories
                .Where(osh => osh.OrderID == orderId)
                .OrderByDescending(osh => osh.ChangeDate)
                .FirstOrDefaultAsync();

            return latestHistory?.Stage;
        }

        private OrderStageHistoryDto MapToDto(OrderStageHistory history)
        {
            var dto = _mapper.Map<OrderStageHistoryDto>(history);
            dto.StageDescription = OrderStageHelper.GetDescription(history.Stage);
            dto.CompletionPercentage = OrderStageHelper.GetCompletionPercentage(history.Stage);
            return dto;
        }
    }
}

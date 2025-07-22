// DecalXeAPI/Services/Interfaces/IOrderStageHistoryService.cs
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IOrderStageHistoryService
    {
        Task<IEnumerable<OrderStageHistoryDto>> GetAllAsync();
        Task<OrderStageHistoryDto?> GetByIdAsync(string id);
        Task<IEnumerable<OrderStageHistoryDto>> GetByOrderIdAsync(string orderId);
        Task<IEnumerable<OrderStageHistoryDto>> GetByStageAsync(OrderStage stage);
        Task<OrderStageHistoryDto?> GetLatestByOrderIdAsync(string orderId);
        Task<OrderStageHistoryDto> CreateAsync(CreateOrderStageHistoryDto createDto);
        Task<OrderStageHistoryDto?> UpdateAsync(string id, UpdateOrderStageHistoryDto updateDto);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task<bool> OrderExistsAsync(string orderId);
        Task<bool> CanTransitionToStageAsync(string orderId, OrderStage newStage);
        Task<OrderStageHistoryDto> TransitionToNextStageAsync(string orderId, string? employeeId = null, string? notes = null);
        Task<OrderStage?> GetCurrentStageAsync(string orderId);
    }
}

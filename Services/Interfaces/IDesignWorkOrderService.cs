// DecalXeAPI/Services/Interfaces/IDesignWorkOrderService.cs
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IDesignWorkOrderService
    {
        Task<DesignWorkOrderDto?> GetWorkOrderByIdAsync(string workOrderId);
        Task<DesignWorkOrderDto?> GetWorkOrderByDesignIdAsync(string designId);
        Task<DesignWorkOrderDto> CreateWorkOrderAsync(DesignWorkOrder workOrder);
        Task<DesignWorkOrderDto?> UpdateWorkOrderAsync(string workOrderId, DesignWorkOrder workOrder);
    }
}
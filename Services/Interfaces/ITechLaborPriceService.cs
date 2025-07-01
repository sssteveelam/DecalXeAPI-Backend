// DecalXeAPI/Services/Interfaces/ITechLaborPriceService.cs
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface ITechLaborPriceService
    {
        Task<List<TechLaborPriceDto>> GetAllAsync();
        Task<TechLaborPriceDto?> GetByIdAsync(string serviceId, string vehicleModelId);
        Task<TechLaborPriceDto> CreateAsync(TechLaborPrice techLaborPrice);
        Task<TechLaborPriceDto?> UpdateAsync(string serviceId, string vehicleModelId, decimal newPrice);
        Task<bool> DeleteAsync(string serviceId, string vehicleModelId);
    }
}
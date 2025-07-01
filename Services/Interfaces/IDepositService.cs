// DecalXeAPI/Services/Interfaces/IDepositService.cs
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IDepositService
    {
        Task<List<DepositDto>> GetDepositsByOrderIdAsync(string orderId);
        Task<DepositDto?> GetDepositByIdAsync(string depositId);
        Task<DepositDto> CreateDepositAsync(Deposit deposit);
    }
}
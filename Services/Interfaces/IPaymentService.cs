using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentDto>> GetPaymentsAsync();
        Task<PaymentDto?> GetPaymentByIdAsync(string id);
        Task<PaymentDto> CreatePaymentAsync(Payment payment);
        Task<bool> UpdatePaymentAsync(string id, Payment payment);
        Task<bool> DeletePaymentAsync(string id);

        // Các hàm kiểm tra tồn tại (Exists) cần thiết cho Service này
        Task<bool> PaymentExistsAsync(string id);
        Task<bool> OrderExistsAsync(string id); // Cần để kiểm tra FK
    }
}
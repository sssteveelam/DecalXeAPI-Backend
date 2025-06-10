using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface ICustomServiceRequestService
    {
        Task<IEnumerable<CustomServiceRequestDto>> GetCustomServiceRequestsAsync();
        Task<CustomServiceRequestDto?> GetCustomServiceRequestByIdAsync(string id);
        Task<(CustomServiceRequestDto? CustomServiceRequest, string? ErrorMessage)> CreateCustomServiceRequestAsync(CreateCustomServiceRequestDto createDto);
        Task<bool> UpdateCustomServiceRequestAsync(string id, CustomServiceRequest customServiceRequest);
        Task<bool> DeleteCustomServiceRequestAsync(string id);
        Task<(OrderDto? Order, string? ErrorMessage)> ConvertToOrderAsync(string id, ConvertCsrToOrderDto convertDto);

        // Các hàm kiểm tra tồn tại (Exists) cũng sẽ được chuyển vào Service nếu không dùng chung
        // Hiện tại, để đơn giản, mình giữ các hàm Exists này trong Controller,
        // hoặc có thể tạo một BaseService chung cho các hàm kiểm tra này.
        // Tuy nhiên, logic kiểm tra FKs nên nằm ở Service để đảm bảo tính toàn vẹn.
        // Do đó, mình sẽ chuyển các hàm Exists sang Service này.
        Task<bool> CustomerExistsAsync(string id);
        Task<bool> EmployeeExistsAsync(string id);
        Task<bool> OrderExistsAsync(string id);
        Task<bool> DecalServiceExistsAsync(string id);
        Task<bool> CustomServiceRequestExistsAsync(string id); // <-- THÊM DÒNG NÀY

    }
}
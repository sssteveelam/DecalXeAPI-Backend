using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IPrintingPriceDetailService
    {
        Task<IEnumerable<PrintingPriceDetailDto>> GetPrintingPriceDetailsAsync();
        Task<PrintingPriceDetailDto?> GetPrintingPriceDetailByIdAsync(string serviceId); // Dùng ServiceID vì nó là PK
        Task<PrintingPriceDetailDto> CreatePrintingPriceDetailAsync(PrintingPriceDetail printingPriceDetail);
        Task<bool> UpdatePrintingPriceDetailAsync(string serviceId, PrintingPriceDetail printingPriceDetail);
        Task<bool> DeletePrintingPriceDetailAsync(string serviceId);

        // Hàm kiểm tra tồn tại (Exists)
        Task<bool> PrintingPriceDetailExistsAsync(string serviceId);
        Task<bool> DecalServiceExistsAsync(string serviceId); // Cần để kiểm tra FK
    }
}
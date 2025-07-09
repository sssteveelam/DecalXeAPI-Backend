// DecalXeAPI/Services/Interfaces/IVehicleModelService.cs
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IVehicleModelService
    {
        // --- Các phương thức quản lý VehicleModel (giữ nguyên) ---
        Task<IEnumerable<VehicleModelDto>> GetAllModelsAsync();
        Task<VehicleModelDto?> GetModelByIdAsync(string id);
        Task<(VehicleModelDto?, string?)> CreateModelAsync(VehicleModel model);
        Task<(bool, string?)> UpdateModelAsync(string id, VehicleModel model);
        Task<bool> DeleteModelAsync(string id);

        // --- CÁC PHƯƠNG THỨC QUẢN LÝ DECALTYPE TƯƠNG THÍCH (ĐÃ NÂNG CẤP) ---

        /// <summary>
        /// Gán một loại decal là tương thích với một mẫu xe, đi kèm với giá tiền.
        /// </summary>
        Task<(VehicleModelDecalTypeDto? CreatedLink, string? ErrorMessage)> AssignDecalTypeToVehicleAsync(string modelId, string decalTypeId, decimal price);

        /// <summary>
        /// Gỡ (xóa) liên kết tương thích giữa một loại decal và một mẫu xe.
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> UnassignDecalTypeFromVehicleAsync(string modelId, string decalTypeId);

        /// <summary>
        /// Lấy danh sách các loại decal tương thích với một mẫu xe cụ thể, bao gồm cả giá.
        /// </summary>
        Task<IEnumerable<VehicleModelDecalTypeDto>> GetCompatibleDecalTypesAsync(string modelId);
        
        /// <summary>
        /// Cập nhật giá cho một liên kết tương thích đã có.
        /// </summary>
        Task<(VehicleModelDecalTypeDto? UpdatedLink, string? ErrorMessage)> UpdateVehicleDecalTypePriceAsync(string modelId, string decalTypeId, decimal newPrice);
    }
}
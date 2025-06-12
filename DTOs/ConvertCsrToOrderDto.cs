// File: DTOs/ConvertCsrToOrderDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class ConvertCsrToOrderDto
    {
        [Required(ErrorMessage = "AssignedEmployeeID là bắt buộc.")]
        public string AssignedEmployeeID { get; set; } = string.Empty;

        [Required(ErrorMessage = "EstimatedCost là bắt buộc.")]
        public decimal EstimatedCost { get; set; }

        [Required(ErrorMessage = "EstimatedWorkUnits là bắt buộc.")]
        public int EstimatedWorkUnits { get; set; }

        [Required(ErrorMessage = "CustomServiceServiceID là bắt buộc.")]
        public string CustomServiceServiceID { get; set; } = string.Empty;

        public bool IsCustomDecal { get; set; } = true; // <-- MỚI: Đánh dấu luôn là CustomDecal khi chuyển đổi
    }
}
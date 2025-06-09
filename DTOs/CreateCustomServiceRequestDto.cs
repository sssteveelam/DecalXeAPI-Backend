using System.ComponentModel.DataAnnotations;
using System;

namespace DecalXeAPI.DTOs
{
    public class CreateCustomServiceRequestDto
    {
        [Required(ErrorMessage = "CustomerID là bắt buộc.")]
        public string CustomerID { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả yêu cầu là bắt buộc.")]
        [MaxLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự.")]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "URL hình ảnh không được vượt quá 500 ký tự.")]
        public string? ReferenceImageURL { get; set; }

        public DateTime? DesiredCompletionDate { get; set; }

        // SalesEmployeeID có thể được gán sau bởi backend hoặc là tùy chọn
        public string? SalesEmployeeID { get; set; }

        // OrderID và RequestStatus sẽ được gán tự động bởi backend ở bước này
        // EstimatedCost và EstimatedWorkUnits sẽ được cập nhật sau bởi Sales/Designer
    }
}
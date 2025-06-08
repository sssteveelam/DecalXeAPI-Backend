using System;

namespace DecalXeAPI.DTOs
{
    public class CustomServiceRequestDto
    {
        public string CustomRequestID { get; set; } = string.Empty;
        public string CustomerID { get; set; } = string.Empty;
        public string CustomerFullName { get; set; } = string.Empty; // Tên đầy đủ của khách hàng
        public DateTime RequestDate { get; set; }
        public string Description { get; set; } = string.Empty; // Đây là Description gốc
        public string? ReferenceImageURL { get; set; }
        public DateTime? DesiredCompletionDate { get; set; }
        public string RequestStatus { get; set; } = string.Empty;
        public decimal? EstimatedCost { get; set; }
        public int? EstimatedWorkUnits { get; set; }
        public string? SalesEmployeeID { get; set; }
        public string? SalesEmployeeFullName { get; set; } // Tên đầy đủ của nhân viên Sales
        public string? OrderID { get; set; } // <-- THÊM DÒNG NÀY
        public string? CustomServiceRequestDescription { get; set; } // <-- THÊM DÒNG NÀY (đây là field để hiển thị mô tả gọn hơn nếu cần)
    }
}
using System;

namespace DecalXeAPI.DTOs
{
    public class OrderDto
    {
        public string OrderID { get; set; } = string.Empty;
        public string CustomerID { get; set; } = string.Empty;
        public string CustomerFullName { get; set; } = string.Empty; // Tên đầy đủ khách hàng
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string? AssignedEmployeeID { get; set; }
        public string? AssignedEmployeeFullName { get; set; } // Tên đầy đủ nhân viên được giao
        public string? CustomServiceRequestID { get; set; } // ID của yêu cầu tùy chỉnh nếu có
        public string? CustomServiceRequestDescription { get; set; } // Mô tả yêu cầu tùy chỉnh

        // --- CỘT VÀ THUỘC TÍNH MỚI TỪ YÊU CẦU REVIEW ---
        public string? VehicleID { get; set; } // ID xe liên kết
        public string? LicensePlate { get; set; } // Biển số xe liên kết
        public string? CarModelName { get; set; } // Tên mẫu xe liên kết
        public string? CarBrandName { get; set; } // Tên hãng xe liên kết

        public DateTime? ExpectedArrivalTime { get; set; } // Thời gian dự kiến đến
        public string CurrentStage { get; set; } = string.Empty; // Giai đoạn hiện tại của đơn hàng
        public string? Priority { get; set; } // Độ ưu tiên
        public bool IsCustomDecal { get; set; } // <-- MỚI: Đánh dấu đây có phải đơn hàng decal tùy chỉnh không

    }
}
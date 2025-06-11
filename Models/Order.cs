using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization; // Để dùng [JsonIgnore]

namespace DecalXeAPI.Models
{
    public class Order
    {
        [Key]
        public string OrderID { get; set; } = Guid.NewGuid().ToString(); // PK

        // Khóa ngoại (Foreign Key): Một Order thuộc về một Customer
        [ForeignKey("Customer")]
        public string CustomerID { get; set; } = string.Empty; // FK_CustomerID
        public Customer? Customer { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow; // Ngày đặt hàng, dùng UtcNow cho chuẩn múi giờ

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(50)]
        public string OrderStatus { get; set; } = string.Empty; // Trạng thái đơn hàng (ví dụ: "New", "Pending", "Completed", "Cancelled")

        // Khóa ngoại (Foreign Key): Nhân viên được giao phụ trách đơn hàng này (ví dụ: sales staff)
        [ForeignKey("Employee")]
        public string? AssignedEmployeeID { get; set; } // FK_AssignedEmployeeID (có thể null ban đầu)
        public Employee? AssignedEmployee { get; set; }

        // Mối quan hệ 1-1 với CustomServiceRequest
        [JsonIgnore] // Tránh lỗi vòng lặp JSON khi include Order từ CustomServiceRequest
        public CustomServiceRequest? CustomServiceRequest { get; set; } // Navigation Property cho yêu cầu tùy chỉnh

        // --- CỘT VÀ NAVIGATION PROPERTIES MỚI TỪ YÊU CẦU REVIEW ---
        [ForeignKey("CustomerVehicle")] // Khóa ngoại tới xe của khách hàng
        public string? VehicleID { get; set; } // Xe được dán decal (Nullable)
        public CustomerVehicle? CustomerVehicle { get; set; } // Navigation Property

        public DateTime? ExpectedArrivalTime { get; set; } // Thời gian dự kiến khách hàng đến (Nullable)

        [Required] // Bắt buộc phải có giai đoạn hiện tại
        [MaxLength(50)]
        public string CurrentStage { get; set; } = "New Profile"; // Giai đoạn hiện tại của đơn hàng

        [MaxLength(50)]
        public string? Priority { get; set; } // Độ ưu tiên (ví dụ: "Low", "Medium", "High")

        // Navigation Properties cho các bảng mới được tạo
        [JsonIgnore] // Để tránh lỗi vòng lặp JSON
        public ICollection<OrderStageHistory>? OrderStageHistories { get; set; } // Lịch sử các giai đoạn của Order
        [JsonIgnore] // Để tránh lỗi vòng lặp JSON
        public ICollection<OrderCompletionImage>? OrderCompletionImages { get; set; } // Ảnh sau khi hoàn tất

        // --- NAVIGATION PROPERTIES HIỆN CÓ (Giữ nguyên) ---
        [JsonIgnore] // Để tránh lỗi vòng lặp JSON
        public ICollection<OrderDetail>? OrderDetails { get; set; }
        [JsonIgnore] // Để tránh lỗi vòng lặp JSON
        public ICollection<Payment>? Payments { get; set; }
        [JsonIgnore] // Để tránh lỗi vòng lặp JSON
        public ICollection<ScheduledWorkUnit>? ScheduledWorkUnits { get; set; }
        [JsonIgnore] // Để tránh lỗi vòng lặp JSON
        public ICollection<Design>? Designs { get; set; }
        [JsonIgnore] // Để tránh lỗi vòng lặp JSON
        public ICollection<Feedback>? Feedbacks { get; set; }
        [JsonIgnore] // Để tránh lỗi vòng lặp JSON
        public ICollection<Warranty>? Warranties { get; set; }
    }
}
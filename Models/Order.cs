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

        [ForeignKey("Customer")]
        public string CustomerID { get; set; } = string.Empty; // FK_CustomerID
        public Customer? Customer { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(50)]
        public string OrderStatus { get; set; } = string.Empty;

        [ForeignKey("Employee")]
        public string? AssignedEmployeeID { get; set; }
        public Employee? AssignedEmployee { get; set; }

        [JsonIgnore]
        public CustomServiceRequest? CustomServiceRequest { get; set; }

        // --- CỘT VÀ NAVIGATION PROPERTIES TỪ REVIEW1/REVIEW2 ---
        [ForeignKey("CustomerVehicle")]
        public string? VehicleID { get; set; } // FK tới xe cụ thể của khách hàng
        public CustomerVehicle? CustomerVehicle { get; set; }

        public DateTime? ExpectedArrivalTime { get; set; }

        [Required]
        [MaxLength(50)]
        public string CurrentStage { get; set; } = "New Profile";

        [MaxLength(50)]
        public string? Priority { get; set; }

        public bool IsCustomDecal { get; set; } = false;

        // MỚI TỪ REVIEW2: Thêm số khung trực tiếp vào Order
        [MaxLength(50)] // Kích thước đủ cho số khung
        public string? ChassisNumber { get; set; } // <-- MỚI: Số khung xe (trực tiếp trên Order)

        // --- CÁC NAVIGATION PROPERTIES ĐƯỢC ĐIỀU CHỈNH/XÓA THEO REVIEW2 ---
        [JsonIgnore]
        public ICollection<OrderDetail>? OrderDetails { get; set; } // Giữ lại
        
        [JsonIgnore]
        public ICollection<OrderStageHistory>? OrderStageHistories { get; set; } // Giữ lại

        [JsonIgnore]
        public ICollection<Deposit>? Deposits { get; set; } // <-- MỚI: Order có nhiều Deposit (Tiền cọc)

        // Các Navigation Property đã bị xóa vì bảng/mối quan hệ bị loại bỏ/thay đổi:
        // public ICollection<Payment>? Payments { get; set; } // ĐÃ XÓA (Payment vẫn tồn tại nhưng không còn NP trực tiếp)
        // public ICollection<ScheduledWorkUnit>? ScheduledWorkUnits { get; set; } // ĐÃ XÓA
        // public ICollection<Design>? Designs { get; set; } // ĐÃ XÓA (Design giờ không còn OrderID trực tiếp)
        // public ICollection<Feedback>? Feedbacks { get; set; } // ĐÃ XÓA
        // public ICollection<Warranty>? Warranties { get; set; } // ĐÃ XÓA
        // public ICollection<OrderCompletionImage>? OrderCompletionImages { get; set; } // ĐÃ XÓA
    }
}

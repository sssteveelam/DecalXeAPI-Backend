using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System; // Để dùng DateTime
using System.Text.Json.Serialization;


namespace DecalXeAPI.Models
{
    public class Order
    {
        [Key]
        public string OrderID { get; set; } = Guid.NewGuid().ToString(); // PK

        // Khóa ngoại (Foreign Key): Một Order thuộc về một Customer
        public string CustomerID { get; set; } = string.Empty; // FK_CustomerID

        // Navigation Property: Một Order có một Customer
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
        public string? AssignedEmployeeID { get; set; } // FK_AssignedEmployeeID (có thể null ban đầu)

        // Navigation Property: Một Order có thể có một Employee được giao
        public Employee? AssignedEmployee { get; set; }

        // Navigation Properties cho các mối quan hệ một-nhiều
        [JsonIgnore]
        public ICollection<OrderDetail>? OrderDetails { get; set; } // Một Order có nhiều OrderDetail
        [JsonIgnore]
        public ICollection<Payment>? Payments { get; set; } // Một Order có nhiều Payment
        [JsonIgnore]
        public ICollection<ScheduledWorkUnit>? ScheduledWorkUnits { get; set; } // Một Order có nhiều ScheduledWorkUnit (thời gian thi công)

        // Navigation Properties cho các mối quan hệ một-một
        public Design? Design { get; set; } // Một Order có một Design (cho dịch vụ tùy chỉnh)
        public Feedback? Feedback { get; set; } // Một Order có một Feedback (sau khi hoàn thành)

        public Warranty? Warranty { get; set; } // Một Order có một Warranty
        [JsonIgnore]
        public CustomServiceRequest? CustomServiceRequest { get; set; }

    }
}
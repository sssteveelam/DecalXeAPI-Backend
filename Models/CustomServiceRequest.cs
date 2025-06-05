using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace DecalXeAPI.Models
{
    public class CustomServiceRequest
    {
        [Key]
        public string CustomRequestID { get; set; } = Guid.NewGuid().ToString(); // PK

        // Khóa ngoại (Foreign Key): Yêu cầu này từ Customer nào
        public string CustomerID { get; set; } = string.Empty; // FK_CustomerID
        // Navigation Property: Một CustomServiceRequest có một Customer
        public Customer? Customer { get; set; }

        [Required]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow; // Ngày yêu cầu

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty; // Mô tả yêu cầu tùy chỉnh

        [MaxLength(500)]
        public string? ReferenceImageURL { get; set; } // URL hình ảnh tham khảo

        public DateTime? DesiredCompletionDate { get; set; } // Ngày khách hàng muốn hoàn thành

        [Required]
        [MaxLength(50)]
        public string RequestStatus { get; set; } = "New"; // Trạng thái yêu cầu (ví dụ: "New", "PendingEstimate", "ConvertedToOrder")

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedCost { get; set; } // Chi phí ước tính

        public int? EstimatedWorkUnits { get; set; } // Số lượng xuất công ước tính

        // Khóa ngoại (Foreign Key): Nhân viên sales nào đã tiếp nhận yêu cầu này
        public string? SalesEmployeeID { get; set; } // FK_SalesEmployeeID (có thể null nếu khách tự tạo online)
        // Navigation Property: Một CustomServiceRequest có thể có một SalesEmployee
        public Employee? SalesEmployee { get; set; }


        // Khóa ngoại (Foreign Key): Yêu cầu này có thể dẫn đến Order nào
        public string? OrderID { get; set; } // FK_OrderID (có thể null nếu chưa được chuyển thành Order)
        // Navigation Property
        public Order? Order { get; set; }


    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization; // Để dùng [JsonIgnore]

namespace DecalXeAPI.Models
{
    public class DecalService
    {
        [Key]
        public string ServiceID { get; set; } = Guid.NewGuid().ToString(); // PK

        [Required]
        [MaxLength(100)]
        public string ServiceName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int StandardWorkUnits { get; set; } // Số lượng xuất công tiêu chuẩn cho dịch vụ này

        // Khóa ngoại (Foreign Key): Một DecalService thuộc về một DecalType
        [ForeignKey("DecalType")]
        public string DecalTypeID { get; set; } = string.Empty; // FK_DecalTypeID
        public DecalType? DecalType { get; set; }


        // --- NAVIGATION PROPERTIES HIỆN CÓ (Giữ nguyên) ---
        [JsonIgnore] // Để tránh lỗi vòng lặp JSON
        public ICollection<OrderDetail>? OrderDetails { get; set; }
       
    }
}
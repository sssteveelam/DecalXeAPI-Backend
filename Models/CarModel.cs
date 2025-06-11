using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System;
using System.Text.Json.Serialization; // Để dùng [JsonIgnore]

namespace DecalXeAPI.Models
{
    public class CarModel
    {
        [Key]
        public string ModelID { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string ModelName { get; set; } = string.Empty; // Ví dụ: SH, Vision, C200, Ranger

        [MaxLength(500)]
        public string? Description { get; set; }

        // Khóa ngoại: Một mẫu xe thuộc về một hãng xe
        [ForeignKey("CarBrand")]
        public string BrandID { get; set; } = string.Empty;
        public CarBrand? CarBrand { get; set; }

        // --- NAVIGATION PROPERTIES HIỆN CÓ ---
        [JsonIgnore] // Để tránh lỗi vòng lặp JSON
        public ICollection<CustomerVehicle>? CustomerVehicles { get; set; }

        // --- NAVIGATION PROPERTY MỚI TỪ YÊU CẦU REVIEW ---
        // Mối quan hệ N-N với DecalTemplate thông qua bảng trung gian CarModelDecalTemplate
        [JsonIgnore] // Để tránh lỗi vòng lặp JSON
        public ICollection<CarModelDecalTemplate>? CarModelDecalTemplates { get; set; }
    }
}
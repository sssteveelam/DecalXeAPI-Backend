using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DecalXeAPI.Models
{
    public class Store
    {
        [Key]
        public string StoreID { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string StoreName { get; set; } = string.Empty;

        [MaxLength(255)] // Địa chỉ có thể dài
        public string? Address { get; set; } // Địa chỉ có thể cho phép NULL nếu chưa cập nhật

        // Navigation Property: Một Store có thể có nhiều Employee.
        [JsonIgnore]
        public ICollection<Employee>? Employees { get; set; }
    }
}
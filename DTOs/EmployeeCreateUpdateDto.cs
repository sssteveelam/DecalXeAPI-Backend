// DecalXeAPI/DTOs/EmployeeCreateUpdateDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class EmployeeCreateUpdateDto
    {
        // Thông tin cơ bản
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        [Required]
        public string StoreID { get; set; } = string.Empty;
        public string? AccountID { get; set; } // Có thể có hoặc không

        // Thông tin chi tiết cho từng vai trò (chỉ điền vào 1 trong các mục này)
        public AdminDetailDto? AdminDetail { get; set; }
        public ManagerDetailDto? ManagerDetail { get; set; }
        public SalesPersonDetailDto? SalesPersonDetail { get; set; }
        public DesignerDetailDto? DesignerDetail { get; set; }
        public TechnicianDetailDto? TechnicianDetail { get; set; }
    }
}
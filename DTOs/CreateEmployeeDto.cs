// DecalXeAPI/DTOs/CreateEmployeeDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class CreateEmployeeDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        [Required]
        public string StoreID { get; set; } = string.Empty;
        public string? AccountID { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
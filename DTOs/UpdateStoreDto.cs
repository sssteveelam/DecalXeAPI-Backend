// DecalXeAPI/DTOs/UpdateStoreDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class UpdateStoreDto
    {
        [Required]
        [MaxLength(100)]
        public string StoreName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Address { get; set; }
    }
}
// DecalXeAPI/DTOs/UpdateDesignDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class UpdateDesignDto
    {
        [Required]
        public string DesignURL { get; set; } = string.Empty;
        [Required]
        public string Version { get; set; } = string.Empty;
        [Required]
        public string ApprovalStatus { get; set; } = string.Empty;
        [Required]
        public decimal DesignPrice { get; set; }
        public bool IsAIGenerated { get; set; }
        public string? AIModelUsed { get; set; }
        
        [MaxLength(200)]
        public string? Size { get; set; } // Kích thước decal
    }
}
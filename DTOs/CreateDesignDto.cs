// DecalXeAPI/DTOs/CreateDesignDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class CreateDesignDto
    {
        [Required]
        public string DesignURL { get; set; } = string.Empty;
        public string? DesignerID { get; set; }
        [Required]
        public string Version { get; set; } = "1.0";
        [Required]
        public string ApprovalStatus { get; set; } = "Pending";
        public bool IsAIGenerated { get; set; } = false;
        public string? AIModelUsed { get; set; }
        [Required]
        public decimal DesignPrice { get; set; }
        
        [MaxLength(200)]
        public string? Size { get; set; } // Kích thước decal
    }
}
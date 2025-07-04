// DecalXeAPI/DTOs/UpdateDecalTemplateDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class UpdateDecalTemplateDto
    {
        [Required]
        public string TemplateName { get; set; } = string.Empty;
        public string? ImageURL { get; set; }
        [Required]
        public string DecalTypeID { get; set; } = string.Empty;
    }
}
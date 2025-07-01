// DecalXeAPI/DTOs/DesignerDetailDto.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace DecalXeAPI.DTOs
{
    public class DesignerDetailDto
    {
        public string? DesignSpecialty { get; set; }
        public string? DesignDifficultyDescription { get; set; }
        public decimal? DesignHoursWorked { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
// DecalXeAPI/DTOs/CreateFeedbackDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class CreateFeedbackDto
    {
        [Required]
        public string OrderID { get; set; } = string.Empty;

        [Required]
        public string CustomerID { get; set; } = string.Empty;

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }
}
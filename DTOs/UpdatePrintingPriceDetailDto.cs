// DecalXeAPI/DTOs/UpdatePrintingPriceDetailDto.cs
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class UpdatePrintingPriceDetailDto
    {
        [Required]
        public decimal BasePricePerSqMeter { get; set; }

        public decimal? MinLength { get; set; }
        public decimal? MaxLength { get; set; }
        public decimal? MinArea { get; set; }
        public decimal? MaxArea { get; set; }
        public decimal? ColorPricingFactor { get; set; }
        public string? PrintType { get; set; }
        public bool IsActive { get; set; }
        public string? FormulaDescription { get; set; }
    }
}
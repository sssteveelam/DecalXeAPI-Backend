namespace DecalXeAPI.DTOs
{
    public class PrintingPriceDetailDto
    {
        public string ServiceID { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty; 
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
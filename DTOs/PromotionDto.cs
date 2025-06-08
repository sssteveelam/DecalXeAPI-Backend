using System;

namespace DecalXeAPI.DTOs
{
    public class PromotionDto
    {
        public string PromotionID { get; set; } = string.Empty;
        public string PromotionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal DiscountPercentage { get; set; }
    }
}
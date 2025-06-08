namespace DecalXeAPI.DTOs
{
    public class DecalTypeDto
    {
        public string DecalTypeID { get; set; } = string.Empty;
        public string DecalTypeName { get; set; } = string.Empty;
        public string? Material { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
    }
}
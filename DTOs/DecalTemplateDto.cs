namespace DecalXeAPI.DTOs
{
    public class DecalTemplateDto
    {
        public string TemplateID { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public string? ImageURL { get; set; }
        public string DecalTypeID { get; set; } = string.Empty;
        public string DecalTypeName { get; set; } = string.Empty; // Để hiển thị tên loại decal
    }
}
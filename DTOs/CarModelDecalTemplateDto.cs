namespace DecalXeAPI.DTOs
{
    public class CarModelDecalTemplateDto
    {
        public string CarModelDecalTemplateID { get; set; } = string.Empty;
        public string ModelID { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty; // Tên mẫu xe
        public string TemplateID { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty; // Tên mẫu decal
    }
}
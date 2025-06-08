namespace DecalXeAPI.DTOs
{
    public class ServiceDecalTemplateDto
    {
        public string ServiceDecalTemplateID { get; set; } = string.Empty;
        public string ServiceID { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty; // Để hiển thị tên dịch vụ
        public string TemplateID { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty; // Để hiển thị tên mẫu decal
    }
}
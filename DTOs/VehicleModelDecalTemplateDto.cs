// DecalXeAPI/DTOs/VehicleModelDecalTemplateDto.cs
namespace DecalXeAPI.DTOs
{
    public class VehicleModelDecalTemplateDto // <-- Đổi từ CarModelDecalTemplateDto
    {
        public string VehicleModelDecalTemplateID { get; set; } = string.Empty; // <-- Đổi từ CarModelDecalTemplateID
        public string ModelID { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public string TemplateID { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
    }
}
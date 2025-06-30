namespace DecalXeAPI.DTOs
{
    // ĐỔI TÊN TỪ CarModelDecalTemplateDto
    public class VehicleModelDecalTemplateDto
    {
        public string VehicleModelDecalTemplateID { get; set; } = string.Empty;
        public string ModelID { get; set; } = string.Empty;
        public string VehicleModelName { get; set; } = string.Empty; // Tên mẫu xe (sẽ được ánh xạ)
        public string TemplateID { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty; // Tên mẫu decal (sẽ được ánh xạ)
    }
}
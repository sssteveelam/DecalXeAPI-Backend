// DecalXeAPI/DTOs/CategoryDto.cs
namespace DecalXeAPI.DTOs
{
    public class CategoryDto
    {
        public string CategoryID { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
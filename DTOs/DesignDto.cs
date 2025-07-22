// DecalXeAPI/DTOs/DesignDto.cs
namespace DecalXeAPI.DTOs
{
    public class DesignDto
    {
        public string DesignID { get; set; } = string.Empty;
        // OrderID đã được xóa
        public string DesignURL { get; set; } = string.Empty;
        public string? DesignerID { get; set; }
        public string? DesignerFullName { get; set; }
        public string Version { get; set; } = string.Empty;
        public string ApprovalStatus { get; set; } = string.Empty;
        public bool IsAIGenerated { get; set; }
        public string? AIModelUsed { get; set; }
        public decimal DesignPrice { get; set; } // Thêm trường mới
        public string? Size { get; set; } // Kích thước decal

        /// <summary>
        /// Danh sách các item template trong design này
        /// </summary>
        public ICollection<DesignTemplateItemDto>? TemplateItems { get; set; }
    }
}
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
        // AIPrompt đã được xóa
        public decimal DesignPrice { get; set; } // Thêm trường mới
    }
}
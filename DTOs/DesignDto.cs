namespace DecalXeAPI.DTOs
{
    public class DesignDto
    {
        public string DesignID { get; set; } = string.Empty;
        public string OrderID { get; set; } = string.Empty;
        public string? OrderStatus { get; set; } // Trạng thái đơn hàng liên quan
        public string DesignURL { get; set; } = string.Empty;
        public string? DesignerID { get; set; }
        public string? DesignerFullName { get; set; } // Tên đầy đủ của người thiết kế
        public string Version { get; set; } = string.Empty;
        public string ApprovalStatus { get; set; } = string.Empty;
        public bool IsAIGenerated { get; set; }
        public string? AIModelUsed { get; set; }
        public string? AIPrompt { get; set; }
    }
}
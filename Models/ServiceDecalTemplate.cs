using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecalXeAPI.Models
{
    // Bảng trung gian ServiceDecalTemplate
    public class ServiceDecalTemplate
    {
        // Khóa chính composite (tổng hợp từ 2 FK)
        // Hoặc có thể dùng một PK riêng nếu muốn quản lý thêm thuộc tính cho mối liên kết này
        [Key]
        public string ServiceDecalTemplateID { get; set; } = Guid.NewGuid().ToString(); // PK riêng

        // Khóa ngoại 1: đến DecalService
        public string ServiceID { get; set; } = string.Empty; // FK_ServiceID
        // Navigation Property
        public DecalService? DecalService { get; set; }

        // Khóa ngoại 2: đến DecalTemplate
        public string TemplateID { get; set; } = string.Empty; // FK_TemplateID
        // Navigation Property
        public DecalTemplate? DecalTemplate { get; set; }

        // Có thể thêm các thuộc tính khác cho mối quan hệ này nếu cần
        // Ví dụ: IsRecommended, OrderPriority, etc.
    }
}
// DecalXeAPI/Models/OrderStage.cs
using System.ComponentModel;

namespace DecalXeAPI.Models
{
    /// <summary>
    /// Enum định nghĩa 4 giai đoạn chính trong vòng đời của một đơn hàng decal
    /// </summary>
    public enum OrderStage
    {
        /// <summary>
        /// Giai đoạn 1: Tiếp nhận yêu cầu và khảo sát xe
        /// </summary>
        [Description("Khảo sát")]
        Survey = 1,

        /// <summary>
        /// Giai đoạn 2: Lên ý tưởng và thiết kế mẫu decal
        /// </summary>
        [Description("Thiết kế")]
        Designing = 2,

        /// <summary>
        /// Giai đoạn 3: Khách hàng chốt mẫu và bắt đầu thi công, dán decal
        /// </summary>
        [Description("Chốt và thi công")]
        ProductionAndInstallation = 3,

        /// <summary>
        /// Giai đoạn 4: Hoàn thành, nghiệm thu và bàn giao cho khách hàng
        /// </summary>
        [Description("Nghiệm thu và nhận hàng")]
        AcceptanceAndDelivery = 4
    }
}

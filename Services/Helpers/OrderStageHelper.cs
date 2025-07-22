// DecalXeAPI/Services/Helpers/OrderStageHelper.cs
using System.ComponentModel;
using System.Reflection;
using DecalXeAPI.Models;

namespace DecalXeAPI.Services.Helpers
{
    /// <summary>
    /// Helper class để làm việc với OrderStage enum
    /// </summary>
    public static class OrderStageHelper
    {
        /// <summary>
        /// Lấy mô tả tiếng Việt của OrderStage enum
        /// </summary>
        /// <param name="stage">OrderStage enum value</param>
        /// <returns>Mô tả tiếng Việt</returns>
        public static string GetDescription(OrderStage stage)
        {
            FieldInfo? field = stage.GetType().GetField(stage.ToString());
            if (field != null)
            {
                DescriptionAttribute? attribute = field.GetCustomAttribute<DescriptionAttribute>();
                if (attribute != null)
                {
                    return attribute.Description;
                }
            }
            return stage.ToString();
        }

        /// <summary>
        /// Lấy tất cả OrderStage với mô tả
        /// </summary>
        /// <returns>Dictionary với key là OrderStage và value là mô tả</returns>
        public static Dictionary<OrderStage, string> GetAllWithDescriptions()
        {
            var result = new Dictionary<OrderStage, string>();
            foreach (OrderStage stage in Enum.GetValues<OrderStage>())
            {
                result[stage] = GetDescription(stage);
            }
            return result;
        }

        /// <summary>
        /// Lấy giai đoạn tiếp theo trong quy trình
        /// </summary>
        /// <param name="currentStage">Giai đoạn hiện tại</param>
        /// <returns>Giai đoạn tiếp theo, hoặc null nếu đã ở giai đoạn cuối</returns>
        public static OrderStage? GetNextStage(OrderStage currentStage)
        {
            return currentStage switch
            {
                OrderStage.Survey => OrderStage.Designing,
                OrderStage.Designing => OrderStage.ProductionAndInstallation,
                OrderStage.ProductionAndInstallation => OrderStage.AcceptanceAndDelivery,
                OrderStage.AcceptanceAndDelivery => null, // Đã hoàn thành
                _ => null
            };
        }

        /// <summary>
        /// Lấy giai đoạn trước đó trong quy trình
        /// </summary>
        /// <param name="currentStage">Giai đoạn hiện tại</param>
        /// <returns>Giai đoạn trước đó, hoặc null nếu đã ở giai đoạn đầu</returns>
        public static OrderStage? GetPreviousStage(OrderStage currentStage)
        {
            return currentStage switch
            {
                OrderStage.Survey => null, // Giai đoạn đầu
                OrderStage.Designing => OrderStage.Survey,
                OrderStage.ProductionAndInstallation => OrderStage.Designing,
                OrderStage.AcceptanceAndDelivery => OrderStage.ProductionAndInstallation,
                _ => null
            };
        }

        /// <summary>
        /// Kiểm tra xem có thể chuyển từ giai đoạn này sang giai đoạn khác không
        /// </summary>
        /// <param name="from">Giai đoạn hiện tại</param>
        /// <param name="to">Giai đoạn muốn chuyển tới</param>
        /// <returns>True nếu có thể chuyển, False nếu không</returns>
        public static bool CanTransitionTo(OrderStage from, OrderStage to)
        {
            // Có thể chuyển tới giai đoạn tiếp theo
            if (GetNextStage(from) == to)
                return true;

            // Có thể chuyển về giai đoạn trước đó (trong trường hợp cần sửa lại)
            if (GetPreviousStage(to) == from)
                return true;

            // Không cho phép nhảy giai đoạn
            return false;
        }

        /// <summary>
        /// Tính phần trăm hoàn thành dựa trên giai đoạn hiện tại
        /// </summary>
        /// <param name="stage">Giai đoạn hiện tại</param>
        /// <returns>Phần trăm hoàn thành (0-100)</returns>
        public static int GetCompletionPercentage(OrderStage stage)
        {
            return stage switch
            {
                OrderStage.Survey => 25,
                OrderStage.Designing => 50,
                OrderStage.ProductionAndInstallation => 75,
                OrderStage.AcceptanceAndDelivery => 100,
                _ => 0
            };
        }
    }
}

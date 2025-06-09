using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.DTOs
{
    public class ConvertCsrToOrderDto
    {
        // CustomRequestID sẽ được lấy từ URL param, không cần trong body

        [Required(ErrorMessage = "AssignedEmployeeID là bắt buộc.")]
        public string AssignedEmployeeID { get; set; } = string.Empty; // Nhân viên được giao xử lý đơn hàng này

        [Required(ErrorMessage = "EstimatedCost là bắt buộc.")]
        public decimal EstimatedCost { get; set; } // Chi phí ước tính cuối cùng (từ CustomServiceRequest)

        [Required(ErrorMessage = "EstimatedWorkUnits là bắt buộc.")]
        public int EstimatedWorkUnits { get; set; } // Số lượng xuất công ước tính cuối cùng (từ CustomServiceRequest)

        // ID của dịch vụ chung cho "thiết kế và thi công tùy chỉnh"
        // Đệ cần đảm bảo có một DecalService trong DB với ServiceID này
        [Required(ErrorMessage = "CustomServiceServiceID là bắt buộc.")]
        public string CustomServiceServiceID { get; set; } = string.Empty;
    }
}
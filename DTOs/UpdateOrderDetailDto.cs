// DecalXeAPI/DTOs/UpdateOrderDetailDto.cs
namespace DecalXeAPI.DTOs
{
    public class UpdateOrderDetailDto
    {
        // Khi cập nhật, không cho phép đổi ServiceID và OrderID
        public int Quantity { get; set; }
        public decimal? ActualAreaUsed { get; set; }
        public decimal? ActualLengthUsed { get; set; }
        public decimal? ActualWidthUsed { get; set; }
    }
}
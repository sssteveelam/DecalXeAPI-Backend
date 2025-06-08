namespace DecalXeAPI.QueryParams
{
    public class OrderQueryParams
    {
        // Tham số tìm kiếm chung (tìm trong nhiều trường)
        public string? SearchTerm { get; set; }

        // Tham số lọc theo trạng thái đơn hàng
        public string? Status { get; set; } // Ví dụ: "New", "Pending", "Completed"

        // Tham số sắp xếp
        public string? SortBy { get; set; } // Ví dụ: "orderDate", "totalAmount", "customerName"
        public string? SortOrder { get; set; } = "asc"; // "asc" (tăng dần) hoặc "desc" (giảm dần)

        // Tham số phân trang
        private const int MaxPageSize = 50; // Giới hạn số lượng bản ghi tối đa trên một trang
        public int PageNumber { get; set; } = 1; // Số trang hiện tại (mặc định là trang 1)

        private int _pageSize = 10; // Số lượng bản ghi trên một trang (mặc định là 10)
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value; // Không cho phép quá MaxPageSize
        }
    }
}
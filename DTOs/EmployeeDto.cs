namespace DecalXeAPI.DTOs
{
    public class EmployeeDto
    {
        public string EmployeeID { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string StoreID { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty; // Tên cửa hàng để hiển thị
        public string? AccountID { get; set; }
        public string? AccountUsername { get; set; } // Username của tài khoản liên kết
        public string? AccountRoleName { get; set; } // Tên vai trò của tài khoản liên kết
    }
}
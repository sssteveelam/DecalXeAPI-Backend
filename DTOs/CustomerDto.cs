namespace DecalXeAPI.DTOs
{
    public class CustomerDto
    {
        public string CustomerID { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string CustomerFullName { get; set; } = string.Empty; // <-- THÊM DÒNG NÀY
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? AccountID { get; set; }
        public string? AccountUsername { get; set; } // Username của tài khoản liên kết
        public string? AccountRoleName { get; set; } // Tên vai trò của tài khoản liên kết
    }
}
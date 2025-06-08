namespace DecalXeAPI.DTOs
{
    public class AccountDto
    {
        public string AccountID { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string RoleID { get; set; } = string.Empty; // Vẫn giữ RoleID để biết liên kết
        public string RoleName { get; set; } = string.Empty; // Thêm RoleName để hiển thị trực tiếp

        // Có thể thêm các thuộc tính khác nếu Frontend cần
        // Ví dụ: public string? CreatedDate { get; set; }
    }
}
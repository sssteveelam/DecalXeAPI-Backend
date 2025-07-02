namespace DecalXeAPI.DTOs
{
    public class EmployeeDto
    {
       public string EmployeeID { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string StoreID { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string? AccountID { get; set; }
        public string? AccountUsername { get; set; }
        public string? AccountRoleName { get; set; }
        public bool IsActive { get; set; }

        // --- THÊM CÁC THUỘC TÍNH CHI TIẾT VAI TRÒ ---
        public AdminDetailDto? AdminDetail { get; set; }
        public ManagerDetailDto? ManagerDetail { get; set; }
        public SalesPersonDetailDto? SalesPersonDetail { get; set; }
        public DesignerDetailDto? DesignerDetail { get; set; }
        public TechnicianDetailDto? TechnicianDetail { get; set; }
    }
}
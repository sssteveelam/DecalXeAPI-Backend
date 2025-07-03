using Swashbuckle.AspNetCore.Filters;
using DecalXeAPI.Models; // Cần cho Account model
using System; // Cần cho Guid

namespace DecalXeAPI.SwaggerExamples
{
    /// <summary>
    /// Cung cấp ví dụ cho Request Body của Account khi cập nhật (PUT /api/Accounts/{id}).
    /// </summary>
    public class AccountUpdateRequestExample : IExamplesProvider<Account>
    {
        public Account GetExamples()
        {
            return new Account
            {
                AccountID = "ACC_CUSTOMER_001", // ID tài khoản cần cập nhật (phải khớp với ID trong URL)
                Username = "customer_web_updated", // Tên đăng nhập mới
                PasswordHash = "new_customer_password123", // Mật khẩu mới (trong thực tế cần hash)
                IsActive = true,
                RoleID = "ROLE_CUSTOMER", // RoleID mới
                Email = "customer.updated@example.com"
            };
        }
    }
}

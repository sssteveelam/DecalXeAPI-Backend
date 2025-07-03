using Swashbuckle.AspNetCore.Filters; // Cần cho IExamplesProvider
using DecalXeAPI.Models; // Cần cho Account model

namespace DecalXeAPI.SwaggerExamples
{
    /// <summary>
    /// Cung cấp ví dụ cho Request Body của Account (ví dụ: khi POST /api/Accounts).
    /// </summary>
    public class AccountRequestExample : IExamplesProvider<Account> // IExamplesProvider cho Account Model
    {
        public Account GetExamples()
        {
            return new Account
            {
                Username = "new_user_example",
                PasswordHash = "secure_password_123",
                IsActive = true,
                RoleID = "ROLE_CUSTOMER", 
                Email = "user.example@decal.com"
            };
        }
    }
}

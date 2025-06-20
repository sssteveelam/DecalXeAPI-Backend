using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IEmailService
    {
        // Phương thức gửi email:
        // recipientEmail: Địa chỉ email người nhận.
        // subject: Chủ đề email.
        // body: Nội dung email (có thể là HTML).
        Task<bool> SendEmailAsync(string recipientEmail, string subject, string body);
    }
}
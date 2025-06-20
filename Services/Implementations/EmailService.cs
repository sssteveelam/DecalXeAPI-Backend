using MailKit.Net.Smtp; // Cần cho SmtpClient
using MailKit.Security; // Cần cho SecureSocketOptions
using MimeKit; // Cần cho MimeMessage, MailboxAddress, TextPart
using Microsoft.Extensions.Configuration; // Cần để đọc cấu hình từ appsettings.json
using Microsoft.Extensions.Logging; // Cần để ghi log
using System.Threading.Tasks;
using System;
using DecalXeAPI.Services.Interfaces; // Để sử dụng IEmailService

namespace DecalXeAPI.Services.Implementations
{
    public class EmailService : IEmailService // <-- Kế thừa từ IEmailService
    {
        private readonly IConfiguration _configuration; // Biến để đọc cấu hình
        private readonly ILogger<EmailService> _logger; // Biến để ghi log

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // Phương thức gửi email
        public async Task<bool> SendEmailAsync(string recipientEmail, string subject, string body)
        {
            try
            {
                // 1. Đọc cấu hình email từ appsettings.json
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587"); // Đảm bảo lấy được port
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderName = _configuration["EmailSettings:SenderName"];
                var appPassword = _configuration["EmailSettings:AppPassword"]; // Mật khẩu ứng dụng

                // Kiểm tra các cài đặt quan trọng
                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(appPassword))
                {
                    _logger.LogError("Cấu hình EmailSettings bị thiếu: SmtpHost, SenderEmail hoặc AppPassword.");
                    return false;
                }

                // 2. Tạo nội dung email (MimeMessage)
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(senderName, senderEmail)); // Người gửi
                email.To.Add(new MailboxAddress("", recipientEmail)); // Người nhận
                email.Subject = subject; // Chủ đề
                email.Body = new TextPart(MimeKit.Text.TextFormat.Html) // Nội dung dạng HTML
                {
                    Text = body
                };

                // 3. Sử dụng SmtpClient để gửi email
                using (var client = new SmtpClient())
                {
                    // Kết nối đến máy chủ SMTP của Gmail
                    // SecureSocketOptions.StartTls: sử dụng STARTTLS để mã hóa kết nối
                    await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);

                    // Xác thực (đăng nhập) bằng email và mật khẩu ứng dụng
                    await client.AuthenticateAsync(senderEmail, appPassword);

                    // Gửi email
                    await client.SendAsync(email);

                    // Ngắt kết nối
                    await client.DisconnectAsync(true); // true để đảm bảo QUIT command được gửi

                    _logger.LogInformation("Email đã được gửi thành công đến {RecipientEmail} với chủ đề: {Subject}", recipientEmail, subject);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email đến {RecipientEmail} với chủ đề: {Subject}. Chi tiết: {ErrorMessage}", recipientEmail, subject, ex.Message);
                return false;
            }
        }
    }
}
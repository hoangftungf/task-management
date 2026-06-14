using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using TaskManagementApi.Services;

namespace TaskManagementApi.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            
            // Người gửi
            email.From.Add(new MailboxAddress(
                _config["EmailSettings:DisplayName"], 
                _config["EmailSettings:Mail"]
            ));
            
            // Người nhận
            email.To.Add(MailboxAddress.Parse(toEmail));
            
            email.Subject = subject;

            // Nội dung (Hỗ trợ HTML)
            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                // Kết nối tới server Gmail
                await smtp.ConnectAsync(_config["EmailSettings:Host"], int.Parse(_config["EmailSettings:Port"]), SecureSocketOptions.StartTls);
                
                // Đăng nhập bằng App Password
                await smtp.AuthenticateAsync(_config["EmailSettings:Mail"], _config["EmailSettings:Password"]);
                
                // Bắn mail đi
                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> LỖI GỬI EMAIL: {ex.Message}");
                throw;
            }
            finally
            {
                // Đóng kết nối
                await smtp.DisconnectAsync(true);
            }
        }
    }
}
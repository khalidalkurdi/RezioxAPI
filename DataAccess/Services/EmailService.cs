using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using Model.Configuration;

namespace DataAccess.Services
{
    public class EmailService : IEmailService
    {
        
        private readonly EmailSettings _configuration;

        public EmailService(IOptions<EmailSettings> configuration)
        {
            _configuration = configuration.Value;
        }
        public async Task SendVerificationCodeAsync(string toEmail, string VerificationCode)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Reziox App", _configuration.Username));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = "Verification Code";

                message.Body = new TextPart("html")
                {
                    Text = $"<p>Your verification code is:<b> {VerificationCode}</b></p>"
                };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_configuration.Host,_configuration.Port, true);
                    await client.AuthenticateAsync(_configuration.Username,_configuration.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"error throw send email :{ex.Message}");
            }
        }
    }
}

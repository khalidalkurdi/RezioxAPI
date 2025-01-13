using MailKit.Net.Smtp;
using MimeKit;

namespace DataAccess.Services
{
    public class EmailService : IEmailService
    {
            string host = "smtp.gmail.com";
            string email = "rezioxapp@gmail.com";
            string pass = "lgwqqflzvdyrhuor";
            

        public async Task SendVerificationCodeAsync(string toEmail, string VerificationCode)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Reziox App", email));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = "Verification Code";

                message.Body = new TextPart("html")
                {
                    Text = $"<p>Your verification code is:<b> {VerificationCode}</b></p>"
                };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(host, 465, true);
                    await client.AuthenticateAsync(email, pass);
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

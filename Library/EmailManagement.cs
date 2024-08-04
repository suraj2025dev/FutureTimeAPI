using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;
using System;
using Library.Data;

namespace Library
{
    public static class EmailManagement
    {
        public static void SendMail(MailDTO dto)
        {
            var emailSettings = new EmailSettings
            {
                Host = AppStatic.CONFIG.App.Email.HOST,
                Port = int.Parse(AppStatic.CONFIG.App.Email.PORT),
                Mail = AppStatic.CONFIG.App.Email.MAIL,
                Password = AppStatic.CONFIG.App.Email.PASSWORD,
                SecureSocketOptions = AppStatic.CONFIG.App.Email.SecureSocketOptions
            };
            
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(dto.DisplayName, emailSettings.Mail));
                foreach (var recipient in dto.EmailTo)
                {
                    message.To.Add(MailboxAddress.Parse(recipient));
                }
                message.Subject = dto.Subject;
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = dto.Body;
                message.Body = bodyBuilder.ToMessageBody();
                using var smtp = new SmtpClient();
                smtp.Connect(emailSettings.Host, emailSettings.Port, (SecureSocketOptions)emailSettings.SecureSocketOptions);
                smtp.Authenticate(emailSettings.Mail, emailSettings.Password);
                smtp.Send(message);
                smtp.Disconnect(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}

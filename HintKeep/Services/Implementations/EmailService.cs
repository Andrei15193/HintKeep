using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace HintKeep.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailServiceConfig _emailServiceConfig;

        public EmailService(EmailServiceConfig emailServiceConfig)
            => _emailServiceConfig = emailServiceConfig;

        public Task SendAsync(string emailAddress, string subject, string body)
            => SendAsync(emailAddress, subject, body, default);

        public async Task SendAsync(string emailAddress, string subject, string body, CancellationToken cancellationToken)
        {
            var email = new MimeMessage
            {
                Subject = subject,
                From =
                {
                    new MailboxAddress(_emailServiceConfig.SenderDisplayNameAddress, _emailServiceConfig.SenderEmailAddress)
                },
                To =
                {
                    MailboxAddress.Parse(emailAddress)
                },
                Body = new BodyBuilder { HtmlBody = body }.ToMessageBody()
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailServiceConfig.ServerAddress, _emailServiceConfig.ServerPort, SecureSocketOptions.SslOnConnect, cancellationToken);
                await client.AuthenticateAsync(_emailServiceConfig.ServerCredentials, cancellationToken);

                await client.SendAsync(email, cancellationToken);

                await client.DisconnectAsync(true);
            }
        }
    }
}
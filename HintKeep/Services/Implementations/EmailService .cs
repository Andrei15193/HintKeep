using System.Threading;
using System.Threading.Tasks;
using HintKeep.Services.Implementations.Configs;
using MailKit.Net.Smtp;
using MimeKit;

namespace HintKeep.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailServiceConfig _config;

        public EmailService(EmailServiceConfig config)
            => _config = config;

        public Task SendAsync(EmailMessage emailMessage)
            => SendAsync(emailMessage, default);

        public async Task SendAsync(EmailMessage emailMessage, CancellationToken cancellationToken)
        {
            var message = new MimeMessage
            {
                From =
                {
                    new MailboxAddress(_config.SenderName, _config.SenderEMailAddress)
                },
                To =
                {
                    MailboxAddress.Parse(emailMessage.To)
                },
                Subject = emailMessage.Title,
                Body = new BodyBuilder
                {
                    HtmlBody = emailMessage.Content
                }.ToMessageBody()
            };

            using (var smtpClient = new SmtpClient())
            {
                await smtpClient.ConnectAsync(_config.SmtpHost, _config.SmtpPort, useSsl: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                await smtpClient.AuthenticateAsync(_config.Credentials, cancellationToken).ConfigureAwait(false);
                await smtpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
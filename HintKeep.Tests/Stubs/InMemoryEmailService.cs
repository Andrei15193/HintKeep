using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Services;

namespace HintKeep.Tests.Stubs
{
    public class InMemoryEmailService : IEmailService
    {
        private readonly List<EmailMessage> _sentEmailMessages = new List<EmailMessage>();

        public IReadOnlyList<EmailMessage> SentEmailMessages
            => _sentEmailMessages;

        public Task SendAsync(EmailMessage emailMessage)
            => SendAsync(emailMessage, default);

        public Task SendAsync(EmailMessage emailMessage, CancellationToken cancellationToken)
        {
            _sentEmailMessages.Add(emailMessage);
            return Task.CompletedTask;
        }
    }
}
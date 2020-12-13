using System.Linq;
using System.Net;
using System.Security;
using Microsoft.Extensions.Configuration;

namespace HintKeep.Services.Implementations.Configs
{
    public sealed class EmailServiceConfig
    {
        public EmailServiceConfig(IConfigurationSection configurationSection)
        {
            SenderName = configurationSection.GetValue<string>(nameof(SenderName));
            SenderEMailAddress = configurationSection.GetValue<string>(nameof(SenderEMailAddress));
            Credentials = _GetCredentials(configurationSection.GetSection(nameof(Credentials)));
            SmtpHost = configurationSection.GetValue<string>(nameof(SmtpHost));
            SmtpPort = configurationSection.GetValue<int>(nameof(SmtpPort));
        }

        private static ICredentials _GetCredentials(IConfigurationSection configurationSection)
            => new NetworkCredential(
                configurationSection.GetValue<string>(nameof(NetworkCredential.UserName)),
                configurationSection
                    .GetValue<string>(nameof(NetworkCredential.SecurePassword))
                    .Aggregate(new SecureString(), (result, @char) => { result.AppendChar(@char); return result; })
            );

        public string SenderName { get; }

        public string SenderEMailAddress { get; }

        public ICredentials Credentials { get; }

        public string SmtpHost { get; }

        public int SmtpPort { get; }
    }
}
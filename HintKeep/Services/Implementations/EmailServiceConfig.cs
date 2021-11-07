using System.Net;

namespace HintKeep.Services.Implementations
{
    public class EmailServiceConfig
    {
        public string SenderEmailAddress { get; init; }

        public string SenderDisplayNameAddress { get; init; }

        public string ServerAddress { get; init; }

        public int ServerPort { get; init; }

        public ICredentials ServerCredentials { get; init; }
    }
}
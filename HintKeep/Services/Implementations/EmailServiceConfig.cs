using System.Net;

namespace HintKeep.Services.Implementations
{
    public record EmailServiceConfig(
        string SenderEmailAddress,
        string SenderDisplayNameAddress,
        string ServerAddress,
        int ServerPort,
        ICredentials ServerCredentials
    );
}
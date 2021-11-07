using System;

namespace HintKeep.Services
{
    public interface ISecurityService
    {
        string GeneratePasswordSalt();

        ConfirmationToken GenerateConfirmationToken();

        string ComputePasswordHash(string salt, string password);
    }

    public class ConfirmationToken
    {
        public ConfirmationToken(string token, TimeSpan expiration)
            => (Token, Expiration) = (token, expiration);

        public string Token { get; }

        public TimeSpan Expiration { get; }
    }
}
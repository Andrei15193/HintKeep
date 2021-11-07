namespace HintKeep.Services.Implementations
{
    public class SecurityServiceConfig
    {
        public string HashAlgorithm { get; init; }

        public int SaltLength { get; init; }

        public string PasswordFormat { get; init; }

        public int ActivationTokenLength { get; init; }

        public int ActivationTokenExpirationMinutes { get; init; }
    }
}
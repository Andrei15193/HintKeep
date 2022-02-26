namespace HintKeep.Services.Implementations
{
    public record SecurityServiceConfig(
        string HashAlgorithm,
        int SaltLength,
        string PasswordFormat,
        int ActivationTokenLength,
        int ActivationTokenExpirationMinutes
    );
}
namespace HintKeep.GraphQL;

public static class ServiceKeys
{
    public const string CorrelationId = "correlation-id";

    public const string UsernameHashAlgorithm = "username-hash-algorithm";
    public const string PasswordHashAlgorithm = "password-hash-algorithm";
    public const string EmailAddressHashAlgorithm = "email-address-hash-algorithm";

    public const string SessionTokenSigningKey = "session-token-signing-key";

    public const string SessionTicketSigningKey = "session-ticket-signing-key";
}
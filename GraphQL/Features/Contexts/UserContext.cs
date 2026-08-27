namespace HintKeep.GraphQL.Features.Contexts;

public record UserContext(string UserId, string Username, string SessionId, string SessionRenewTicket);
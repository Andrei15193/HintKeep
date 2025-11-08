using Microsoft.Extensions.DependencyInjection;

namespace HintKeep.GraphQL.AppSetup.Registrations;

internal static class CorrelationId
{
    public static IServiceCollection AddCorrelationId(this IServiceCollection services)
        => services
            .AddKeyedScoped(typeof(Guid), ServiceKeys.CorrelationId, delegate { return Guid.NewGuid(); });
}
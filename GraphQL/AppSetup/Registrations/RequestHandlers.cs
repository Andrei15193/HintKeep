using HintKeep.GraphQL.Definitions;
using Microsoft.Extensions.DependencyInjection;

namespace HintKeep.GraphQL.AppSetup.Registrations;

public static class RequestHandlers
{
    public static IServiceCollection AddRequestHandlers(this IServiceCollection services)
    {
        var requestHandlers =
            from type in typeof(Program).Assembly.DefinedTypes
            where type.IsClass
            from implementedInterface in type.ImplementedInterfaces
            where implementedInterface.IsConstructedGenericType && implementedInterface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
            select (RequestHandlerInterface: implementedInterface, RequestHandlerImplementation: type);

        foreach (var (requestHandlerInterface, requestHandlerImplementation) in requestHandlers)
            services.AddScoped(requestHandlerInterface, requestHandlerImplementation);

        return services;
    }
}
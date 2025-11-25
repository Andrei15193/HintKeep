using GraphQL;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using HintKeep.GraphQL.AppSetup.Middlewares;
using HintKeep.GraphQL.AppSetup.Registrations;

FunctionsApplication
    .CreateBuilder(args)
    .ConfigureFunctionsWebApplication()

    .UseMiddleware<LoggingMiddleware>()
    .UseMiddleware<AuthenticationMiddleware>()
    .UseMiddleware<GraphiQLMiddlewareAdapter>()

    .AddHintKeepServices()

    .Build()
    .Run();

/// <remarks>
/// The <see cref="Program"/> class is only exposed to facilitate integration tests.
/// </remarks>
public partial class Program { }
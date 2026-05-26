using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Data.Tables;
using CloudStub.AzureDataTables;
using GraphQL;
using GraphQL.Transport;
using GraphQL.Types;
using GraphQL.Validation;
using HintKeep.GraphQL.AppSetup.Middlewares;
using HintKeep.GraphQL.AppSetup.Registrations;
using HintKeep.GraphQL.Data;
using HintKeep.GraphQL.Definitions.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using SameSiteMode = Microsoft.Net.Http.Headers.SameSiteMode;

namespace HintKeep.GraphQL.Features;

public class HintKeepWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHostBuilder? CreateHostBuilder()
        => Host.CreateDefaultBuilder().ConfigureWebHostDefaults(builder => { });

    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder
            .UseContentRoot(".")
            .ConfigureServices((context, services) =>
            {
                using var settingsFileStream = context
                    .HostingEnvironment
                    .ContentRootFileProvider
                    .GetFileInfo("local.settings.json")
                    .CreateReadStream();

                services
                    .AddSingleton<IConfiguration>(new ConfigurationBuilder()
                        .AddInMemoryCollection(
                            JsonSerializer
                                .Deserialize<JsonObject>(settingsFileStream)
                                ?["Values"]
                                ?.AsObject()
                                .ToDictionary(
                                    jsonProperty => jsonProperty.Key,
                                    jsonProperty => Convert.ToString(jsonProperty.Value?.GetValue<object?>())
                                )
                        )
                        .Build()
                    )

                    .AddSingleton<AuthenticationMiddleware>()

                    .AddHintKeepServices();
            })
            .ConfigureTestServices(services =>
            {
                services
                    .AddSingleton<TableServiceClient, TableServiceClientStub>()
                    .AddSingleton(services =>
                {
                    var hintKeepTableStorage = new HintKeepTableStorage(services.GetRequiredService<TableServiceClient>());

                    var hintKeepTables = typeof(HintKeepTableStorage)
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                        .Where(property => typeof(TableClient).IsAssignableFrom(property.PropertyType))
                        .Select(property => (TableClient)property.GetValue(hintKeepTableStorage)!);
                    foreach (var hintKeepTable in hintKeepTables)
                        hintKeepTable.CreateIfNotExists();

                    return hintKeepTableStorage;
                });
            })
            .Configure(builder => builder
                .UseMiddleware<AuthenticationMiddleware>()
                .UseRouting()
                .UseEndpoints(endpoints => endpoints
                    .MapPost(
                        "/api/graphql",
                        async httpContext =>
                        {
                            var schema = httpContext.RequestServices.GetRequiredService<ISchema>();
                            var serializer = httpContext.RequestServices.GetRequiredService<IGraphQLTextSerializer>();

                            string requestBody;
                            using (var streamReader = new StreamReader(httpContext.Request.Body))
                                requestBody = await streamReader.ReadToEndAsync(httpContext.RequestAborted);

                            var graphQlRequest = serializer.Deserialize<GraphQLRequest>(requestBody);

                            var result = graphQlRequest is null
                                ? "{}"
                                : await schema.ExecuteAsync(
                                    serializer,
                                    options =>
                                    {
                                        options.User = httpContext.User;
                                        options.SetHttpCookieHandler(new TestHttpCookieHandler(httpContext));

                                        options.OperationName = graphQlRequest.OperationName;
                                        options.Query = graphQlRequest.Query;
                                        options.Variables = graphQlRequest.Variables;
                                        options.Extensions = graphQlRequest.Extensions;
                                        options.DocumentId = graphQlRequest.DocumentId;
                                        options.CancellationToken = httpContext.RequestAborted;

                                        options.Root = new object();
                                        options.RequestServices = httpContext.RequestServices;
                                        options.ValidationRules = httpContext.RequestServices.GetServices<IValidationRule>();
                                    }
                                );

                            httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                            httpContext.Response.Headers.Append("Content-Type", "application/json; charset=utf-8");
                            await httpContext.Response.WriteAsync(result, Encoding.UTF8, httpContext.RequestAborted);
                        }
                    )
                )
            );

    private class TestHttpCookieHandler(HttpContext httpContext) : HintKeepHttp.IHttpCookieHandler
    {
        public string? Get(string name)
            => httpContext
                .Request
                .Cookies
                .SingleOrDefault(cookie => cookie.Key == name)
                .Value;

        public void Set(string name, string value, DateTime expiration)
            => httpContext.Response.Headers.Append(
                HeaderNames.SetCookie,
                new SetCookieHeaderValue(name, value)
                {
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    HttpOnly = true,
                    Expires = expiration
                }.ToString()
            );
    }
}

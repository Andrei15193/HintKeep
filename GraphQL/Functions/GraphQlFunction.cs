using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Azure.Core;
using GraphQL;
using GraphQL.Server.Ui.GraphiQL;
using GraphQL.Transport;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace HintKeep.GraphQL.Functions;

public class GraphQlFunction(
        IHostEnvironment environment,
        ILogger<GraphQlFunction> logger,
        ISchema schema,
        IGraphQLTextSerializer serializer
    )
{
    [Function("graphql")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData request)
    {
        var response = request.CreateResponse();
        response.Headers.Add("Cache-Control", "no-store");

        try
        {
            await HandleRequest(request, response);
        }
        catch (JsonException jsonException)
        {
            logger.LogError(jsonException, "Invalid JSON in request body.");

            response.StatusCode = HttpStatusCode.BadRequest;
            if (environment.IsDevelopment())
            {
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(serializer.Serialize(jsonException), Encoding.UTF8);
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unknown error occurred.");

            response.StatusCode = HttpStatusCode.InternalServerError;
            if (environment.IsDevelopment())
            {
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(serializer.Serialize(exception), Encoding.UTF8);
            }
        }

        return response;
    }

    private Task HandleRequest(HttpRequestData request, HttpResponseData response)
        => request.Method.ToLowerInvariant() switch
        {
            "get" when environment.IsDevelopment() => HandleGraphiQlPlaygroundAsync(request, response),
            "post" => HandleGraplQlRequestAsync(request, response),
            _ => HandleUnsupportedMethodAsync(request, response),
        };

    private async Task HandleGraphiQlPlaygroundAsync(HttpRequestData request, HttpResponseData response)
    {
        logger.LogInformation("GraphQL Playground Request Started.");

        var graphiQLMiddleware = new GraphiQLMiddleware(
            context => Task.CompletedTask,
            new GraphiQLOptions
            {
                GraphQLEndPoint = request.Url.AbsolutePath,
                GraphQLWsSubscriptions = false
            }
        );
        await graphiQLMiddleware.Invoke(new GraphiQLHttpContextAdapter(request, response));

        logger.LogInformation("GraphQL Playground Request Completed.");
    }

    private async Task HandleGraplQlRequestAsync(HttpRequestData request, HttpResponseData response)
    {
        logger.LogInformation("GraphQL Request Started.");

        string requestBody;
        using (var streamReader = new StreamReader(request.Body))
            requestBody = await streamReader.ReadToEndAsync();
        logger.LogDebug("Executing GraphQL Request '{graphQlRequest}'.", requestBody);

        var graphQlRequest = serializer.Deserialize<GraphQLRequest>(requestBody);

        var result = graphQlRequest is null
            ? "{}"
            : await schema.ExecuteAsync(
                serializer,
                options =>
                {
                    options.OperationName = graphQlRequest.OperationName;
                    options.Query = graphQlRequest.Query;
                    options.Variables = graphQlRequest.Variables;
                    options.Extensions = graphQlRequest.Extensions;
                    options.DocumentId = graphQlRequest.DocumentId;

                    options.Root = new object();
                    options.RequestServices = request.FunctionContext.InstanceServices;
                }
            );

        logger.LogInformation("GraphQL Request Executed.");

        response.StatusCode = HttpStatusCode.OK;
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(result, Encoding.UTF8);

        logger.LogInformation("GraphQL Request Completed.");
    }

    private async Task HandleUnsupportedMethodAsync(HttpRequestData request, HttpResponseData response)
    {
        logger.LogWarning("GraphQL unsupported HTTP method '{HttpMethod}'.", request.Method);
        response.StatusCode = HttpStatusCode.NotFound;
        await response.WriteStringAsync("Method not supported");
    }

    private class GraphiQLHttpContextAdapter : HttpContext
    {
        public GraphiQLHttpContextAdapter(HttpRequestData request, HttpResponseData response)
        {
            Request = new GraphiQLHttpRequestAdapter(request, this);
            Response = new GraphiQLHttpResponseAdapter(response, this);
        }

        public override IFeatureCollection Features => throw new NotImplementedException();

        public override HttpRequest Request { get; }

        public override HttpResponse Response { get; }

        public override ConnectionInfo Connection => throw new NotImplementedException();

        public override WebSocketManager WebSockets => throw new NotImplementedException();

        public override ClaimsPrincipal User { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override IDictionary<object, object?> Items { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override IServiceProvider RequestServices { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override CancellationToken RequestAborted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override string TraceIdentifier { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override ISession Session { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Abort()
        {
        }

        private class GraphiQLHttpRequestAdapter(HttpRequestData request, HttpContext context) : HttpRequest
        {
            private class RequestCookieCollection(IReadOnlyCollection<IHttpCookie> cookies) :
                Dictionary<string, string>(cookies.ToDictionary(cookie => cookie.Name, cookie => cookie.Value, StringComparer.OrdinalIgnoreCase)),
                IRequestCookieCollection
            {
                ICollection<string> IRequestCookieCollection.Keys => Keys;
            }

            public override HttpContext HttpContext { get; } = context;

            public override string Method { get; set; } = request.Method;

            public override string Scheme { get; set; } = request.Url.Scheme;

            public override bool IsHttps { get; set; } = "https".Equals(request.Url.Scheme, StringComparison.OrdinalIgnoreCase);

            public override HostString Host { get; set; } = new(request.Url.Host);

            public override PathString PathBase { get; set; } = new(request.Url.AbsolutePath);

            public override PathString Path { get; set; } = new(request.Url.AbsolutePath);

            public override QueryString QueryString { get; set; } = new(request.Url.Query);

            public override IQueryCollection Query { get; set; } = new QueryCollection(request.Query.AllKeys.Where(key => key is not null).ToDictionary(key => key!, key => new StringValues(request.Query[key]), StringComparer.OrdinalIgnoreCase));

            public override string Protocol { get; set; } = "HTTP/1.1";

            public override IHeaderDictionary Headers { get; } = new HeaderDictionary(request.Headers.ToDictionary(header => header.Key, header => new StringValues([.. header.Value]), StringComparer.OrdinalIgnoreCase));

            public override IRequestCookieCollection Cookies { get; set; } = new RequestCookieCollection(request.Cookies);

            public override long? ContentLength { get; set; } = request
                .Headers
                .Where(header => header.Key.Equals(HttpHeader.Names.ContentType, StringComparison.OrdinalIgnoreCase))
                .SelectMany(header => header.Value)
                .Select(long.Parse)
                .FirstOrDefault();

            public override string? ContentType { get; set; } = request
                .Headers
                .Where(header => header.Key.Equals(HttpHeader.Names.ContentType, StringComparison.OrdinalIgnoreCase))
                .SelectMany(header => header.Value)
                .FirstOrDefault();

            public override Stream Body { get; set; } = request.Body;

            public override bool HasFormContentType => false;

            public override IFormCollection Form { get; set; } = new FormCollection([]);

            public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = default)
                => Task.FromResult<IFormCollection>(new FormCollection([]));
        }

        private class GraphiQLHttpResponseAdapter(HttpResponseData response, HttpContext context) : HttpResponse
        {
            private class HeaderDictionary(HttpHeadersCollection headers) : IHeaderDictionary
            {
                public StringValues this[string key]
                {
                    get => new([.. headers.GetValues(key)]);
                    set
                    {
                        headers.Remove(key);
                        headers.Add(key, value.AsEnumerable());
                    }
                }

                public long? ContentLength
                {
                    get => headers.GetValues(HttpHeader.Names.ContentLength).Select(long.Parse).FirstOrDefault();
                    set
                    {
                        headers.Remove(HttpHeader.Names.ContentLength);
                        headers.Add(HttpHeader.Names.ContentLength, value.ToString());
                    }
                }

                public ICollection<string> Keys
                    => [.. headers.Select(header => header.Key)];

                public ICollection<StringValues> Values
                    => [.. headers.Select(header => new StringValues([.. header.Value]))];

                public int Count
                    => headers.Count();

                public bool IsReadOnly
                    => false;

                public void Add(string key, StringValues value)
                    => headers.Add(key, value.AsEnumerable());

                public void Add(KeyValuePair<string, StringValues> item)
                    => Add(item.Key, item.Value);

                public void Clear()
                    => headers.Clear();

                public bool Contains(KeyValuePair<string, StringValues> item)
                    => headers.Contains(new KeyValuePair<string, IEnumerable<string>>(item.Key, item.Value.AsEnumerable()!));

                public bool ContainsKey(string key)
                    => headers.Contains(key);

                public void CopyTo(KeyValuePair<string, StringValues>[] array, int arrayIndex)
                {
                    var index = arrayIndex;
                    foreach (var header in headers)
                    {
                        array[index] = new(header.Key, new([.. header.Value]));
                        index++;
                    }
                }

                public bool Remove(string key)
                    => headers.Remove(key);

                public bool Remove(KeyValuePair<string, StringValues> item)
                {
                    if (!headers.TryGetValues(item.Key, out var values))
                        return false;
                    else
                    {
                        headers.Remove(item.Key);
                        headers.Add(item.Key, values.Except(item.Value, StringComparer.OrdinalIgnoreCase));

                        return true;
                    }
                }

                public bool TryGetValue(string key, [MaybeNullWhen(false)] out StringValues value)
                {
                    if (!headers.TryGetValues(key, out var values))
                    {
                        value = new();
                        return false;
                    }
                    else
                    {
                        value = new([.. values]);
                        return true;
                    }
                }

                public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
                    => headers.Select(header => new KeyValuePair<string, StringValues>(header.Key, new([.. header.Value]))).GetEnumerator();

                IEnumerator IEnumerable.GetEnumerator()
                    => GetEnumerator();
            }

            private class ResponseCookies(HttpCookies cookies) : IResponseCookies
            {
                public void Append(string key, string value)
                    => cookies.Append(key, value);

                public void Append(string key, string value, CookieOptions options)
                    => cookies.Append(key, value);

                public void Delete(string key)
                {
                }

                public void Delete(string key, CookieOptions options)
                {
                }
            }

            public override HttpContext HttpContext => context;

            public override int StatusCode { get => (int)response.StatusCode; set => response.StatusCode = (HttpStatusCode)value; }

            public override IHeaderDictionary Headers { get; } = new HeaderDictionary(response.Headers);

            public override Stream Body
            {
                get => response.Body;
                set => response.Body = value;
            }

            public override long? ContentLength
            {
                get => Headers.ContentLength;
                set => Headers.ContentLength = value;
            }

            public override string? ContentType
            {
                get => Headers.ContentType;
                set => Headers.ContentType = value;
            }

            public override IResponseCookies Cookies { get; } = new ResponseCookies(response.Cookies);

            public override bool HasStarted
                => true;

            public override void OnCompleted(Func<object, Task> callback, object state)
                => throw new NotImplementedException();

            public override void OnStarting(Func<object, Task> callback, object state)
                => throw new NotImplementedException();

            public override void Redirect([StringSyntax("Uri")] string location, bool permanent)
                => throw new NotImplementedException();
        }
    }
}
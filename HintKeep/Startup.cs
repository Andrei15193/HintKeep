using System.Text.Json;
using System.Text.Json.Serialization;
using HintKeep.Storage;
using HintKeep.Storage.Azure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using HintKeep.Controllers.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;

namespace HintKeep
{
    public class Startup
    {
        private const string ObjectIdClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
            => _configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IEntityTables>(new AzureEntityTables(CloudStorageAccount.Parse(_configuration.GetConnectionString("AZURE_STORAGE")).CreateCloudTableClient()));

            services.AddHttpContextAccessor();
            services.AddScoped(serviceProvider =>
            {
                var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
                return httpContext.User.Identity.IsAuthenticated
                    ? new Session(httpContext.User.FindFirstValue(ObjectIdClaimType))
                    : null;
            });

            services.AddMediatR(typeof(Startup));

            services
                .AddControllers(options =>
                {
                    options.Filters.Add(new AuthorizeFilter());
                    options.Filters.Add<ExceptionFilter>();
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.AllowTrailingCommas = false;
                    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.IncludeFields = false;
                    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.Strict;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Disallow;
                    options.JsonSerializerOptions.WriteIndented = false;
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = actionContext => new UnprocessableEntityObjectResult(actionContext.ModelState);
                });

            services
                .AddAuthorization(
                    options =>
                    {
                        options.DefaultPolicy = new AuthorizationPolicyBuilder()
                            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                            .RequireAuthenticatedUser()
                            .RequireClaim(ObjectIdClaimType)
                            .Build();
                    }
                )
                .AddAuthentication(
                    options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    }
                )
                .AddJwtBearer(
                    options =>
                    {
                        var authenticationConfiguration = _configuration.GetSection("Authentication");
                        var tenantName = authenticationConfiguration.GetValue<string>("TenantName");
                        var tenantId = authenticationConfiguration.GetValue<string>("TenantId");
                        var applicationId = authenticationConfiguration.GetValue<string>("ApplicationId");
                        var policy = authenticationConfiguration.GetValue<string>("Policy");

                        options.SaveToken = false;
                        options.RequireHttpsMetadata = true;
                        options.MetadataAddress = $"https://{tenantName}.b2clogin.com/{tenantName}.onmicrosoft.com/{policy}/v2.0/.well-known/openid-configuration";
                        options.Events = new JwtBearerEvents
                        {
                            OnChallenge = context =>
                            {
                                _SetLoginHeader(context.Request, context.Response);
                                return Task.CompletedTask;
                            },
                            OnAuthenticationFailed = context =>
                            {
                                _SetLoginHeader(context.Request, context.Response);
                                return Task.CompletedTask;
                            }
                        };
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            ValidateIssuer = true,
                            ValidIssuer = $"https://{tenantName}.b2clogin.com/tfp/{tenantId}/v2.0/",
                            ValidateAudience = true,
                            ValidAudience = applicationId,
                            ValidateLifetime = true,
                            LifetimeValidator = (notBefore, expires, securityToken, validationParameters)
                                => notBefore != null && expires != null && notBefore.Value.ToUniversalTime() <= DateTime.UtcNow && DateTime.UtcNow < expires.Value.ToUniversalTime(),
                        };

                        void _SetLoginHeader(HttpRequest request, HttpResponse response)
                        {
                            var returnUrl = Uri.EscapeDataString(request.Scheme + "://" + request.Host + "/authentications");
                            response.Headers["x-login"] = $"https://{tenantName}.b2clogin.com/{tenantName}.onmicrosoft.com/oauth2/v2.0/authorize?p={policy}&client_id={applicationId}&nonce=defaultNonce&redirect_uri={returnUrl}&scope=openid&response_type=id_token&prompt=login";
                        }
                    }
                );

            services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "HintKeep", Version = "v1" }));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app
                    .UseDeveloperExceptionPage()
                    .UseSwagger()
                    .UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HintKeep v1"));

            app
                .UseHttpsRedirection()
                .UseRouting()
                .UseAuthorization()
                .UseDefaultFiles()
                .UseStaticFiles()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapFallbackToFile("/index.html");
                });
        }
    }
}

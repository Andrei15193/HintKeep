using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HintKeep.Controllers.Filters;
using HintKeep.Services;
using HintKeep.Services.Implementations;
using HintKeep.Storage;
using HintKeep.Storage.Azure;
using HintKeep.Storage.CloudStub;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace HintKeep
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
            => (_configuration, _webHostEnvironment) = (configuration, webHostEnvironment);

        public void ConfigureServices(IServiceCollection services)
        {
            if (_webHostEnvironment.IsDevelopment())
                services.AddSingleton<IEntityTables>(new CloudStubEntityTables(new FileTableStorageHandler()));
            else
                services.AddSingleton<IEntityTables>(new AzureEntityTables(CloudStorageAccount.Parse(_configuration.GetConnectionString("AZURE_STORAGE")).CreateCloudTableClient()));

            services
                .AddHttpContextAccessor()
                .AddScoped(serviceProvider =>
                {
                    var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
                    return httpContext.User.Identity.IsAuthenticated
                        ? new Session(httpContext.User.FindFirstValue(ClaimTypes.Name))
                        : null;
                });

            services
                .AddTransient(serviceProvider =>
                {
                    var emailServiceConfigSection = serviceProvider.GetService<IConfiguration>().GetSection(nameof(EmailService));

                    var serverCredentialsConfigSection = emailServiceConfigSection.GetSection(nameof(EmailServiceConfig.ServerCredentials));
                    var password = new SecureString();
                    foreach (var @char in serverCredentialsConfigSection.GetValue<string>(nameof(NetworkCredential.Password)))
                        password.AppendChar(@char);

                    return new EmailServiceConfig(
                        emailServiceConfigSection.GetValue<string>(nameof(EmailServiceConfig.SenderEmailAddress)),
                        emailServiceConfigSection.GetValue<string>(nameof(EmailServiceConfig.SenderDisplayNameAddress)),
                        emailServiceConfigSection.GetValue<string>(nameof(EmailServiceConfig.ServerAddress)),
                        emailServiceConfigSection.GetValue<int>(nameof(EmailServiceConfig.ServerPort)),
                        new NetworkCredential(serverCredentialsConfigSection.GetValue<string>(nameof(NetworkCredential.UserName)), password)
                    );
                })
                .AddTransient(serviceProvider =>
                {
                    var sessionServiceConfigSection = serviceProvider.GetService<IConfiguration>().GetSection(nameof(SessionService));

                    return new SessionServiceConfig(
                        sessionServiceConfigSection.GetValue<string>(nameof(SessionServiceConfig.ApplicationId)),
                        sessionServiceConfigSection.GetValue<string>(nameof(SessionServiceConfig.Audience)),
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(sessionServiceConfigSection.GetValue<string>(nameof(SessionServiceConfig.SigningKey)))),
                        (string)typeof(SecurityAlgorithms)
                            .GetField(
                                sessionServiceConfigSection.GetValue<string>(nameof(SessionServiceConfig.SingingAlgorithm)),
                                BindingFlags.Public | BindingFlags.Static | BindingFlags.GetField
                            )
                            .GetValue(default)
                    );
                })
                .AddTransient(serviceProvider =>
                {
                    var securityServiceConfigSection = serviceProvider.GetService<IConfiguration>().GetSection(nameof(SecurityService));

                    return new SecurityServiceConfig(
                        securityServiceConfigSection.GetValue<string>(nameof(SecurityServiceConfig.HashAlgorithm)),
                        securityServiceConfigSection.GetValue<int>(nameof(SecurityServiceConfig.SaltLength)),
                        securityServiceConfigSection.GetValue<string>(nameof(SecurityServiceConfig.PasswordFormat)),
                        securityServiceConfigSection.GetValue<int>(nameof(SecurityServiceConfig.ActivationTokenLength)),
                        securityServiceConfigSection.GetValue<int>(nameof(SecurityServiceConfig.ActivationTokenExpirationMinutes))
                    );
                });

            if (_webHostEnvironment.IsDevelopment())
                services.AddTransient<IEmailService, EmailServiceMock>();
            else
                services.AddTransient<IEmailService, EmailService>();

            services
                .AddTransient<ISecurityService, SecurityService>()
                .AddTransient<ISessionService, SessionService>()
                .AddMediatR(typeof(Startup))
                .AddControllers(options =>
                {
                    options.Filters.Add(new AuthorizeFilter());
                    options.Filters.Add<ExceptionFilter>();
                    options.Filters.Add(new ResponseCacheAttribute { NoStore = true });
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
                            .RequireClaim(ClaimTypes.Name)
                            .RequireClaim(ClaimTypes.Role)
                            .RequireClaim(JwtRegisteredClaimNames.Jti)
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
                        var sessionServiceConfigSection = _configuration.GetSection(nameof(SessionService));

                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(sessionServiceConfigSection.GetValue<string>(nameof(SessionServiceConfig.SigningKey)))),
                            ValidIssuer = sessionServiceConfigSection.GetValue<string>(nameof(SessionServiceConfig.ApplicationId)),
                            ValidateIssuer = true,
                            ValidAudience = sessionServiceConfigSection.GetValue<string>(nameof(SessionServiceConfig.Audience)),
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            LifetimeValidator = (notBefore, expires, securityToken, validationParameters)
                                => notBefore != null && expires != null && notBefore.Value.ToUniversalTime() <= DateTime.UtcNow && DateTime.UtcNow < expires.Value.ToUniversalTime(),
                        };
                    }
                );

            services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "HintKeep", Version = "v1" }));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var rewriteOptions = new RewriteOptions()
                .AddRedirect(@"http://hintkeep\.net", "https://www.hintkeep.net")
                .AddRedirect(@"https://hintkeep\.net", "https://www.hintkeep.net");

            if (env.IsDevelopment())
                app
                    .UseDeveloperExceptionPage()
                    .UseSwagger()
                    .UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HintKeep v1"));

            app
                .Use(async (context, next) =>
                {
                    if (context.Request.Host.Host == "hintkeep.net")
                        context.Response.Redirect("https://www.hintkeep.net");
                    else
                        await next();
                })
                .UseHttpsRedirection()
                .UseRouting()
                .UseAuthorization()
                .UseDefaultFiles()
                .UseStaticFiles()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapFallbackToFile("/index.html");
                })
                .UseResponseCaching();
        }
    }
}

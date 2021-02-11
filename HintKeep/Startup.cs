using System.Text.Json;
using System.Text.Json.Serialization;
using HintKeep.Storage;
using HintKeep.Storage.Azure;
using HintKeep.Services;
using HintKeep.Services.Implementations;
using HintKeep.Services.Implementations.Configs;
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
using Microsoft.IdentityModel.Tokens;
using System;
using Microsoft.AspNetCore.Http;

namespace HintKeep
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
            => _configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            var jsonWebTokenServiceConfig = new JsonWebTokenServiceConfig(_configuration.GetSection(nameof(JsonWebTokenService)));

            services.AddSingleton<IEntityTables>(new AzureEntityTables(CloudStorageAccount.Parse(_configuration.GetConnectionString("AZURE_STORAGE")).CreateCloudTableClient()));

            services.AddHttpContextAccessor();
            services.AddScoped(serviceProvider =>
            {
                var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
                return httpContext.User.Identity.IsAuthenticated
                    ? new Session(httpContext.User.FindFirstValue(ClaimTypes.Name), httpContext.User.FindFirstValue(ClaimTypes.SerialNumber))
                    : null;
            });

            services.AddTransient(serviceProvider => new CryptographicHashServiceConfig(_configuration.GetSection(nameof(CryptographicHashService))));
            services.AddSingleton(new EmailServiceConfig(_configuration.GetSection(nameof(EmailService))));
            services.AddSingleton(new SaltServiceConfig(_configuration.GetSection(nameof(SaltService))));
            services.AddSingleton(jsonWebTokenServiceConfig);

            services.AddSingleton<IRngService, RngService>();
            services.AddTransient<ICryptographicHashService, CryptographicHashService>();
            services.AddTransient<ISaltService, SaltService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IJsonWebTokenService, JsonWebTokenService>();

            services.AddMediatR(config => config.Using<Mediator>().AsSingleton(), typeof(Startup).Assembly);

            services
                .AddControllers(options =>
                {
                    options.Filters.Add(new AuthorizeFilter());
                    options.Filters.Add<ActiveSessionAuthorizationFilter>();
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
                            .RequireClaim(ClaimTypes.Name)
                            .RequireClaim(ClaimTypes.SerialNumber)
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
                        options.RequireHttpsMetadata = true;
                        options.SaveToken = false;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = jsonWebTokenServiceConfig.SigningKey,
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = true,
                            LifetimeValidator = (notBefore, expires, securityToken, validationParameters)
                                => notBefore != null && expires != null && notBefore.Value.ToUniversalTime() <= DateTime.UtcNow && DateTime.UtcNow < expires.Value.ToUniversalTime()
                        };
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
                .UseEndpoints(endpoints => endpoints.MapControllers())
                .UseDefaultFiles()
                .UseStaticFiles();
        }
    }
}

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

namespace HintKeep
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
            => _configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IEntityTables>(new AzureEntityTables(CloudStorageAccount.Parse(_configuration.GetConnectionString("AZURE_STORAGE")).CreateCloudTableClient()));

            services.AddTransient(serviceProvider => new CryptographicHashServiceConfig(_configuration.GetSection(nameof(CryptographicHashService))));
            services.AddSingleton(new EmailServiceConfig(_configuration.GetSection(nameof(EmailService))));
            services.AddSingleton(new SaltServiceConfig(_configuration.GetSection(nameof(SaltService))));

            services.AddSingleton<IRngService, RngService>();
            services.AddTransient<ICryptographicHashService, CryptographicHashService>();
            services.AddTransient<ISaltService, SaltService>();
            services.AddTransient<IEmailService, EmailService>();

            services.AddMediatR(config => config.Using<Mediator>().AsSingleton(), typeof(Startup).Assembly);

            services
                .AddControllers()
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

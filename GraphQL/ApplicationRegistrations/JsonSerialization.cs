using System.Text.Json;
using System.Text.Json.Serialization;
using HintKeep.GraphQL.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HintKeep.GraphQL.ApplicationRegistrations;

public static class JsonSerialization
{
    public static IServiceCollection AddJsonSerialization(this IServiceCollection services)
        => services
            .AddSingleton(services => new JsonSerializerOptions
            {
                WriteIndented = services.GetRequiredService<IHostEnvironment>().IsDevelopment(),
                IndentSize = 4,
                IndentCharacter = ' ',
                NewLine = "\n",

                RespectNullableAnnotations = true,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
                ReadCommentHandling = JsonCommentHandling.Disallow,
                PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,

                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                IncludeFields = false,
                IgnoreReadOnlyFields = true,
                IgnoreReadOnlyProperties = false,

                AllowTrailingCommas = false,
                PropertyNameCaseInsensitive = false,
                NumberHandling = JsonNumberHandling.Strict,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false),
                    new UtcDateTimeConverter()
                }
            });
}
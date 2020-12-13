using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace HintKeep.Tests.Unit.Services
{
    public static class ServiceConfigFactory
    {
        public static TConfig Create<TConfig>(object config)
            => (TConfig)Activator.CreateInstance(typeof(TConfig), _GetConfigurationSectionFrom(config));

        private static IConfigurationSection _GetConfigurationSectionFrom(object config)
        {
            var configurationBuilder = new ConfigurationBuilder();

            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream, encoding: Encoding.UTF8, leaveOpen: true))
                {
                    streamWriter.Write(JsonSerializer.Serialize(new { Test = config }, new JsonSerializerOptions { PropertyNamingPolicy = null }));
                    streamWriter.Flush();
                }
                memoryStream.Seek(0L, SeekOrigin.Begin);
                return configurationBuilder.AddJsonStream(memoryStream).Build().GetSection("Test");
            }
        }
    }
}
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace HintKeep
{
    internal static class Program
    {
        internal static void Main(string[] args)
            => Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>()).Build().Run();
    }
}
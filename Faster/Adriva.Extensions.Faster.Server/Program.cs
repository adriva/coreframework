using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;

namespace Adriva.Extensions.Faster.Server
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var startup = new Startup(builder.Environment, builder.Configuration);

            startup.ConfigureServices(builder.Services);

            var application = builder.Build();
            startup.ConfigurePipeline(application);
            application.Run();
        }
    }
}
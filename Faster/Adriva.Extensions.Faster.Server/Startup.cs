using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Adriva.Extensions.Faster.Server
{
    internal sealed class Startup
    {
        private readonly IWebHostEnvironment Environment;
        private readonly IConfiguration Configuration;

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            this.Environment = environment;
            this.Configuration = configuration;
        }

        internal void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication();
            services.AddAuthorization();
            services.AddMvc();
            services.AddCors();
            services.AddFaster(options =>
            {
                options.UseLocks = true;
            });
        }

        internal void ConfigurePipeline(WebApplication application)
        {
            if (this.Environment.IsDevelopment())
            {
                application.UseDeveloperExceptionPage();
            }

            application.UseAuthentication();

            application.UseRouting();
            application.UseAuthorization();
            application.UseFasterStorageApi();
        }

    }
}
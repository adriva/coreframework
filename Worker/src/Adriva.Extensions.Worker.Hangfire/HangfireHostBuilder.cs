using System;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Adriva.Extensions.Worker.Hangfire;

internal class HangfireHostBuilder(IServiceCollection services) : IHangfireHostBuilder
{
    private readonly IServiceCollection Services = services;

    public IHangfireHostBuilder WithSerializerSettings(Action<JsonSerializerSettings> configure)
    {
        JsonSerializerSettings jsonSerializerSettings = new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        configure(jsonSerializerSettings);

        GlobalConfiguration.Configuration.UseSerializerSettings(jsonSerializerSettings);
        return this;
    }

    public IHangfireHostBuilder WithJobActivator<T>() where T : JobActivator
    {
        this.Services.AddSingleton<JobActivator, T>();
        return this;
    }

    public IHangfireHostBuilder WithPlatformOptions(Action<BackgroundJobServerOptions> configure)
    {
        this.Services.Configure(configure);
        return this;
    }
}

using System;
using Hangfire;
using Newtonsoft.Json;

namespace Adriva.Extensions.Worker.Hangfire;

public interface IHangfireHostBuilder
{
    IHangfireHostBuilder WithSerializerSettings(Action<JsonSerializerSettings> configure);

    IHangfireHostBuilder WithJobActivator<T>() where T : JobActivator;

    IHangfireHostBuilder WithPlatformOptions(Action<BackgroundJobServerOptions> configure);
}

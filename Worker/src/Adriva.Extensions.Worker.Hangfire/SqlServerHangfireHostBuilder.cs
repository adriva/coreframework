using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Worker.Hangfire;

internal sealed class SqlServerHangfireHostBuilder(IServiceCollection services) : HangfireHostBuilder(services), ISqlServerHangfireHostBuilder
{

}
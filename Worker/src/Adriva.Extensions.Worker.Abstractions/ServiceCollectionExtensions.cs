using Adriva.Extensions.Worker.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWorkerHost(this IServiceCollection services, Action<IWorkerHostBuilder> build)
    {
        WorkerHostBuilder workerHostBuilder = new(services);
        build(workerHostBuilder);

        return services;
    }
}
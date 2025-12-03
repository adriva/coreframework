using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Worker.Abstractions;

public interface IWorkerHostBuilder
{
    IServiceCollection Services { get; }
}

internal sealed class WorkerHostBuilder(IServiceCollection services) : IWorkerHostBuilder
{
    public IServiceCollection Services { get; } = services;
}


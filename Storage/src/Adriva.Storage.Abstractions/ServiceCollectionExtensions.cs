using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IO;

namespace Adriva.Storage.Abstractions;

public interface IStorageServicesBuilder
{
    IServiceCollection Services { get; }
}

public static class ServiceCollectionExtensions
{
    private sealed class StorageServicesBuilder(IServiceCollection services) : IStorageServicesBuilder
    {
        public IServiceCollection Services { get; private set; } = services;
    }

    public static IStorageServicesBuilder AddStorageServices(this IServiceCollection services)
    {
        services.TryAddSingleton<RecyclableMemoryStreamManager>();
        return new StorageServicesBuilder(services);
    }

    public static IServiceCollection AddBlobManager<TBlob, TManager>(this IServiceCollection services)
        where TBlob : class, IBlob
        where TManager : class, IBlobManager<TBlob>
    {
        services.TryAddScoped<IBlobManager<TBlob>, TManager>();
        services.TryAddScoped(serviceProvider => serviceProvider.GetServices<IBlobManager<TBlob>>().OfType<TManager>().First());

        return services;
    }
}
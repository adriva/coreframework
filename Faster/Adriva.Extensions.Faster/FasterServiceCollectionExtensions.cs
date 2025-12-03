using System;
using Adriva.Extensions.Faster;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FasterServiceCollectionExtensions
    {
        public static IServiceCollection AddFaster(this IServiceCollection services, Action<FasterOptions> configure)
        {
            services.AddSingleton<StorageMiddleware>();
            services.AddSingleton<IFasterStorageClient, FasterStorageService>();
            services.AddHostedService(serviceProvider => (FasterStorageService)serviceProvider.GetRequiredService<IFasterStorageClient>());

            services.Configure(configure);
            return services;
        }
    }
}
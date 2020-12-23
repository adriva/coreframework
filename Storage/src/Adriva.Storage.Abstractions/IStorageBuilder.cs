using System;
using Adriva.Storage.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IStorageBuilder
    {
        IServiceCollection Services { get; }

        IStorageBuilder AddQueueClient<TClient, TOptions>(string name, Action<TOptions> configure)
                                                where TClient : class, IQueueClient
                                                where TOptions : class, new();

        IStorageBuilder AddBlobClient<TClient, TOptions>(string name, Action<TOptions> configure)
                where TClient : class, IBlobClient
                where TOptions : class, new();
    }
}
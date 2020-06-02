using System;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Storage.Abstractions
{
    public interface IStorageClientBuilder
    {
        string Name { get; }

        IServiceCollection Services { get; }

        IStorageClientBuilder Configure<TOptions>(Action<TOptions> configure) where TOptions : class, new();
    }
}
using System;
using System.Collections.ObjectModel;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public interface IOptimizationContext : IAsyncDisposable
    {
        string Identifier { get; }

        ReadOnlyCollection<Asset> Assets { get; }

        void AddAsset(string pathOrUrl);

    }
}
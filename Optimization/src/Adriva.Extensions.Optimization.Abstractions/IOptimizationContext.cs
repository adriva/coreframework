using System;
using System.Collections.ObjectModel;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public interface IOptimizationContext : IDisposable
    {
        string Identifier { get; }

        ReadOnlyCollection<Asset> Assets { get; }

        void AddAsset(string pathOrUrl);

    }
}
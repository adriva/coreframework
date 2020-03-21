using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public sealed class OptimizationResult : IEnumerable<Asset>, IEnumerable, IDisposable
    {
        private readonly IEnumerable<Asset> Assets;

        public OptimizationResult(IEnumerable<Asset> assets)
        {
            this.Assets = assets ?? Enumerable.Empty<Asset>();
        }

        public IEnumerator<Asset> GetEnumerator() => this.Assets.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.Assets.GetEnumerator();

        public void Dispose()
        {
            foreach (var asset in this.Assets)
            {
                asset.Dispose();
            }
        }
    }
}
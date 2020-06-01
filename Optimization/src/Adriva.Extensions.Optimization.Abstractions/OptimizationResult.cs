using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public sealed class OptimizationResult : IEnumerable<Asset>, IEnumerable, IAsyncDisposable
    {
        public static OptimizationResult Empty => new OptimizationResult() { Assets = Enumerable.Empty<Asset>() };

        private IEnumerable<Asset> Assets;

        public async ValueTask InitializeAsync(IEnumerable<Asset> assets)
        {
            if (null == assets) return;

            foreach (var asset in assets)
            {
                var originalContent = asset.Content;
                await asset.SetContentAsync(originalContent, true);
                await originalContent.DisposeAsync();
            }

            this.Assets = assets;
        }

        public IEnumerator<Asset> GetEnumerator() => this.Assets.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.Assets.GetEnumerator();

        public async ValueTask DisposeAsync()
        {
            foreach (var asset in this.Assets)
            {
                await asset.DisposeAsync();
            }
        }
    }
}
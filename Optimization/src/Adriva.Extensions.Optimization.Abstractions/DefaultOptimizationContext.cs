using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Adriva.Common.Core;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public sealed class DefaultOptimizationContext : IOptimizationContext
    {
        private readonly List<Asset> AssetList = new List<Asset>();

        public ReadOnlyCollection<Asset> Assets => new ReadOnlyCollection<Asset>(this.AssetList);

        public string Identifier { get; private set; }

        public void AddAsset(string pathOrUrl)
        {
            Asset asset = new Asset(pathOrUrl);
            if (this.AssetList.Any(a => a.Location == asset.Location)) return;
            this.AssetList.Add(asset);

            this.Identifier = Utilities.CalculateBinaryHash(string.Join("|", this.AssetList.OrderBy(a => a.Name).Select(a => a.Name)));
        }

        public void Dispose()
        {
            foreach (var asset in this.AssetList)
            {
                asset.Dispose();
            }
        }
    }
}
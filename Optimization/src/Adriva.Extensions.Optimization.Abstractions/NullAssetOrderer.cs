using System.Collections.Generic;
using System.Linq;

namespace Adriva.Extensions.Optimization.Abstractions
{
    internal class NullAssetOrderer : IAssetOrderer
    {
        public IEnumerable<Asset> Order(IEnumerable<Asset> assets)
        {
            if (null == assets) return Enumerable.Empty<Asset>();
            return assets;
        }
    }
}
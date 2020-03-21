using System.Collections.Generic;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public interface IAssetOrderer
    {
        IEnumerable<Asset> Order(IEnumerable<Asset> assets);
    }
}
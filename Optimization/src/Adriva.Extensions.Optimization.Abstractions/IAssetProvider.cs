using System.Collections.Generic;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public interface IAssetProvider
    {
        IEnumerable<string> GetAssetPaths(AssetFileExtension extension);
    }
}

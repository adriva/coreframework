using System.Collections.Generic;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public interface IAssetProvider
    {
        IEnumerable<AssetFileExtension> GetAssetFileExtensions();

        IEnumerable<string> GetAssetPaths(AssetFileExtension extension);
    }
}

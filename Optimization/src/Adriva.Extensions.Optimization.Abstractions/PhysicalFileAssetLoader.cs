using System.IO;
using System.Threading.Tasks;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public sealed class PhysicalFileAssetLoader : IAssetLoader
    {
        public bool CanLoad(Asset asset)
        {
            return asset.Location.IsFile;
        }

        public ValueTask<Stream> OpenReadStreamAsync(Asset asset)
        {
            return new ValueTask<Stream>(File.Open(asset.Location.AbsolutePath, FileMode.Open, FileAccess.Read));
        }
    }

}
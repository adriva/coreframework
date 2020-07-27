using System.IO;
using System.Threading.Tasks;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public class PhysicalFileAssetLoader : IAssetLoader
    {
        public virtual bool CanLoad(Asset asset)
        {
            return asset.Location.IsAbsoluteUri && asset.Location.IsFile;
        }

        public virtual ValueTask<Stream> OpenReadStreamAsync(Asset asset)
        {
            return new ValueTask<Stream>(File.Open(asset.Location.AbsolutePath, FileMode.Open, FileAccess.Read, FileShare.Read));
        }
    }

}
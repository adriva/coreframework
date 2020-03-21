using System.IO;
using System.Threading.Tasks;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public interface IAssetLoader
    {
        bool CanLoad(Asset asset);

        ValueTask<Stream> OpenReadStreamAsync(Asset asset);
    }

}
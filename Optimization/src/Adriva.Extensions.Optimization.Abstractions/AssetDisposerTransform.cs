using System.Collections.Generic;
using System.Threading.Tasks;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public abstract class AssetDisposerTransform : ITransform
    {
        public abstract Task<IEnumerable<Asset>> TransformAsync(string extension, params Asset[] assets);

        public virtual async ValueTask CleanUpAsync(params Asset[] assets)
        {
            if (null == assets || 0 == assets.Length) return;

            foreach (var asset in assets)
            {
                await asset.DisposeAsync();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public abstract class SingleTranform : ITransform
    {
        public abstract Task<Asset> TransformAsync(Asset asset);

        public virtual async Task<IEnumerable<Asset>> TransformAsync(params Asset[] assets)
        {
            if (null == assets) throw new ArgumentNullException(nameof(assets));

            int count = assets.Count();
            if (1 != count) throw new ArgumentException($"SingleTranform can transform one asset at a time. You have provided '{count}' assets.");

            Asset output = await this.TransformAsync(assets[0]);
            return new Asset[] { output };
        }
    }

}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Adriva.Extensions.Optimization.Abstractions;

namespace Adriva.Extensions.Optimization.Transforms
{
    /// <summary>
    /// Bundle transform to create a bundled asset from multiple style sheet assets.
    /// </summary>
    public class StylesheetBundleTransform : MergeContentTransform
    {
        protected override string GetSeperator(Asset asset) => Environment.NewLine;

        public async override Task<IEnumerable<Asset>> TransformAsync(string extension, params Asset[] assets)
        {
            return await base.TransformAsync(extension, assets);
        }
    }
}

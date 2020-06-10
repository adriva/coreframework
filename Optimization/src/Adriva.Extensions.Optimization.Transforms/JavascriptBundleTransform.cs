using System;
using Adriva.Extensions.Optimization.Abstractions;

namespace Adriva.Extensions.Optimization.Transforms
{
    /// <summary>
    /// Bundle transform to create a bundled asset from multiple javascript assets.
    /// </summary>
    public class JavascriptBundleTransform : MergeContentTransform
    {
        protected override string GetSeperator(Asset asset) => $";{Environment.NewLine}";
    }
}

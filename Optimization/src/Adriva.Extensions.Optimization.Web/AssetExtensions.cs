using System;
using Adriva.Extensions.Optimization.Abstractions;
using Microsoft.AspNetCore.Hosting;

namespace Adriva.Extensions.Optimization.Web
{
    internal static class AssetExtensions
    {
        /// <summary>
        /// Gets the location of the asset that represents either the absolute or relative path that can be used in html.
        /// </summary>
        /// <param name="asset">The asset that the content will be read.</param>
        /// <param name="hostingEnvironment">Current hosting environment.</param>
        /// <param name="options">Currently active instance of WebOptimizationOptions class that will be used to construct a Url or path.</param>
        /// <returns><A string that represents either the relative or the absolute path of the asset.</returns>
        public static string GetWebLocation(this Asset asset, IWebHostEnvironment hostingEnvironment, WebOptimizationOptions options = null)
        {
            if (null == asset) throw new ArgumentNullException(nameof(asset));
            if (null == asset.Location) throw new ArgumentNullException(nameof(asset.Location));

            string assetLocation;

            if (!asset.Location.IsAbsoluteUri)
            {
                if (asset.Location.OriginalString.StartsWith("/")) assetLocation = Convert.ToString(asset.Location.OriginalString);
                else assetLocation = string.Concat("/", asset.Location.OriginalString);
            }
            else
            {
                if (asset.Location.IsFile)
                {
                    Uri webRootUri = new Uri(hostingEnvironment.WebRootPath.EndsWith("/", StringComparison.Ordinal) ? hostingEnvironment.WebRootPath : hostingEnvironment.WebRootPath + "/");
                    assetLocation = webRootUri.MakeRelativeUri(asset.Location).OriginalString;
                }
                else return Convert.ToString(asset.Location);
            }

            return $"{options?.AssetRootUrl}{assetLocation}";
        }
    }
}
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Adriva.Extensions.Optimization.Abstractions
{
    /// <summary>
    /// Provides extension methods that can be used with an asset.
    /// </summary>
    public static class AssetExtensions
    {
        /// <summary>
        /// Returns a copy of the asset and overwrites the content of the new instance.
        /// </summary>
        /// <param name="asset">The source asset instance that will be cloned.</param>
        /// <param name="content">Content of the new asset.</param>
        /// <returns>A task that represents the asynchronous operation. The value of the Result parameter is the asset that is a clone of the source asset.</returns>
        public static async Task<Asset> CloneWithContentAsync(this Asset asset, string content)
        {
            if (null == asset) throw new ArgumentNullException(nameof(asset));
            if (null == asset.Content) throw new ArgumentNullException(nameof(asset.Content));

            Asset output = new Asset(asset.Location);
            await output.SetContentAsync(content);
            return output;
        }

        /// <summary>
        /// Reads the content of an asset as a string value.
        /// </summary>
        /// <param name="asset">The asset that the content will be read.</param>
        /// <returns>A task that represents the asynchronous operation. The value of the Result parameter is the string representation of the content of the asset.</returns>
        public static async Task<string> ReadContentAsStringAsync(this Asset asset)
        {
            if (null == asset) throw new ArgumentNullException(nameof(asset));
            if (null == asset.Content) throw new ArgumentNullException(nameof(asset.Content));

            using (StreamReader reader = new StreamReader(asset.Content, Encoding.UTF8, false, 4096, true))
            {
                asset.Content.Seek(0, SeekOrigin.Begin);
                return await reader.ReadToEndAsync();
            }
        }
    }
}
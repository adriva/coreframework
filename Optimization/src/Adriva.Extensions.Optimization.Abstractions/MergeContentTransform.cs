using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adriva.Common.Core;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public class MergeContentTransform : AssetDisposerTransform
    {
        protected virtual string GetSeperator(Asset asset) => Environment.NewLine;

        public async override Task<IEnumerable<Asset>> TransformAsync(params Asset[] assets)
        {
            AutoStream outputStream = new AutoStream(1024 * 64);

            foreach (var asset in assets)
            {
                string seperator = this.GetSeperator(asset);
                byte[] seperatorBytes = string.IsNullOrEmpty(seperator) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(seperator);
                await asset.Content.CopyToAsync(outputStream);
                if (0 < seperatorBytes.Length)
                {
                    await outputStream.WriteAsync(seperatorBytes, 0, seperatorBytes.Length);
                }
            }
            outputStream.Seek(0, SeekOrigin.Begin);

            string hashName = Utilities.GetBaseString(Utilities.CalculateStreamHash(outputStream), Utilities.Base63Alphabet);

            outputStream.Seek(0, SeekOrigin.Begin);

            string outputExtension = Path.GetExtension(assets.FirstOrDefault()?.Name ?? string.Empty);

            return new[] { new Asset($"{hashName}{outputExtension}", outputStream) };
        }
    }
}
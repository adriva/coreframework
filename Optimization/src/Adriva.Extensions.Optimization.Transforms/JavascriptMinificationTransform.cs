using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adriva.Extensions.Optimization.Abstractions;
using NUglify;
using NUglify.JavaScript;

namespace Adriva.Extensions.Optimization.Transforms
{
    public class JavascriptMinificationTransform : AssetDisposerTransform
    {
        public override async Task<IEnumerable<Asset>> TransformAsync(params Asset[] assets)
        {
            CodeSettings codeSettings = new CodeSettings()
            {
                AmdSupport = true,
                ScriptVersion = ScriptVersion.None,
                PreserveImportantComments = false
            };

            List<Asset> outputs = new List<Asset>();
            foreach (var asset in assets)
            {
                string code = await asset.ReadContentAsStringAsync();
                UglifyResult uglifyResult = Uglify.Js(code, codeSettings);

                if (uglifyResult.HasErrors)
                {
                    throw new AggregateException(uglifyResult.Errors.Select(x => new Exception(x.Message)));
                }

                Asset outputAsset = await asset.CloneWithContentAsync(uglifyResult.Code);
                outputs.Add(outputAsset);
            }
            return outputs.AsEnumerable();
        }
    }
}

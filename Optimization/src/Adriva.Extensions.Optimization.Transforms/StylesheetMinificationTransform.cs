using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adriva.Extensions.Optimization.Abstractions;
using NUglify;
using NUglify.JavaScript;
using NUglify.Css;
using Microsoft.Extensions.Options;

namespace Adriva.Extensions.Optimization.Transforms
{
    public class StylesheetMinificationTransform : AssetDisposerTransform
    {
        private readonly StylesheetMinificationOptions MinificationOptions;

        public StylesheetMinificationTransform(IOptions<StylesheetMinificationOptions> optionsAccessor)
        {
            this.MinificationOptions = optionsAccessor.Value;
        }

        public override async Task<IEnumerable<Asset>> TransformAsync(string extension, params Asset[] assets)
        {
            CssSettings cssSettings = new CssSettings()
            {
                CommentMode = CssComment.None
            };

            if (0 < this.MinificationOptions.Substitutions.Count)
            {
                cssSettings.ReplacementTokensApplyDefaults(this.MinificationOptions.Substitutions);
            }

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
                UglifyResult uglifyResult = Uglify.Css(code, cssSettings, codeSettings);

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

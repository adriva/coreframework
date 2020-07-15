using System.Threading.Tasks;
using Adriva.Extensions.Caching.Abstractions;
using Adriva.Extensions.Optimization.Abstractions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace Adriva.Extensions.Optimization.Web
{
    /// <summary>
    /// Creates an HTML tag to access an optimized resource within the Html output.
    /// </summary>
    [HtmlTargetElement("optimizedresource", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class OptimizedResourceTagHelper : TagHelper, ITagBuilderOptions
    {
        private readonly IOptimizationScope OptimizationScope;
        private readonly IOptimizationManager OptimizationManager;
        private readonly IOptimizationResultTagBuilderFactory TagBuilderFactory;
        private readonly ICache Cache;

        [HtmlAttributeName("context")]
        public IOptimizationContext Context { get; set; }

        /// <summary>
        /// Gets or sets the extension of assets that will be rendered in this tag.
        /// </summary>
        /// <value>A string value representing the extension of the assets.</value>
        [HtmlAttributeName("extension")]
        public AssetFileExtension Extension { get; set; }

        /// <summary>
        /// Gets or sets the output mode of the tag.
        /// </summary>
        /// <value>An OptimizationTagOutput enumeration value representing the output mode.</value>
        [HtmlAttributeName("Output")]
        public OptimizationTagOutput Output { get; set; }

        public OptimizedResourceTagHelper(
                    IOptimizationManager optimizationManager,
                    IOptimizationScope optimizationScope,
                    IOptimizationResultTagBuilderFactory tagBuilderFactory,
                    IOptions<WebOptimizationOptions> optionsAccessor,
                    ICache cache)
        {
            this.OptimizationScope = optimizationScope;
            this.OptimizationManager = optimizationManager;
            this.TagBuilderFactory = tagBuilderFactory;
            this.Cache = cache;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (null == this.Context)
                throw new System.ArgumentException("<optimizedresource> tag requires a valid OptimizationContext. Have you forgotten to set the 'context' attribute ?");

            string cacheKey = $"{this.Context.Identifier}+{this.Extension}";

            OptimizationResult optimizationResult = await this.Cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                return await this.OptimizationManager.OptimizeAsync(this.Context, this.Extension);
            });

            output.SuppressOutput();

            IOptimizationResultTagBuilder tagBuilder = this.TagBuilderFactory.GetBuilder(this.Extension);

            foreach (var asset in optimizationResult)
            {
                HtmlContentBuilder assetContentBuilder = new HtmlContentBuilder();
                await tagBuilder.PopulateHtmlTagAsync(this, context.AllAttributes, asset, assetContentBuilder);
                output.PostElement.AppendHtml(assetContentBuilder);

                // if the asset is written out to a static file
                // we can safely dispose the asset (the content) sine it will be served from a static file
                if (OptimizationTagOutput.StaticFile == this.Output)
                {
                    await asset.DisposeAsync();
                }
            }

        }
    }
}

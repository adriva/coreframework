using System.Threading.Tasks;
using Adriva.Extensions.Caching.Abstractions;
using Adriva.Extensions.Optimization.Abstractions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Adriva.Extensions.Optimization.Web
{
    /// <summary>
    /// Creates an HTML tag to access an optimized resource within the Html output.
    /// </summary>
    [HtmlTargetElement("optimizedresource", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class OptimizedResourceTagHelper : TagHelper, ITagBuilderOptions
    {
        private readonly IOptimizationContext OptimizationContext;
        private readonly IOptimizationManager OptimizationManager;
        private readonly IOptimizationResultTagBuilderFactory TagBuilderFactory;
        private readonly ICache Cache;

        /// <summary>
        /// Gets or sets the extension of assets that will be rendered in this tag.
        /// </summary>
        /// <value>A string value representing the extension of the assets.</value>
        [HtmlAttributeName("extension")]
        public string Extension { get; set; }

        /// <summary>
        /// Gets or sets the output mode of the tag.
        /// </summary>
        /// <value>An OptimizationTagOutput enumeration value representing the output mode.</value>
        [HtmlAttributeName("Output")]
        public OptimizationTagOutput Output { get; set; }

        public OptimizedResourceTagHelper(IOptimizationManager optimizationManager, IOptimizationContext optimizationContext, IOptimizationResultTagBuilderFactory tagBuilderFactory, ICache cache)
        {
            this.OptimizationContext = optimizationContext;
            this.OptimizationManager = optimizationManager;
            this.TagBuilderFactory = tagBuilderFactory;
            this.Cache = cache;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            string cacheKey = $"{this.OptimizationContext.Identifier}+{this.Extension}";
            OptimizationResult optimizationResult = await this.Cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                return await this.OptimizationManager.OptimizeAsync(this.OptimizationContext, this.Extension);
            });

            output.SuppressOutput();


            IOptimizationResultTagBuilder tagBuilder = this.TagBuilderFactory.GetBuilder(this.Extension);

            foreach (var asset in optimizationResult)
            {
                HtmlContentBuilder assetContentBuilder = new HtmlContentBuilder();
                await tagBuilder.PopulateHtmlTagAsync(this, asset, assetContentBuilder);
                output.PostElement.AppendHtml(assetContentBuilder);
            }

        }
    }
}

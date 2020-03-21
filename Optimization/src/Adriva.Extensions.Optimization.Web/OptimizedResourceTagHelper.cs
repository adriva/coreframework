using System.Threading.Tasks;
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

        public OptimizedResourceTagHelper(IOptimizationManager optimizationManager, IOptimizationContext optimizationContext, IOptimizationResultTagBuilderFactory tagBuilderFactory)
        {
            this.OptimizationContext = optimizationContext;
            this.OptimizationManager = optimizationManager;
            this.TagBuilderFactory = tagBuilderFactory;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var optimizationResult = await this.OptimizationManager.OptimizeAsync(this.OptimizationContext, this.Extension);

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

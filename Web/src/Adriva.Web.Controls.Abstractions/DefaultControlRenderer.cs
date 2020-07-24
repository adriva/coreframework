using System;
using System.Threading.Tasks;
using Adriva.Extensions.Optimization.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using Adriva.Extensions.Optimization.Web;
using System.IO;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;

namespace Adriva.Web.Controls.Abstractions
{
    public partial class DefaultControlRenderer : IControlRenderer
    {
        private class TagBuilderOptions : ITagBuilderOptions
        {
            public AssetFileExtension Extension { get; private set; }

            public OptimizationTagOutput Output { get; private set; }

            public TagBuilderOptions(AssetFileExtension extension, OptimizationTagOutput output)
            {
                this.Extension = extension;
                this.Output = output;
            }
        }

        private readonly WebControlsRendererOptions RendererOptions;
        private readonly WebControlsOptions Options;

        private string ContainerNameField;
        private AssetDeliveryMethod AssetDeliveryMethodField;
        private RendererTagHelper RendererControl;

        private AssetDeliveryMethod AssetDeliveryMethod
        {
            get
            {
                if (AssetDeliveryMethod.Unspecified == this.AssetDeliveryMethodField)
                {
                    if (AssetDeliveryMethod.Unspecified == this.RendererControl.AssetDeliveryMethod)
                    {
                        this.AssetDeliveryMethodField = this.Options.AssetDeliveryMethod;
                    }

                    this.AssetDeliveryMethodField = this.RendererControl.AssetDeliveryMethod;

                    if (AssetDeliveryMethod.Unspecified == this.AssetDeliveryMethodField)
                    {
                        this.AssetDeliveryMethodField = AssetDeliveryMethod.InlineTag;
                    }
                }

                return this.AssetDeliveryMethodField;
            }
        }

        private string ContainerName
        {
            get
            {
                if (null == this.ContainerNameField)
                {
                    if (null == this.RendererControl.ContainerName)
                    {
                        this.ContainerNameField = this.Options.ContainerName ?? string.Empty;
                    }
                    else
                    {
                        this.ContainerNameField = this.RendererControl.ContainerName ?? string.Empty;
                    }
                }

                return this.ContainerNameField;
            }
        }

        public DefaultControlRenderer(IOptions<WebControlsRendererOptions> rendererOptionsAccessor, IOptions<WebControlsOptions> optionsAccessor)
        {
            this.Options = optionsAccessor.Value;
            this.RendererOptions = rendererOptionsAccessor.Value;
        }

        protected virtual void RenderRootControl(IControlOutputContext context)
        {

        }

        private async Task RenderAssetsAsync(IControlOutputContext context, RendererTagAttributes attributes)
        {
            var assetPaths = this.ResolveAssetPaths(context);
            var view = context.Control.ViewContext.View as RazorView;
            RazorPage razorPage = view.RazorPage as RazorPage;

            async Task RenderAssetTagsInPageAsync()
            {
                var httpContext = context.GetHttpContext();
                var tagBuilderFactory = httpContext.RequestServices.GetService<IOptimizationResultTagBuilderFactory>();

                HtmlContentBuilder htmlContentBuilder = new HtmlContentBuilder();

                foreach (var assetPath in assetPaths)
                {
                    var extension = Path.GetExtension(assetPath);
                    var optimizationResultTagBuilder = tagBuilderFactory.GetBuilder(extension);
                    Asset asset = new Asset(assetPath);
                    await this.RenderAssetAsync(optimizationResultTagBuilder, attributes, asset, extension, htmlContentBuilder);
                }

                htmlContentBuilder.WriteTo(razorPage);
            };

            switch (this.AssetDeliveryMethod)
            {
                case AssetDeliveryMethod.InlineTag:
                    await RenderAssetTagsInPageAsync();
                    break;
                case AssetDeliveryMethod.OptimizationContext:
                    var currentHttpContext = context.GetHttpContext();
                    var optimizationScope = currentHttpContext.RequestServices.GetRequiredService<IOptimizationScope>();
                    var optimizationContext = optimizationScope.AddOrGetContext(this.ContainerName);
                    foreach (var assetPath in assetPaths)
                    {
                        optimizationContext.AddAsset(assetPath);
                    }
                    break;
                case AssetDeliveryMethod.SectionWriterTag:
                    if (string.IsNullOrWhiteSpace(this.ContainerName))
                    {
                        throw new ArgumentNullException(nameof(this.ContainerName), "ContainerName should have a valid section name.");
                    }

                    razorPage.SectionWriters[this.ContainerName] = async () =>
                    {
                        await RenderAssetTagsInPageAsync();
                    };
                    break;
            }
        }

        protected virtual async Task RenderAssetAsync(IOptimizationResultTagBuilder optimizationResultTagBuilder, RendererTagAttributes attributes, Asset asset, string extension, IHtmlContentBuilder htmlContentBuilder)
        {
            TagBuilderOptions options = new TagBuilderOptions(extension, OptimizationTagOutput.Tag);
            await optimizationResultTagBuilder.PopulateHtmlTagAsync(options, attributes, asset, htmlContentBuilder);
            await Task.CompletedTask;
        }

        protected virtual IEnumerable<string> ResolveAssetPaths(IControlOutputContext context)
        {
            IAssetProvider assetProvider = (context.Control as IAssetProvider) ?? (context.Children.FirstOrDefault()?.Control as IAssetProvider);

            if (null == assetProvider) yield break;

            var extensions = assetProvider.GetAssetFileExtensions();

            if (null == extensions) yield break;

            extensions = extensions.Distinct();

            foreach (var extension in extensions)
            {
                var paths = assetProvider.GetAssetPaths(extension);

                if (null == paths) yield break;

                foreach (var path in paths)
                {
                    yield return path;
                }
            }
        }

        public virtual void Render(IControlOutputContext context, RendererTagAttributes attributes)
        {
            this.RenderAsync(context, attributes).GetAwaiter().GetResult();
        }

        public virtual async Task RenderAsync(IControlOutputContext context, RendererTagAttributes attributes)
        {
            this.RendererControl = context.Control as RendererTagHelper;
            if (null == context.Parent)
            {
                if (1 != context.Children.Count) throw new Exception();
                context.Output.TagName = string.Empty;
                this.RenderRootControl(context.Children[0]);
                context.Children[0].Output.Content.MoveTo(context.Output.Content);
                await this.RenderAssetsAsync(context, attributes);
            }
        }
    }
}
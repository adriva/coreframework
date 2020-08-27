using System;
using System.Threading.Tasks;
using Adriva.Extensions.Optimization.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using Adriva.Extensions.Optimization.Web;
using Microsoft.AspNetCore.Html;

namespace Adriva.Web.Controls.Abstractions
{
    public abstract class DefaultControlRenderer : IControlRenderer
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
        private readonly IServiceProvider ServiceProvider;

        private RendererTagHelper RendererControl;

        private string OptimizationContextName
        {
            get
            {
                string optimizationContextName = null;

                if (string.IsNullOrWhiteSpace(this.RendererControl.OptimizationContextName))
                {
                    optimizationContextName = this.Options.OptimizationContextName;
                }

                if (null == optimizationContextName) optimizationContextName = Microsoft.Extensions.Options.Options.DefaultName;

                return optimizationContextName;
            }
        }

        public DefaultControlRenderer(IServiceProvider serviceProvider, IOptions<WebControlsRendererOptions> rendererOptionsAccessor, IOptions<WebControlsOptions> optionsAccessor)
        {
            this.ServiceProvider = serviceProvider;
            this.Options = optionsAccessor.Value;
            this.RendererOptions = rendererOptionsAccessor.Value;
        }

        protected virtual void RenderRootControl(IControlOutputContext context)
        {

        }

        private void RenderAssets(IControlOutputContext context, RendererTagAttributes attributes)
        {
            var assetPaths = this.ResolveAssetPaths(context);

            var currentHttpContext = context.GetHttpContext();
            var optimizationScope = currentHttpContext.RequestServices.GetRequiredService<IOptimizationScope>();
            var optimizationContext = optimizationScope.AddOrGetContext(this.OptimizationContextName);
            foreach (var assetPath in assetPaths)
            {
                optimizationContext.AddAsset(assetPath);
            }
        }

        protected virtual async Task RenderAssetAsync(IOptimizationResultTagBuilder optimizationResultTagBuilder, RendererTagAttributes attributes, Asset asset, string extension, IHtmlContentBuilder htmlContentBuilder)
        {
            TagBuilderOptions options = new TagBuilderOptions(extension, OptimizationTagOutput.Tag);
            await optimizationResultTagBuilder.PopulateHtmlTagAsync(options, attributes, asset, htmlContentBuilder);
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
            this.RendererControl = context.Control as RendererTagHelper;
            if (null == context.Parent)
            {
                if (1 != context.Children.Count) throw new Exception();
                context.Output.TagName = string.Empty;
                this.RenderRootControl(context.Children[0]);
                context.Children[0].Output.Content.MoveTo(context.Output.Content);
                this.RenderAssets(context, attributes);
            }
        }

        public virtual async Task RenderAsync(IControlOutputContext context, RendererTagAttributes attributes)
        {
            await Task.CompletedTask;
            this.Render(context, attributes);
        }
    }
}
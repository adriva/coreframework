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
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Adriva.Web.Controls.Abstractions
{
    public class DefaultControlRenderer : IControlRenderer
    {
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
                    if (string.IsNullOrWhiteSpace(this.RendererControl.ContainerName))
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

        private void RenderAssets(IControlOutputContext context)
        {
            var assetPaths = this.ResolveAssetPaths(context.Children.FirstOrDefault().Control);

            switch (this.Options.AssetDeliveryMethod)
            {
                case AssetDeliveryMethod.InlineTag:
                    break;
                case AssetDeliveryMethod.OptimizationContext:
                    break;
                case AssetDeliveryMethod.SectionWriterTag:
                    if (string.IsNullOrWhiteSpace(this.ContainerName))
                    {
                        throw new ArgumentNullException(nameof(this.ContainerName), "ContainerName should have a valid section name.");
                    }

                    var view = context.Control.ViewContext.View as RazorView;
                    RazorPage razorPage = view.RazorPage as RazorPage;

                    razorPage.SectionWriters[this.ContainerName] = async () =>
                    {
                        var httpContext = context.GetHttpContext();
                        var tagBuilderFactory = httpContext.RequestServices.GetService<IOptimizationResultTagBuilderFactory>();

                        HtmlContentBuilder htmlContentBuilder = new HtmlContentBuilder();

                        foreach (var assetPath in assetPaths)
                        {
                            var extension = Path.GetExtension(assetPath);
                            var optimizationResultTagBuilder = tagBuilderFactory.GetBuilder(extension);
                            Asset asset = new Asset(assetPath);
                            await this.RenderAssetAsync(optimizationResultTagBuilder, asset, htmlContentBuilder);
                        }

                        htmlContentBuilder.WriteTo(razorPage);
                    };
                    break;
            }
            var httpContext = context.GetHttpContext();
            var optimizationScope = httpContext.RequestServices.GetService<IOptimizationScope>();
            var optimizationContext = optimizationScope.AddOrGetContext(this.ContainerName);
        }

        protected virtual async Task RenderAssetAsync(IOptimizationResultTagBuilder optimizationResultTagBuilder, Asset asset, IHtmlContentBuilder htmlContentBuilder)
        {
            htmlContentBuilder.AppendHtmlLine("<optimizedresource />");
            await Task.CompletedTask;
        }

        protected virtual IEnumerable<string> ResolveAssetPaths(ControlTagHelper controlTagHelper)
        {
            if (!(controlTagHelper is IAssetProvider assetProvider)) yield break;

            var extensions = assetProvider.GetAssetFileExtensions();

            if (null == extensions) yield break;

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

        public virtual void Render(IControlOutputContext context, IDictionary<string, object> attributes)
        {
            this.RendererControl = context.Control as RendererTagHelper;
            if (null == context.Parent)
            {
                this.RenderRootControl(context);
                this.RenderAssets(context);
            }
        }

        public virtual Task RenderAsync(IControlOutputContext context, IDictionary<string, object> attributes)
        {
            this.RendererControl = context.Control as RendererTagHelper;
            if (null == context.Parent)
            {
                this.RenderRootControl(context);
                this.RenderAssets(context);
            }

            return Task.CompletedTask;
        }
    }

    internal static class Extensions
    {
        public static void WriteTo(this IHtmlContent htmlContent, RazorPageBase razorPageBase)
        {
            using (StringWriter writer = new StringWriter())
            {
                htmlContent.WriteTo(writer, HtmlEncoder.Default);
                var buffer = writer.GetStringBuilder();
                razorPageBase.WriteLiteral(buffer);
            }
        }
    }
}
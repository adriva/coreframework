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
    public abstract class DefaultControlRenderer<TControl> : IControlRenderer where TControl : ControlTagHelper
    {
        private readonly WebControlsRendererOptions RendererOptions;
        private readonly WebControlsOptions Options;
        private readonly IServiceProvider ServiceProvider;

        private RendererTagHelper RendererControl;

        protected string OptimizationContextName
        {
            get
            {
                string optimizationContextName = null;

                if (!string.IsNullOrWhiteSpace(this.RendererControl.OptimizationContextName))
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

        protected virtual void RenderRootControl(TControl control, IControlOutputContext context)
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
                    if (null == path) throw new ArgumentNullException($"Null resource path specified in web control '{context.Control?.GetType().FullName}'.");

                    if (Uri.TryCreate(path, UriKind.Absolute, out Uri tempUri)) yield return path;
                    else
                    {
                        yield return $"{this.Options.AssetsRootPath.Value.TrimEnd('/')}/{path.TrimStart('/')}";
                    }
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
                this.RenderRootControl((TControl)context.Children[0].Control, context.Children[0]);
                context.Children[0].Output.PreContent.MoveTo(context.Output.PreContent);
                context.Children[0].Output.Content.MoveTo(context.Output.Content);
                context.Children[0].Output.PostContent.MoveTo(context.Output.PostContent);

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
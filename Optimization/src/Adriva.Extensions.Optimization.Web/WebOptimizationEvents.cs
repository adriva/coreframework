using System;
using System.IO;
using Adriva.Extensions.Optimization.Abstractions;
using Adriva.Extensions.Optimization.Transforms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Optimization.Web
{
    internal sealed class WebOptimizationEvents : IOptimizationEvents<WebOptimizationOptions>
    {
        public Action<IServiceProvider, WebOptimizationOptions> ServiceInitialized
        {
            get => (serviceProvider, options) =>
            {
                IHostingEnvironment hostingEnvironment = serviceProvider.GetService<IHostingEnvironment>();
                if (!string.IsNullOrWhiteSpace(options.StaticAssetsPath))
                {
                    var physicalAssetsPath = hostingEnvironment.WebRootFileProvider.GetFileInfo(options.StaticAssetsPath).PhysicalPath;
                    Directory.Delete(physicalAssetsPath, true);
                    Directory.CreateDirectory(physicalAssetsPath);
                }
            };
        }

        public Func<string, WebOptimizationOptions, ITransform, bool> TransformRunning
        {
            get => (extension, options, transform) =>
            {
                if (transform is StylesheetBundleTransform) return options.BundleStylesheets;
                if (transform is StylesheetMinificationTransform) return options.MinifyStylesheets;
                if (transform is JavascriptBundleTransform) return options.BundleJavascripts;
                if (transform is JavascriptMinificationTransform) return options.MinifyJavascripts;
                return true;
            };
        }

    }
}

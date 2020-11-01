using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Adriva.Common.Core;
using Adriva.Extensions.Optimization.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Text;
using Microsoft.Extensions.FileProviders;
using System.Linq;
using System.Threading.Tasks;
using Adriva.Web.Common;

namespace Adriva.Extensions.Optimization.Web
{
    internal class WebOptimizationContext : IOptimizationContext
    {
        private readonly IWebHostEnvironment HostingEnvironment;

        internal HttpContext HttpContext { get; private set; }

        public string Identifier { get; private set; }

        public string Name { get; private set; }

        private readonly List<Asset> AssetList = new List<Asset>();

        public ReadOnlyCollection<Asset> Assets => new ReadOnlyCollection<Asset>(this.AssetList);

        public WebOptimizationContext(IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment hostingEnvironment)
        {
            this.HttpContext = httpContextAccessor.HttpContext;
            this.HostingEnvironment = hostingEnvironment;
        }

        private Uri ResolveAssetUri(string pathOrUrl)
        {
            if (string.IsNullOrWhiteSpace(pathOrUrl)) throw new ArgumentNullException(nameof(pathOrUrl));

            if (Uri.TryCreate(pathOrUrl, UriKind.Absolute, out Uri uri) && Utilities.IsValidHttpUri(uri))
            {
                return uri;
            }

            int loop = -1;
            StringBuilder buffer = new StringBuilder();

            while (++loop < pathOrUrl.Length)
            {
                char current = pathOrUrl[loop];

                if (0 == loop && '~' == current) continue;
                else if (2 > loop && '/' == current) continue;
                else
                {
                    buffer.Append(current);
                }
            }

            IFileInfo fileInfo = this.HostingEnvironment.WebRootFileProvider.GetFileInfo(buffer.ToString());
            if (fileInfo.Exists)
            {
                return new Uri(fileInfo.PhysicalPath);
            }
            if (Uri.TryCreate(buffer.ToString(), UriKind.RelativeOrAbsolute, out Uri assetUri))
            {
                if (assetUri.IsAbsoluteUri) return assetUri;
                else
                {
                    buffer.Clear();
                    var request = this.HttpContext.Request;
                    return request.GetApplicationUri(UriKind.Absolute, pathOrUrl);
                }
            }
            return null;
        }

        public void AddAsset(string pathOrUrl)
        {
            Asset asset = new Asset(this.ResolveAssetUri(pathOrUrl));
            if (this.AssetList.Any(a => a.Location == asset.Location)) return;
            this.AssetList.Add(asset);

            this.Identifier = Utilities.CalculateHash(string.Join("|", this.AssetList.OrderBy(a => a.Name).Select(a => a.Name)));
        }

        public void SetName(string name)
        {
            if (null == this.Name) this.Name = name;
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var asset in this.AssetList)
            {
                await asset.DisposeAsync();
            }
        }
    }
}

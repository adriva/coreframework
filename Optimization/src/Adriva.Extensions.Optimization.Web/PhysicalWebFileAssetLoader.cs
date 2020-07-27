using System.IO;
using System.Threading.Tasks;
using Adriva.Extensions.Optimization.Abstractions;
using Microsoft.AspNetCore.Hosting;

namespace Adriva.Extensions.Optimization.Web
{
    public class PhysicalWebFileAssetLoader : PhysicalFileAssetLoader
    {
        private readonly IWebHostEnvironment HostingEnvironment;

        public PhysicalWebFileAssetLoader(IWebHostEnvironment hostingEnvironment)
        {
            this.HostingEnvironment = hostingEnvironment;
        }

        public override bool CanLoad(Asset asset)
        {
            if (asset.Location.IsAbsoluteUri) return false;
            var fileInfo = this.HostingEnvironment.WebRootFileProvider.GetFileInfo(asset.Location.OriginalString);
            return fileInfo.Exists;
        }

        public override ValueTask<Stream> OpenReadStreamAsync(Asset asset)
        {
            var assetFileInfo = this.HostingEnvironment.WebRootFileProvider.GetFileInfo(asset.Location.OriginalString);

            return new ValueTask<Stream>(assetFileInfo.CreateReadStream());
        }
    }
}

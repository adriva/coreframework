using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Adriva.Common.Core;

namespace Adriva.Extensions.Optimization.Abstractions
{
    /// <summary>
    /// Represents an asset resource that can be processed by the optimization transforms.
    /// </summary>
    public sealed class Asset : IDisposable
    {
        /// <summary>
        /// Gets the name of the asset resource.
        /// </summary>
        /// <value>A string representing the name of the asset.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the location of the asset resource.
        /// </summary>
        /// <value>A Uri that represents the locations a local or remote asset.</value>
        public Uri Location { get; private set; }

        /// <summary>
        /// Gets the content stream of the asset.
        /// </summary>
        /// <value>A Stream that returns the content of the asset resource.</value>
        public Stream Content { get; private set; }

        /// <summary>
        /// Initiates a new instance of an asset resource that has a specific location.
        /// </summary>
        /// <param name="uri">The location of the asset resource.</param>
        public Asset(Uri uri)
        {
            this.Location = uri ?? throw new ArgumentNullException(nameof(uri));
            this.Name = Path.GetFileName(this.Location.ToString());
        }

        /// <summary>
        /// Initiates a new instance of an asset resource that has a specific location.
        /// </summary>
        /// <param name="pathOrUrl">The location of the asset resource as a Uri string.</param>
        public Asset(string pathOrUrl)
        {
            if (!Uri.TryCreate(pathOrUrl, UriKind.Absolute, out Uri tempUri))
            {
                throw new InvalidCastException($"Specified path '{pathOrUrl}' is not a valid Uri.");
            }

            this.Location = tempUri;
            this.Name = Path.GetFileName(this.Location.ToString());
        }

        /// <summary>
        /// Initiates a new instance of an asset resource that has a specific name and content.
        /// </summary>
        /// <param name="name">The name of the asset resource.</param>
        /// <param name="content">The content of the asset resource.</param>
        public Asset(string name, Stream content)
        {
            if (!Uri.TryCreate(name, UriKind.Absolute, out Uri tempUri))
            {
                tempUri = new Uri($"asset://dynamic/{name}");
            }

            if (null != this.Content) throw new InvalidOperationException("Asset content is already set.");
            this.Content = content;
            this.Name = name;
            this.Location = tempUri;
        }

        internal async Task SetContentAsync(string content)
        {
            content = null == content ? string.Empty : content;
            this.Content = new AutoStream(64 * 1024);
            using (StreamWriter writer = new StreamWriter(this.Content, Encoding.UTF8, 4096, true))
            {
                await writer.WriteAsync(content);
            }
            this.Content.Seek(0, SeekOrigin.Begin);
        }

        internal async Task SetContentAsync(Stream stream)
        {
            if (null != this.Content) throw new InvalidOperationException("Asset content is already set.");

            this.Content = new AutoStream(64 * 1024); //64K ?
            await stream.CopyToAsync(this.Content);
            this.Content.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// Releases all resources used by the asset.
        /// </summary>
        public void Dispose()
        {
            this.Content?.Dispose();
        }

        /// <summary>
        /// Returns a string that represents the current asset.
        /// </summary>
        /// <returns>A string value representing the current asset.</returns>
        public override string ToString()
        {
            return $"Asset ({this.Name})";
        }
    }
}
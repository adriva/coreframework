using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Adriva.Common.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace Adriva.Web.Controls.Abstractions
{
    internal class ResourceLoader
    {
        private readonly WebControlsOptions Options;
        private readonly CompositeFileProvider FileProvider;

        public ResourceLoader(WebControlsOptions options)
        {
            this.Options = options;

            List<IFileProvider> embeddedFileProviders = new List<IFileProvider>();

            foreach (var controlLibrary in this.Options.ControlLibraries)
            {
                embeddedFileProviders.Add(new EmbeddedFileProvider(controlLibrary));
            }

            this.FileProvider = new CompositeFileProvider(embeddedFileProviders);
        }

        public Stream Load(PathString resourcePath, out string etag)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
                throw new ArgumentNullException(nameof(resourcePath));

            string normalizedPath = resourcePath.Value.Replace("/", ".").Substring(1);

            if (string.IsNullOrWhiteSpace(normalizedPath))
                throw new ArgumentNullException(nameof(resourcePath));

            var fileInfo = this.FileProvider.GetDirectoryContents(string.Empty).FirstOrDefault(x => 0 == string.Compare(x.Name, normalizedPath, StringComparison.OrdinalIgnoreCase));

            if (null == fileInfo || !fileInfo.Exists)
                throw new FileNotFoundException($"Web control asset not found at '{resourcePath}'.");

            etag = Utilities.GetBaseString((ulong)fileInfo.LastModified.UtcTicks, Utilities.Base36Alphabet);

            return fileInfo.CreateReadStream();
        }
    }
}
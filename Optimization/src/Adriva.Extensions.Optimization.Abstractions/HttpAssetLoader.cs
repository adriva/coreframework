using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Extensions.Caching.Abstractions;
using Adriva.Extensions.Caching.Memory;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public sealed class HttpAssetLoader : IAssetLoader
    {
        private readonly ICache Cache;
        private readonly HttpClient HttpClient;

        public HttpAssetLoader(IHttpClientFactory httpClientFactory, ICache<InMemoryCache> cacheWrapper)
        {
            this.Cache = cacheWrapper.Instance;
            this.HttpClient = httpClientFactory.CreateClient();
        }

        public bool CanLoad(Asset asset)
        {
            return Utilities.IsValidHttpUri(asset.Location);
        }

        public async ValueTask<Stream> OpenReadStreamAsync(Asset asset)
        {
            string locationHash = Utilities.CalculateBinaryHash(asset.Location.ToString());
            string extension = Path.GetExtension(asset.Location.ToString());
            string filePath = Path.Combine(Path.GetTempPath(), $"{locationHash}{extension}");

            // just to make sure after some time the temp file is deleted
            _ = this.Cache.GetOrCreateAsync(locationHash, async (entry) =>
            {
                await Task.CompletedTask;
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12);
                return new object();
            }, (key, value, cache) =>
            {
                try { File.Delete(filePath); }
                catch { }
            }, null);


            FileStream fileStream = null;

            if (File.Exists(filePath))
            {
                fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return fileStream;
            }
            else
            {
                fileStream = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            }

            using (var response = await this.HttpClient.GetAsync(asset.Location, HttpCompletionOption.ResponseHeadersRead))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to load asset from '{asset.Location}'. Response code is '{response.StatusCode}'.");
                }
                await response.Content.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
                fileStream.Seek(0, SeekOrigin.Begin);
                return fileStream;
            }
        }
    }

}
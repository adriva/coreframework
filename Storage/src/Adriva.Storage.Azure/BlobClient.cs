using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Storage.Abstractions;
using Azure.Storage.Blobs;
using MsAzure = Azure.Storage.Blobs;
using MsAzureModels = Azure.Storage.Blobs.Models;

namespace Adriva.Storage.Azure
{
    public class BlobClient : IBlobClient, IAsyncInitializedStorageClient<AzureBlobClientOptions>
    {
        protected AzureBlobClientOptions Options { get; private set; }

        protected BlobContainerClient BlobContainer { get; private set; }

        protected MsAzure.BlobClient GetClient(string name) => this.BlobContainer.GetBlobClient(name);

        public async ValueTask InitializeAsync(AzureBlobClientOptions options)
        {
            this.Options = options;
            var blobServiceClient = new BlobServiceClient(this.Options.ConnectionString);
            this.BlobContainer = blobServiceClient.GetBlobContainerClient(this.Options.ContainerName);
            await this.BlobContainer.CreateIfNotExistsAsync(MsAzureModels.PublicAccessType.None);
        }

        public async ValueTask DeleteAsync(string name)
        {
            await this.GetClient(name).DeleteIfExistsAsync(MsAzureModels.DeleteSnapshotsOption.IncludeSnapshots);
        }

        public async ValueTask<bool> ExistsAsync(string name)
        {
            return await this.GetClient(name).ExistsAsync();
        }

        public async Task<BlobItemProperties> GetPropertiesAsync(string name)
        {
            var client = this.GetClient(name);
            var properties = await client.GetPropertiesAsync();
            return new BlobItemProperties(properties.Value.ContentLength, properties.Value.ETag.ToString(), properties.Value.LastModified);
        }

        public async Task<SegmentedResult<string>> ListAllAsync(string continuationToken, string prefix = null, int count = 500)
        {
            count = Math.Max(1, Math.Min(5000, count));

            var pagedData = this.BlobContainer
                                .GetBlobsByHierarchyAsync(MsAzureModels.BlobTraits.None, MsAzureModels.BlobStates.None, "/", prefix)
                                .AsPages(continuationToken, count);

            List<string> blobItems = new List<string>();

            await foreach (var page in pagedData)
            {
                continuationToken = page.ContinuationToken;

                foreach (var item in page.Values)
                {
                    if (item.IsBlob)
                    {
                        blobItems.Add(item.Blob.Name);
                    }
                }
            }

            return new SegmentedResult<string>(blobItems, continuationToken, !string.IsNullOrWhiteSpace(continuationToken));
        }

        public Task<Stream> OpenReadStreamAsync(string name)
        {
            return this.GetClient(name).OpenReadAsync();
        }

        protected virtual async Task<Uri> ProcessUploadAsync(string name, Func<MsAzure.BlobClient, Task> uploadCallback, int cacheDuration)
        {
            var client = this.GetClient(name);

            await uploadCallback(client);

            if (0 < cacheDuration)
            {
                MsAzureModels.BlobHttpHeaders blobHttpHeaders = new MsAzureModels.BlobHttpHeaders()
                {
                    CacheControl = $"max-age={cacheDuration}, must-revalidate"
                };

                await client.SetHttpHeadersAsync(blobHttpHeaders);
            }

            return client.Uri;
        }

        public Task<Uri> UpsertAsync(string name, Stream stream, bool shouldOverwrite = false, int cacheDuration = 0)
        {
            return this.ProcessUploadAsync(name, x => x.UploadAsync(stream, shouldOverwrite), cacheDuration);
        }

        public Task<Uri> UpsertAsync(string name, ReadOnlyMemory<byte> data, bool shouldOverwrite = false, int cacheDuration = 0)
        {
            return this.ProcessUploadAsync(name, x => x.UploadAsync(BinaryData.FromBytes(data), shouldOverwrite), cacheDuration);
        }
    }
}

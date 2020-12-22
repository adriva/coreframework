using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Storage.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Adriva.Storage.SqlServer
{
    internal class SqlServerBlobClient : IBlobClient
    {
        private static readonly SemaphoreSlim DatabaseCreateSemaphore = new SemaphoreSlim(1, 1);

        private static bool IsDatabaseObjectsCreated = false;

        private readonly BlobDbContext DbContext;
        private string ContainerName;

        // name format = CONTAINER/FOLDER/FOLDER/FOLDER/NAME
        public static bool TryParseName(string name, out string containerName, out string blobName)
        {
            containerName = blobName = null;

            if (string.IsNullOrWhiteSpace(name)) return false;
            if (!char.IsLetter(name[0])) return false;
            name = name.Replace("\\", "/");
            containerName = name;

            do
            {
                string temp = Path.GetDirectoryName(containerName);
                if (string.IsNullOrWhiteSpace(temp)) break;
                containerName = temp;
            } while (!string.IsNullOrWhiteSpace(containerName));

            blobName = Path.GetRelativePath(containerName, name);

            if (string.IsNullOrWhiteSpace(containerName) || string.IsNullOrWhiteSpace(blobName) || 0 == string.Compare(name, containerName, StringComparison.OrdinalIgnoreCase))
            {
                containerName = blobName = null;
                return false;
            }

            return true;
        }

        public SqlServerBlobClient(BlobDbContext blobDbContext)
        {
            this.DbContext = blobDbContext;
        }

        private async ValueTask EnsureDatabaseObjectsAsync()
        {
            if (!SqlServerBlobClient.IsDatabaseObjectsCreated)
            {
                await SqlServerBlobClient.DatabaseCreateSemaphore.WaitAsync();

                try
                {
                    if (!SqlServerBlobClient.IsDatabaseObjectsCreated)
                    {
                        await DbHelpers.ExecuteScriptAsync(this.DbContext.Database, "blob-createtable.sql");
                        SqlServerBlobClient.IsDatabaseObjectsCreated = true;
                    }
                }
                finally
                {
                    SqlServerBlobClient.DatabaseCreateSemaphore.Release();
                }
            }
        }

        private async ValueTask<long?> GetBlobIdAsync(string name)
        {
            return await this.DbContext.Blobs.Where(b => b.ContainerName == this.ContainerName && b.Name == name).Select(b => b.Id).FirstOrDefaultAsync();
        }

        public ValueTask InitializeAsync(StorageClientContext context)
        {
            this.ContainerName = context.Name;
            return new ValueTask();
        }

        public async ValueTask<bool> ExistsAsync(string name)
        {
            return null != (await this.GetBlobIdAsync(name));
        }

        public Task<BlobItemProperties> GetPropertiesAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<SegmentedResult<string>> ListAllAsync(string continuationToken, string prefix = null, int count = 500)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> OpenReadStreamAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<ReadOnlyMemory<byte>> ReadAllBytesAsync(string name)
        {
            var id = await this.GetBlobIdAsync(name);
            if (!id.HasValue) return new ReadOnlyMemory<byte>(Array.Empty<byte>());

            var entity = await this.DbContext.Blobs.FindAsync(id);
            if (null == entity) return new ReadOnlyMemory<byte>(Array.Empty<byte>());
            return entity.Content;
        }

        public Task<string> UpsertAsync(string name, Stream stream, int cacheDuration = 0)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpsertAsync(string name, ReadOnlyMemory<byte> data, int cacheDuration = 0)
        {
            throw new NotImplementedException();
        }

        public ValueTask DeleteAsync(string name)
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync() => new ValueTask();
    }
}
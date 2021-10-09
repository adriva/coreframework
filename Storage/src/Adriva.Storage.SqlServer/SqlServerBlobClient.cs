using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Storage.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Adriva.Storage.SqlServer
{
    internal class SqlServerBlobClient : IBlobClient
    {
        private static readonly SemaphoreSlim DatabaseCreateSemaphore = new SemaphoreSlim(1, 1);

        private readonly DbContextFactory DbContextFactory;

        private string ContainerName;
        private SqlServerBlobOptions Options;
        private BlobDbContext DbContext;

        private static string CalculateETag(Stream stream)
        {
            if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);

            byte[] hash = Utilities.CalculateStreamHash(stream);

            return Utilities.GetBaseString(hash, Utilities.Base36Alphabet).ToUpperInvariant();
        }

        private static void ValidateName(ref string name)
        {
            /*
            name => letter+ (letter,space,digit,seperator)* letter+
            */

            bool IsValidChar(char c)
            {
                return
                    (char.IsLetterOrDigit(c))
                    || ' ' == c
                    || '-' == c
                    || '_' == c
                    || '/' == c
                    ;
            }

            var exception = new ArgumentException("Invalid blob name.", nameof(name));

            name = name.Replace("\\", "/");
            if (string.IsNullOrWhiteSpace(name)) throw exception;
            if (!char.IsLetter(name[0])) throw exception;
            if ('/' == name[name.Length - 1]) throw exception;
            if (!IsValidChar(name[name.Length - 1])) throw exception;

            int loop = 1, count = name.Length;

            while (loop < count)
            {
                if (IsValidChar(name[loop]))
                {
                    ++loop;
                    continue;
                }
                else throw exception;
            }

        }

        public SqlServerBlobClient(DbContextFactory dbContextFactory)
        {
            this.DbContextFactory = dbContextFactory;
        }

        private async ValueTask EnsureDatabaseObjectsAsync()
        {
            await SqlServerBlobClient.DatabaseCreateSemaphore.WaitAsync();

            try
            {
                await DbHelpers.ExecuteScriptAsync(this.DbContext.Database, this.Options, "blob-createtable", "blob-createsp");
            }
            finally
            {
                SqlServerBlobClient.DatabaseCreateSemaphore.Release();
            }
        }

        public async ValueTask InitializeAsync(StorageClientContext context)
        {
            this.Options = context.GetOptions<SqlServerBlobOptions>();
            this.DbContext = this.DbContextFactory.GetBlobDbContext(context, this.Options);
            await this.EnsureDatabaseObjectsAsync();
            this.ContainerName = context.Name;
        }

        private async ValueTask<long?> GetBlobIdAsync(string name)
        {
            SqlServerBlobClient.ValidateName(ref name);
            long id = await this.DbContext.Blobs.Where(b => b.ContainerName == this.ContainerName && b.Name == name).Select(b => b.Id).FirstOrDefaultAsync();
            if (0 == id) return null;
            return id;
        }

        public async ValueTask<bool> ExistsAsync(string name)
        {
            SqlServerBlobClient.ValidateName(ref name);
            return (await this.GetBlobIdAsync(name)).HasValue;
        }

        public async Task<BlobItemProperties> GetPropertiesAsync(string name)
        {
            SqlServerBlobClient.ValidateName(ref name);
            var id = await this.GetBlobIdAsync(name);

            if (!id.HasValue) return BlobItemProperties.NotExists;

            var blobEntity = await this.DbContext.Blobs.FindAsync(id.Value);
            if (null == blobEntity) return BlobItemProperties.NotExists;

            return new BlobItemProperties(blobEntity.Length, blobEntity.ETag, blobEntity.LastModifiedUtc);
        }

        public async Task<SegmentedResult<string>> ListAllAsync(string continuationToken, string prefix = null, int count = 500)
        {
            if (!long.TryParse(continuationToken, out long startId)) startId = 0;

            if (null != prefix && !prefix.EndsWith('/')) prefix = prefix + '/';

            var items = await this.DbContext.Blobs
                .Where(b => b.ContainerName == this.ContainerName && (null == prefix || b.Name.StartsWith(prefix)) && b.Id >= startId)
                .Select(b => new { b.Id, b.Name })
                .OrderBy(b => b.Id)
                .Take(1 + count)
                .ToListAsync();

            if (count + 1 == items.Count)
            {
                continuationToken = items[count].Id.ToString();
            }
            else
            {
                continuationToken = null;
            }

            if (items.Any())
            {
                return new SegmentedResult<string>(items.Select(x => x.Name).Take(count), continuationToken, !string.IsNullOrWhiteSpace(continuationToken));
            }
            else
            {
                return SegmentedResult<string>.Empty;
            }
        }

        public async Task<Stream> OpenReadStreamAsync(string name)
        {
            SqlServerBlobClient.ValidateName(ref name);
            var id = await this.GetBlobIdAsync(name);

            if (!id.HasValue) return SqlServerStream.Empty;

            DbConnection connection = new SqlConnection(this.Options.ConnectionString);
            DbCommand command = null;
            DbDataReader reader = null;
            try
            {
                if (ConnectionState.Closed == connection.State)
                {
                    await connection.OpenAsync();
                }

                command = connection.CreateCommand();
                command.CommandText = $"SELECT [Content] FROM {this.Options.SchemaName}.{this.Options.TableName} WHERE Id = {id.Value}";

                reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync()) return SqlServerStream.Empty;
                else return new SqlServerStream(reader.GetStream(0), reader, command, connection);
            }
            catch
            {
                if (null != reader) await reader.DisposeAsync();
                if (null != command) await command.DisposeAsync();
                if (null != connection) await connection.DisposeAsync();
                throw;
            }
        }

        public async Task<ReadOnlyMemory<byte>> ReadAllBytesAsync(string name)
        {
            SqlServerBlobClient.ValidateName(ref name);
            var id = await this.GetBlobIdAsync(name);
            if (!id.HasValue) return new ReadOnlyMemory<byte>(Array.Empty<byte>());

            var entity = await this.DbContext.Blobs.FindAsync(id);
            if (null == entity) return new ReadOnlyMemory<byte>(Array.Empty<byte>());
            return entity.Content;
        }

        public async Task<string> UpsertAsync(string name, Stream stream, int cacheDuration = 0)
        {
            SqlServerBlobClient.ValidateName(ref name);

            long? existingId = await this.GetBlobIdAsync(name);

            using (var connection = new SqlConnection(this.Options.ConnectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = connection.CreateCommand())
                    {
                        string etag = SqlServerBlobClient.CalculateETag(stream);
                        stream.Seek(0, SeekOrigin.Begin);

                        command.Transaction = transaction;

                        command.CommandText = $"{this.Options.SchemaName}.{this.Options.UpsertProcedureName}";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddRange(new[] {
                            new SqlParameter("@id", SqlDbType.BigInt){ Value = existingId ?? 0 },
                            new SqlParameter("@containerName", SqlDbType.NVarChar, 1024){ Value = this.ContainerName },
                            new SqlParameter("@name", SqlDbType.NVarChar, 1024){ Value = name },
                            new SqlParameter("@data", SqlDbType.VarBinary, -1) {Value = stream },
                            new SqlParameter("@length", SqlDbType.BigInt) {Value = stream.Length },
                            new SqlParameter("@etag", SqlDbType.VarChar, 100) {Value = etag }
                        });

                        await command.ExecuteNonQueryAsync();
                    }

                    await transaction.CommitAsync();
                }
            }

            return name;
        }

        public async Task<string> UpsertAsync(string name, ReadOnlyMemory<byte> data, int cacheDuration = 0)
        {
            using (var stream = new MemoryStream(data.ToArray()))
            {
                return await this.UpsertAsync(name, stream, cacheDuration);
            }
        }

        public async Task UpdateAsync(string name, ReadOnlyMemory<byte> data, string etag)
        {
            using (var stream = new MemoryStream(data.ToArray()))
            {
                await this.UpdateAsync(name, stream, etag);
            }
        }

        public async Task UpdateAsync(string name, Stream stream, string etag)
        {
            SqlServerBlobClient.ValidateName(ref name);

            long? existingId = await this.GetBlobIdAsync(name);

            using (var connection = new SqlConnection(this.Options.ConnectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = connection.CreateCommand())
                    {
                        string newEtag = SqlServerBlobClient.CalculateETag(stream);
                        stream.Seek(0, SeekOrigin.Begin);

                        command.Transaction = transaction;

                        command.CommandText = $"{this.Options.SchemaName}.{this.Options.UpdateProcedureName}";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddRange(new[] {
                            new SqlParameter("@id", SqlDbType.BigInt){ Value = existingId ?? 0 },
                            new SqlParameter("@data", SqlDbType.VarBinary, -1) {Value = stream },
                            new SqlParameter("@length", SqlDbType.BigInt) {Value = stream.Length },
                            new SqlParameter("@etag", SqlDbType.VarChar, 100) {Value = newEtag },
                            new SqlParameter("@matchEtag", SqlDbType.VarChar, 100) {Value = etag }
                        });

                        await command.ExecuteNonQueryAsync();
                    }

                    await transaction.CommitAsync();
                }
            }
        }

        public async ValueTask DeleteAsync(string name)
        {
            SqlServerBlobClient.ValidateName(ref name);

            using (var transaction = await this.DbContext.Database.BeginTransactionAsync())
            {
                long? id = await this.GetBlobIdAsync(name);
                if (!id.HasValue) return;

                await this.DbContext.Database.ExecuteSqlRawAsync($"DELETE {this.Options.SchemaName}.{this.Options.TableName} WHERE Id = {id.Value}");

                await transaction.CommitAsync();
            }
        }
    }
}
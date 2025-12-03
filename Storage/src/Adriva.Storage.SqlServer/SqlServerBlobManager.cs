using System.Data;
using System.Linq.Expressions;
using Adriva.Common.Core;
using Adriva.Storage.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;

namespace Adriva.Storage.SqlServer;

public class SqlServerBlobManager : IBlobManager<SqlServerBlob>
{
    private static readonly SemaphoreSlim InitLock = new(1, 1);

    private readonly SqlServerBlobManagerConfiguration Configuration;
    private readonly SqlServerBlobStorage Storage;
    private readonly RecyclableMemoryStreamManager RecyclableMemoryStreamManager;
    private readonly ILogger<SqlServerBlobManager> Logger;

    private static bool IsInitialized;

    public SqlServerBlobManager(SqlServerBlobStorage storage, RecyclableMemoryStreamManager recyclableMemoryStreamManager, IOptions<SqlServerBlobManagerConfiguration> configurationAccessor, ILogger<SqlServerBlobManager> logger)
    {
        this.Storage = storage;
        this.Configuration = configurationAccessor.Value;
        this.RecyclableMemoryStreamManager = recyclableMemoryStreamManager;
        this.Logger = logger;

        if (this.Configuration.CreateDatabaseObjectsIfNeeded)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(this.Configuration.ConnectionString);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(this.Configuration.SchemaName);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(this.Configuration.TableName);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(this.Configuration.FileGroupName);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(this.Configuration.UpsertProcedureName);
        }
    }

    private async Task InitializeAsync()
    {
        if (!SqlServerBlobManager.IsInitialized)
        {
            await SqlServerBlobManager.InitLock.WaitAsync();

            if (!SqlServerBlobManager.IsInitialized)
            {
                try
                {
                    if (this.Configuration.CreateDatabaseObjectsIfNeeded)
                    {
                        this.Logger.LogInformation("Trying to create database objects if needed");

                        await Helpers.CreateTablesAsync(this.Configuration);
                        await Helpers.CreateStoredProceduresAsync(this.Configuration);
                    }

                    this.Logger.LogInformation("Database objects created and SqlServerBlobManager is initialized.");
                    SqlServerBlobManager.IsInitialized = true;
                }
                catch (Exception error)
                {
                    this.Logger.LogInformation(error, "Error initializing SqlServerBlobManager.");
                    throw;
                }
                finally
                {
                    SqlServerBlobManager.InitLock.Release();
                }
            }
        }
    }

    public async Task<SqlServerBlob> UpsertAsync(SqlServerBlob blob, Stream sourceStream)
    {
        ArgumentNullException.ThrowIfNull(sourceStream);

        if (!sourceStream.CanRead)
        {
            throw new IOException("Cannot read source stream.");
        }

        if (0 == sourceStream.Length)
        {
            throw new IOException("Source stream has a length of 0 bytes.");
        }

        await this.InitializeAsync();

        string sql = $"EXEC [{this.Configuration.SchemaName}].[{this.Configuration.UpsertProcedureName}] @id, @container, @name, @etag, @mimeType, @properties, @contentData, @insertedId OUT";

        var insertedIdParameter = new SqlParameter("@insertedId", SqlDbType.BigInt)
        {
            Direction = ParameterDirection.Output
        };

        if (this.Configuration.AutoResolveMimeType)
        {
            if (sourceStream.CanSeek)
            {
                long originalPosition = sourceStream.Position;

                if (MimeUtilities.All.TryResolveMimeType(sourceStream, out string[] mimeTypes))
                {
                    blob = blob with { MimeType = mimeTypes[0] };

                    sourceStream.Seek(originalPosition, SeekOrigin.Begin);
                }
            }
        }

        await using var transaction = await this.Storage.Database.BeginTransactionAsync();

        try
        {
            await this.Storage.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@id", blob.Id),
                new SqlParameter("@container", blob.Container),
                new SqlParameter("@name", blob.Name),
                new SqlParameter("@etag", blob.ETag),
                new SqlParameter("@mimeType", (object?)blob.MimeType ?? DBNull.Value),
                new SqlParameter("@properties", blob.Properties),
                new SqlParameter("@contentData", SqlDbType.Binary, -1)
                {
                    Value = sourceStream
                },
                insertedIdParameter
            );
        }
        catch (SqlException sqlError)
        {
            switch (sqlError.Number)
            {
                // https://learn.microsoft.com/en-us/sql/relational-databases/errors-events/database-engine-events-and-errors-2000-to-2999?view=sql-server-ver16
                case 2601:
                    throw new InvalidOperationException("There is another blob with the same name in the same container specified. Either upsert the existing item by specifying its id or change the name or container of this blob.");
                default:
                    throw;
            }
        }

        long newId = (long)insertedIdParameter.Value;

        await this.Storage.Blobs.Where(x => x.Id == newId).ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Size, sourceStream.Length));

        await transaction.CommitAsync();

        return blob with { Id = newId, Size = sourceStream.Length };
    }

    public async Task<SegmentedResult<SqlServerBlob>> ListAsync(Expression<Func<SqlServerBlob, bool>> predicate, string? continuationToken = null, int segmentSize = 100)
    {
        predicate ??= x => true;

        segmentSize = Math.Min(500, Math.Max(0, segmentSize));

        if (!long.TryParse(continuationToken, out long startingId))
        {
            startingId = long.MaxValue;
        }

        var items = await this.Storage.Blobs
                        .Where(predicate)
                        .Where(x => x.Id <= startingId)
                        .OrderByDescending(x => x.Id)
                        .Take(1 + segmentSize)
                        .Select(x => new SqlServerBlob(x.Id, x.Container, x.Name, x.ETag, x.MimeType, x.Size, x.Properties, x.ModifiedOn))
                        .ToArrayAsync();

        return new([.. items.Take(segmentSize)], segmentSize < items.Length ? Convert.ToString(items.Last().Id) : null, segmentSize < items.Length);
    }

    public Task<SqlServerBlob?> GetAsync(Expression<Func<SqlServerBlob, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return this.Storage.Blobs.Where(predicate)
            .Select(x => new SqlServerBlob(x.Id, x.Container, x.Name, x.ETag, x.MimeType, x.Size, x.Properties, x.ModifiedOn))
            .SingleOrDefaultAsync();
    }

    public async Task DeleteAsync(Expression<Func<SqlServerBlob, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        await this.Storage.Blobs.Where(predicate).ExecuteDeleteAsync();
    }

    public async ValueTask<bool> ExistsAsync(Expression<Func<SqlServerBlob, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return await this.Storage.Blobs.Where(predicate).AnyAsync();
    }

    public async Task<ReadOnlyMimeStream> OpenReadStreamAsync(Expression<Func<SqlServerBlob, bool>> predicate)
    {
        Stream output = Stream.Null;
        string? blobMimeType = null;

        await this.ProcessReadStreamAsync(predicate, async (sqlStream, mimeType) =>
        {
            if (Stream.Null == sqlStream)
            {
                return;
            }

            var memoryStream = this.RecyclableMemoryStreamManager.GetStream();
            await sqlStream.CopyToAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            output = memoryStream;
            blobMimeType = mimeType;
        });

        return new(output, blobMimeType);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="processSqlStream">An asynchronous callback function that takes the stream, mimeType as input.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task ProcessReadStreamAsync(Expression<Func<SqlServerBlob, bool>> predicate, Func<Stream, string?, Task> processSqlStream)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(processSqlStream);

        var matchingBlobs = await this.Storage.Blobs.Where(predicate).Select(x => new { x.Id, x.MimeType }).Take(2).ToListAsync();

        if (0 == matchingBlobs.Count)
        {
            await processSqlStream(Stream.Null, null);
        }
        else if (1 < matchingBlobs.Count)
        {
            throw new InvalidOperationException("Predicate matched more than one blob.");
        }

        await this.Storage.Database.OpenConnectionAsync();

        try
        {
            var connection = this.Storage.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            command.CommandText = $"SELECT ContentData FROM [{this.Configuration.SchemaName}].[{this.Configuration.TableName}] WHERE Id = @id";

            command.Parameters.Add(new SqlParameter("@id", matchingBlobs[0].Id));

            using var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

            if (await reader.ReadAsync())
            {
                using var sqlStream = reader.GetStream(0);
                await processSqlStream(sqlStream, matchingBlobs[0].MimeType);
            }
            else
            {
                await processSqlStream(Stream.Null, matchingBlobs[0].MimeType);
            }
        }
        finally
        {
            await this.Storage.Database.CloseConnectionAsync();
        }
    }
}

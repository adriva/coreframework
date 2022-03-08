using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Adriva.Extensions.Caching.SqlServer
{
    /// <summary>
    /// Distributed cache implementation using Microsoft SQL Server database.
    /// </summary>
    public class SqlServerCache : IDistributedCache, ICache
    {
        private static readonly TimeSpan MinimumExpiredItemsDeletionInterval = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan DefaultExpiredItemsDeletionInterval = TimeSpan.FromMinutes(30);

        private readonly IDatabaseOperations DbOperations;
        private readonly TimeSpan ExpiredItemsDeletionInterval;
        private DateTimeOffset LastExpirationScan;
        private readonly Action DeleteExpiredCachedItemsDelegate;
        private readonly TimeSpan DefaultSlidingExpiration;
        private readonly Object Mutex = new Object();

        /// <summary>
        /// Initializes a new instance of <see cref="SqlServerCache"/>.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        public SqlServerCache(IOptions<SqlServerCacheOptions> options)
        {
            var cacheOptions = options.Value;

            if (string.IsNullOrEmpty(cacheOptions.ConnectionString))
            {
                throw new ArgumentException($"{nameof(SqlServerCacheOptions.ConnectionString)} cannot be empty or null.");
            }
            if (string.IsNullOrEmpty(cacheOptions.SchemaName))
            {
                throw new ArgumentException($"{nameof(SqlServerCacheOptions.SchemaName)} cannot be empty or null.");
            }
            if (string.IsNullOrEmpty(cacheOptions.TableName))
            {
                throw new ArgumentException($"{nameof(SqlServerCacheOptions.TableName)} cannot be empty or null.");
            }
            if (cacheOptions.ExpiredItemsDeletionInterval.HasValue &&
                cacheOptions.ExpiredItemsDeletionInterval.Value < SqlServerCache.MinimumExpiredItemsDeletionInterval)
            {
                throw new ArgumentException($"{nameof(SqlServerCacheOptions.ExpiredItemsDeletionInterval)} cannot be less than the minimum " + $"value of {SqlServerCache.MinimumExpiredItemsDeletionInterval.TotalMinutes} minutes.");
            }
            if (cacheOptions.DefaultSlidingExpiration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(cacheOptions.DefaultSlidingExpiration), cacheOptions.DefaultSlidingExpiration, "The sliding expiration value must be positive.");
            }

            this.ExpiredItemsDeletionInterval = cacheOptions.ExpiredItemsDeletionInterval ?? SqlServerCache.DefaultExpiredItemsDeletionInterval;
            this.DeleteExpiredCachedItemsDelegate = this.DeleteExpiredCacheItems;
            this.DefaultSlidingExpiration = cacheOptions.DefaultSlidingExpiration;

            this.DbOperations = new DatabaseOperations(cacheOptions.ConnectionString, cacheOptions.SchemaName, cacheOptions.TableName, cacheOptions.DependencyTableName);
        }

        /// <inheritdoc />
        public byte[] Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var value = this.DbOperations.GetCacheItem(key);

            this.ScanForExpiredItemsIfRequired();

            return value;
        }

        /// <inheritdoc />
        public async Task<byte[]> GetAsync(string key, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            var value = await this.DbOperations.GetCacheItemAsync(key, token).ConfigureAwait(false);

            this.ScanForExpiredItemsIfRequired();

            return value;
        }

        /// <inheritdoc />
        public void Refresh(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            this.DbOperations.RefreshCacheItem(key);

            this.ScanForExpiredItemsIfRequired();
        }

        /// <inheritdoc />
        public async Task RefreshAsync(string key, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            await this.DbOperations.RefreshCacheItemAsync(key, token).ConfigureAwait(false);

            this.ScanForExpiredItemsIfRequired();
        }

        /// <inheritdoc />
        public void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            this.DbOperations.DeleteCacheItem(key);

            this.ScanForExpiredItemsIfRequired();
        }

        /// <inheritdoc />
        public async Task RemoveAsync(string key, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            await this.DbOperations.DeleteCacheItemAsync(key, token).ConfigureAwait(false);

            this.ScanForExpiredItemsIfRequired();
        }

        /// <inheritdoc />
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.GetOptions(ref options);

            this.DbOperations.SetCacheItem(key, value, options);

            this.ScanForExpiredItemsIfRequired();
        }

        /// <inheritdoc />
        public async Task SetAsync(
            string key,
            byte[] value,
            DistributedCacheEntryOptions options,
            CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            token.ThrowIfCancellationRequested();

            this.GetOptions(ref options);

            await this.DbOperations.SetCacheItemAsync(key, value, options, token).ConfigureAwait(false);

            this.ScanForExpiredItemsIfRequired();
        }

        // Called by multiple actions to see how long it's been since we last checked for expired items.
        // If sufficient time has elapsed then a scan is initiated on a background task.
        private void ScanForExpiredItemsIfRequired()
        {
            lock (this.Mutex)
            {
                var utcNow = DateTimeOffset.UtcNow;
                if ((utcNow - this.LastExpirationScan) > ExpiredItemsDeletionInterval)
                {
                    this.LastExpirationScan = utcNow;
                    Task.Run(DeleteExpiredCachedItemsDelegate);
                }
            }
        }

        private void DeleteExpiredCacheItems()
        {
            this.DbOperations.DeleteExpiredCacheItems();
        }

        private void GetOptions(ref DistributedCacheEntryOptions options)
        {
            if (!options.AbsoluteExpiration.HasValue
                && !options.AbsoluteExpirationRelativeToNow.HasValue
                && !options.SlidingExpiration.HasValue)
            {
                options = new DistributedCacheEntryOptions()
                {
                    SlidingExpiration = DefaultSlidingExpiration
                };
            }
        }

        /// <inheritdoc />
        public async Task<T> GetOrCreateAsync<T>(string key, Func<ICacheItem, Task<T>> factory, EvictionCallback evictionCallback = null, params string[] dependencyMonikers)
        {
            byte[] buffer = await this.GetAsync(key);

            if (null == buffer)
            {
                SqlCacheItem cacheItem = new SqlCacheItem();
                T data = await factory?.Invoke(cacheItem);

                if (null == data)
                {
                    return data;
                }

                string json = Utilities.SafeSerialize(data);

                buffer = Encoding.UTF8.GetBytes(json);

                await this.SetAsync(key, buffer, cacheItem);

                await this.DbOperations.AddOrUpdateDependencyMonikersAsync(key, dependencyMonikers);

                return data;
            }
            else
            {
                string json = Encoding.UTF8.GetString(buffer);
                return Utilities.SafeDeserialize<T>(json);
            }
        }

        /// <inheritdoc />
        public Task<T> GetOrCreateAsync<T>(string key, Func<ICacheItem, Task<T>> factory, params string[] dependencyMonikers)
        {
            return this.GetOrCreateAsync(key, factory, null, dependencyMonikers);
        }

        /// <inheritdoc />
        public async ValueTask RemoveAsync(string key)
        {
            await this.RemoveAsync(key, CancellationToken.None);
        }

        /// <inheritdoc />
        public void NotifyChanged(string key, string dependencyMoniker)
        {
            this.DbOperations.NotifyChanged(key, dependencyMoniker);
        }

        /// <inheritdoc />
        public Task NotifyChangedAsync(string key, string dependencyMoniker)
        {
            return this.DbOperations.NotifyChangedAsync(key, dependencyMoniker);
        }
    }
}
using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Adriva.Extensions.Faster
{
    public readonly struct StorageDataEntry
    {
        public static readonly StorageDataEntry Empty = new StorageDataEntry(null, null, null);

        public object Data { get; }

        public DateTime? ExpiresAt { get; }

        public string LockToken { get; }

        public string ETag { get; }

        internal StorageDataEntry(object data, string etag, DateTime? expiresAt)
        {
            this.Data = data;
            this.ExpiresAt = expiresAt;
            this.ETag = etag;
        }

        internal StorageDataEntry(object data, string etag, TimeSpan timeToLive)
        {
            this.Data = data;
            this.ExpiresAt = DateTime.UtcNow.Add(timeToLive);
            this.ETag = etag;
        }

        public StorageDataEntry(string lockToken, TimeSpan timeToLive)
        {
            this.Data = null;
            this.LockToken = lockToken;
            this.ExpiresAt = DateTime.UtcNow.Add(timeToLive);
            this.ETag = null;
        }

        public static bool operator ==(StorageDataEntry first, StorageDataEntry second)
        {
            return first.Data == second.Data
                &&
                (
                    null == first.LockToken && null == second.LockToken
                    || string.Equals(first.LockToken, second.LockToken, StringComparison.Ordinal)
                );
        }

        public static bool operator !=(StorageDataEntry first, StorageDataEntry second)
        {
            return first.Data != second.Data
                ||
                (
                    (null == first.LockToken && null != second.LockToken)
                    || (null != first.LockToken && null == second.LockToken)
                    || !string.Equals(first.LockToken, second.LockToken, StringComparison.Ordinal)
                );
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj is not StorageDataEntry storageDataEntry)
            {
                return false;
            }

            return this.GetHashCode() == storageDataEntry.GetHashCode();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Data, null == this.LockToken ? string.Empty : this.LockToken, this.ExpiresAt ?? DateTime.MaxValue);
        }
    }
}
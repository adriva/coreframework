using System;

namespace Adriva.Storage.Abstractions
{
    public class BlobItemProperties
    {
        public static BlobItemProperties NotExists { get; private set; } = new BlobItemProperties() { Exists = false };

        public bool Exists { get; private set; }

        public long Length { get; private set; }

        public string ETag { get; private set; }

        public DateTimeOffset? LastModified { get; private set; }

        private BlobItemProperties() { }

        public BlobItemProperties(long length, string eTag, DateTimeOffset? lastModified)
        {
            this.Exists = true;
            this.Length = length;
            this.ETag = eTag;
            this.LastModified = lastModified;
        }
    }
}
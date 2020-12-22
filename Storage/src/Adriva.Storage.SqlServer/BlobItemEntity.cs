using System;

namespace Adriva.Storage.SqlServer
{
    internal class BlobItemEntity
    {
        public long Id { get; set; }

        public string ContainerName { get; set; }

        public string Name { get; set; }

        public byte[] Content { get; set; }

        public long Length { get; set; }

        public string ETag { get; set; }

        public DateTime LastModifiedUtc { get; set; }

    }
}

using System;

namespace Adriva.Storage.Abstractions
{
    public interface ITableRow
    {
        string PartitionKey { get; set; }

        string RowKey { get; set; }

        DateTimeOffset Timestamp { get; set; }

        string ETag { get; set; }

        void ReadEntity(PropertyBag properties);

        PropertyBag WriteEntity();
    }
}
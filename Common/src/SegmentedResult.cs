using System;
using System.Collections.Generic;

namespace Adriva.Common.Core
{

    public sealed class SegmentedResult<T> : ISegmentedResult
    {

        public static readonly SegmentedResult<T> Empty = new SegmentedResult<T>(Array.Empty<T>(), null, false);

        public bool HasMore { get; private set; }

        public string ContinuationToken { get; private set; }

        public IEnumerable<T> Items { get; private set; }

        public SegmentedResult(IEnumerable<T> items)
            : this(items, null, false)
        {

        }

        public SegmentedResult(IEnumerable<T> items, string continuationToken, bool hasMore)
        {
            this.Items = items;
            this.ContinuationToken = continuationToken;
            this.HasMore = hasMore;
        }
    }
}

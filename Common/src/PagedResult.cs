using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Adriva.Common.Core
{
    /// <summary>
    /// Represents a single page of data items where there may be more pages.
    /// </summary>
    /// <typeparam name="T">The type of items in the page.</typeparam>
    public sealed class PagedResult<T> : IPagedResult
    {
        /// <summary>
        /// Gets an empty page of data items which has only one page.
        /// </summary>
        /// <typeparam name="T">The type of items in the page.</typeparam>
        /// <returns>A PagedResult object which has no data items.</returns>
        [JsonIgnore]
        public static PagedResult<T> Empty { get; private set; } = new PagedResult<T>(Array.Empty<T>(), 0, 0, 0);

        /// <summary>
        /// Gets the index of the current page within the whole availabe pages. 
        /// </summary>
        /// <value>A System.Int32 value representing the current page index.</value>
        [JsonProperty("pageNumber")]
        public int PageNumber { get; private set; }

        [JsonProperty("pageCount")]
        public int PageCount { get; private set; }

        [JsonProperty("recordCount")]
        public long RecordCount { get; private set; }

        [JsonProperty("items")]
        public IEnumerable<T> Items { get; private set; }

        public PagedResult(IEnumerable<T> items)
            : this(items, 0, 1, items.Count())
        {

        }

        public PagedResult(IEnumerable<T> items, int pageIndex, int pageCount, long recordCount)
        {
            this.Items = items;
            this.PageNumber = Math.Max(1, pageIndex);
            this.PageCount = Math.Max(0, pageCount);
            this.RecordCount = recordCount;
        }

        [JsonConstructor]
        public PagedResult()
        {

        }
    }
}

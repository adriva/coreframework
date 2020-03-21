namespace Adriva.Common.Core
{
    /// <summary>
    /// Represents a single page of a resultset which may have more pages.
    /// </summary>
    public interface IPagedResult
    {
        /// <summary>
        /// Gets the current page index of the resultset.
        /// </summary>
        /// <value>An Int32 value representing the current page index.</value>
        int PageIndex { get; }

        /// <summary>
        /// Gets the total number of pages of the overall resultset.
        /// </summary>
        /// <value>An Int32 value representing the count of all pages.</value>
        int PageCount { get; }

        /// <summary>
        /// Gets the total number of items available in the overall resultset.
        /// </summary>
        /// <value>An Int32 value representing the count of all items.</value>
        long RecordCount { get; }
    }
}

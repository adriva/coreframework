namespace Adriva.Common.Core
{
    /// <summary>
    /// Represents a partial result set which may have more segments.
    /// </summary>
    public interface ISegmentedResult
    {
        /// <summary>
        /// Gets a value if the current resultset has more segments.
        /// </summary>
        /// <value>True if the resultset has more segments, otherwise False.</value>
        bool HasMore { get; }

        /// <summary>
        /// Gets a value representing the token value to fetch the next segment's data.
        /// </summary>
        /// <value>A string value storing the token value. Token value is implementation specific and depends on the data source.</value>
        string ContinuationToken { get; }
    }
}

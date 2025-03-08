namespace Adriva.Extensions.Caching.SqlServer
{
    internal static class Columns
    {
        public static class Names
        {
            public const string CacheItemId = "Id";
            public const string CacheItemValue = "Value";
            public const string ExpiresAtTime = "ExpiresAtTime";
            public const string SlidingExpirationInSeconds = "SlidingExpirationInSeconds";
            public const string AbsoluteExpiration = "AbsoluteExpiration";
        }

        public static class DependencyNames
        {
            public const string Key = "Key";
            public const string CacheKey = "CacheKey";
        }

        public static class Indexes
        {
            // The value of the following index positions is dependent on how the SQL queries
            // are selecting the columns.
            public const int CacheItemValueIndex = 0;
        }
    }
}
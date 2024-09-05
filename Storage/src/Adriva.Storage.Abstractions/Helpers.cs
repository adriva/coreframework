namespace Microsoft.Extensions.DependencyInjection
{
    public static class Helpers
    {
        private const string QueuePrefix = "queue";
        private const string BlobPrefix = "blob";
        private const string TablePrefix = "table";

        public static string GetQualifiedQueueName(string name) => string.Concat(Helpers.QueuePrefix, ":", name);

        public static string GetQualifiedBlobName(string name) => string.Concat(Helpers.BlobPrefix, ":", name);

        public static string GetQualifiedTableName(string name) => string.Concat(Helpers.TablePrefix, ":", name);
    }
}
namespace Microsoft.Extensions.DependencyInjection
{
    public static class Helpers
    {
        private const string QueuePrefix = "queue";
        private const string BlobPrefix = "blob";

        public static string GetQualifiedQueueName(string name) => string.Concat(Helpers.QueuePrefix, ":", name);

        public static string GetQualifiedBlobName(string name) => string.Concat(Helpers.BlobPrefix, ":", name);
    }
}
namespace Microsoft.Extensions.DependencyInjection
{
    public static class Helpers
    {
        private const string QueuePrefix = "queue";

        public static string GetQualifiedQueueName(string name) => string.Concat(Helpers.QueuePrefix, ":", name);
    }
}
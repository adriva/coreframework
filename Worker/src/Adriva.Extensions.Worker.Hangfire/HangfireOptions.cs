namespace Adriva.Extensions.Worker.Hangfire
{
    public class HangfireOptions
    {
        public int AutomaticRetryCount { get; set; }

        public string ConnectionString { get; set; }

        public string SchemaName { get; set; } = "rt";
    }
}

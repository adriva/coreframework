namespace Adriva.Extensions.Reporting.Abstractions
{
    public class DataSourceDefinition : DynamicDefinition
    {
        public string Type { get; set; }

        public string ConnectionString { get; set; }
    }
}
namespace Adriva.Extensions.Reporting.Abstractions
{
    public class CommandDefinition : DynamicDefinition
    {
        public string CommandText { get; set; }

        public string DataSource { get; set; }
    }
}
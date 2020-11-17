namespace Adriva.Extensions.Reporting.Abstractions
{
    public sealed class ReportCommandParameter
    {
        public string Name { get; private set; }

        public FilterValue FilterValue { get; private set; }

        public ReportCommandParameter(string name, FilterValue filterValue)
        {
            this.Name = name;
            this.FilterValue = filterValue;
        }
    }
}
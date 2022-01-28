namespace Adriva.Extensions.Reporting.Abstractions
{
    public sealed class ReportOutput
    {
        public ReportCommand Command { get; private set; }

        public DataSet DataSet { get; private set; }

        public ReportOutput(ReportCommand reportCommand, DataSet dataSet)
        {
            this.Command = reportCommand;
            this.DataSet = dataSet;
        }
    }
}
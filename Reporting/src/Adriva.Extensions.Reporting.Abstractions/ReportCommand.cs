using System.Collections.Generic;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public sealed class ReportCommand
    {
        public IList<ReportCommandParameter> Parameters { get; private set; }

        public CommandDefinition CommandDefinition { get; private set; }

        public string Text { get; private set; }

        public ReportCommand(string text, CommandDefinition commandDefinition)
        {
            this.Text = text;
            this.CommandDefinition = commandDefinition;
            this.Parameters = new List<ReportCommandParameter>();
        }
    }
}
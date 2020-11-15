using System;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class ReportCommandContext : ReportContext
    {
        public CommandDefinition CommandDefinition {get; private set;}

        public ReportCommandContext(ReportDefinition reportDefinition, string commandName)  : base(reportDefinition){
            if (!reportDefinition.Commands.TryGetValue(commandName, out CommandDefinition commandDefinition)){
                throw new ArgumentException($"Command '{commandName}' could not be found in the report definition.");
            }

            this.CommandDefinition = commandDefinition;
        }
    }
}

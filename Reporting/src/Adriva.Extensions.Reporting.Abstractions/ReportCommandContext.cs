using System;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class ReportCommandContext : ReportContext
    {
        public string CommandName { get; private set; }

        public CommandDefinition CommandDefinition { get; private set; }

        public static ReportCommandContext Create(IServiceProvider serviceProvider, ReportDefinition reportDefinition, string commandName)
        {
            return new ReportCommandContext(serviceProvider, reportDefinition, commandName);
        }

        protected ReportCommandContext(IServiceProvider serviceProvider, ReportDefinition reportDefinition, string commandName) : base(serviceProvider, reportDefinition)
        {
            if (null == commandName)
            {
                throw new ArgumentNullException(nameof(commandName), "Command name cannot be null");
            }

            if (!reportDefinition.Commands.TryGetValue(commandName, out CommandDefinition commandDefinition))
            {
                throw new ArgumentException($"Command '{commandName}' could not be found in the report definition.");
            }

            this.CommandDefinition = commandDefinition;
            this.CommandName = commandName;
        }
    }
}


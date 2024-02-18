using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class DefaultCommandBuilder : ICommandBuilder
    {
        protected IFilterValueBinder FilterValueBinder { get; private set; }

        public DefaultCommandBuilder(IFilterValueBinder filterValueBinder)
        {
            this.FilterValueBinder = filterValueBinder;
        }

        protected virtual bool TryParseCommandText(string text, out string commandText, out IList<string> parameterNames)
        {
            commandText = null;
            parameterNames = null;

            var matches = Regex.Matches(text, @"(?<parameterName>\@\w+)");

            if (null == matches) return false;

            parameterNames = new List<string>();

            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    var parameterNameGroup = match.Groups["parameterName"];

                    if (parameterNameGroup.Success)
                    {
                        parameterNames.Add(parameterNameGroup.Value);
                    }
                }
            }

            commandText = text;

            return !string.IsNullOrWhiteSpace(commandText);
        }

        public async Task<ReportCommand> BuildCommandAsync(ReportCommandContext context, IDictionary<string, string> values, bool allowArbitraryValues)
        {
            if (!this.TryParseCommandText(context.CommandDefinition.CommandText, out string commandText, out IList<string> parameterNames))
            {
                throw new InvalidOperationException($"Failed to parse command '{context.CommandName}' in report '{context.ReportDefinition.Name}'.");
            }

            ReportCommand reportCommand = new ReportCommand(commandText, context.CommandDefinition);

            if (null == values) values = new Dictionary<string, string>(new FilterNameComparer());
            else values = new Dictionary<string, string>(values, new FilterNameComparer());

            foreach (string parameterName in parameterNames)
            {
                if (context.ReportDefinition.TryFindFilterDefinition(parameterName, out FilterDefinition filterDefinition))
                {
                    if (!values.TryGetValue(parameterName, out string rawValue))
                    {
                        rawValue = null;
                    }

                    FilterValue filterValue = await this.FilterValueBinder.GetFilterValueAsync(context, filterDefinition, rawValue);
                    ReportCommandParameter reportCommandParameter = new ReportCommandParameter(parameterName, filterValue);
                    reportCommand.Parameters.Add(reportCommandParameter);
                }
                else if (allowArbitraryValues)
                {
                    if (!values.TryGetValue(parameterName, out string rawValue))
                    {
                        rawValue = null;
                    }
                    FilterValue filterValue = new FilterValue(rawValue, rawValue);
                    ReportCommandParameter reportCommandParameter = new ReportCommandParameter(parameterName, filterValue);
                    reportCommand.Parameters.Add(reportCommandParameter);
                }
                else
                {
                    throw new InvalidOperationException($"Filter definition for command parameter '{parameterName}' could not be found. (Command name is '{context.CommandName}'.)");
                }
            }

            return reportCommand;
        }
    }
}
using System.Collections.Generic;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public interface ICommandBuilder
    {
        ReportCommand BuildCommand(ReportCommandContext context, IDictionary<string, string> values);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public interface ICommandBuilder
    {
        Task<ReportCommand> BuildCommandAsync(ReportCommandContext context, IDictionary<string, string> values, bool allowArbitraryValues);
    }
}
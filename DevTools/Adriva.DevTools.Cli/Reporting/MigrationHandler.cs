using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Adriva.DevTools.Cli.Reporting
{
    internal sealed class MigrationHandler : BaseHandler
    {
        public MigrationHandler(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }

        [CommandHandler("update-report")]
        [CommandArgument("input", Aliases = new[] { "-i" }, IsRequired = true, Type = typeof(FileInfo))]
        public async Task InvokeAsync(FileInfo input)
        {
            if (!input.Exists)
            {
                this.Logger.LogError($"Input file '{input.FullName}' does not exist.");
            }

            await Task.CompletedTask;
        }
    }
}
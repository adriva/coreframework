using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Adriva.DevTools.Cli.Reporting
{
    internal sealed class LegacyReportMapper
    {

    }

    internal sealed class MigrationHandler : BaseHandler
    {
        private readonly IServiceProvider ServiceProvider;

        public MigrationHandler(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            this.ServiceProvider = serviceProvider;
        }

        [CommandHandler("update-report")]
        [CommandArgument("--repository", Aliases = new[] { "-r" }, IsRequired = false, Type = typeof(DirectoryInfo), Description = "Path of the reports repository. Required if --combined flag is set.")]
        [CommandArgument("--report", Aliases = new[] { "-n" }, IsRequired = true, Type = typeof(string), Description = "Name of the legacy report if --combined flag is set, otherwise the path of the legacy report.")]
        [CommandArgument("--output", Aliases = new[] { "-o" }, IsRequired = false, Type = typeof(DirectoryInfo), Description = "Output folder where the new report will be generated. (Default: Current directory).")]
        [CommandArgument("--combined", Aliases = new[] { "-c" }, IsRequired = false, Type = typeof(bool), Description = "Resolves base reports that may be defined in the legacy report and combines them all into one output. (Default: False)")]
        public async Task InvokeAsync(DirectoryInfo repository, DirectoryInfo output, string report, bool combined)
        {
            if (combined && null == repository)
            {
                throw new InvalidProgramException($"Combined flag requires the repository to be set. (--repository parameter)");
            }

            if (!combined)
            {
                repository = new DirectoryInfo(Path.GetDirectoryName(report));
                report = $"{Path.GetFileNameWithoutExtension(report)}.json";
            }



            await Task.CompletedTask;
        }
    }
}
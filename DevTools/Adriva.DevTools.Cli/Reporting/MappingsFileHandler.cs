using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Adriva.DevTools.Cli.Reporting
{

    internal sealed class MappingsFileHandler : BaseHandler
    {
        private readonly IServiceProvider ServiceProvider;

        public MappingsFileHandler(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            this.ServiceProvider = serviceProvider;
        }

        [CommandHandler("create-mappings-file", Description = "Creates a sample text mappings file")]
        [CommandArgument("--path", Aliases = new[] { "-p" }, IsRequired = false, Type = typeof(DirectoryInfo), Description = "Path of the sample report updater mappings file that will be generated. (Defaults to current directory)")]
        public async Task InvokeAsync(DirectoryInfo path)
        {
            if (null == path)
            {
                path = new DirectoryInfo(Directory.GetCurrentDirectory());
            }

            if (!path.Exists)
            {
                this.Logger.LogError($"Sepcified output path '{path}' does not exist. Output path should be a valid directory.");
                return;
            }

            IList<string> sampleMappings = new List<string>(){
                string.Empty,
                "# This line is a comment. You can start lines with '#' to mark them as comments",
                string.Empty,
                "# Mapping old controls to new ones",
                "'old_control_name' : 'new_control_name'",
                "'dropdown' : 'newdropdown'",
                "'textbox' : 'b-form-input'",
                string.Empty,
                "# Mapping formatter targets to new ones",
                "'Adriva.Extensions.Reports.ValueFormatters:AddDays, Adriva.Extensions.Reports' : 'Namespace.TypeName:MethodName, LibraryName'",
                string.Empty,
                "# Case sensitive mapping. You can add CS at the end of line to mark the mapping as Case Sensitive.",
                "'OldNaMe' : 'newNamE' : CS",
                string.Empty
            };

            StringBuilder buffer = new StringBuilder();

            this.Logger.LogTrace("Sample file content is:");

            foreach (var sampleMapping in sampleMappings)
            {
                buffer.AppendLine(sampleMapping.Replace('\'', '"'));
            }

            this.Logger.LogTrace(buffer.ToString());

            string outputFilePath = Path.Combine(path.FullName, "mappings.txt");
            await File.WriteAllTextAsync(outputFilePath, buffer.ToString(), Encoding.UTF8);
            this.Logger.LogInformation($"File written '{outputFilePath}'.");
        }
    }
}
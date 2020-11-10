using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Adriva.Common.Core;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class FileSystemReportRepository : IReportRepository
    {
        private const string Extension = ".json";

        private readonly FileSystemReportRepositoryOptions Options;
        private readonly IHostEnvironment HostingEnvironment;
        private readonly IFileProvider FileProvider;

        public FileSystemReportRepository(IHostEnvironment hostingtEnvironment, IOptions<FileSystemReportRepositoryOptions> optionsAccessor)
        {
            this.HostingEnvironment = hostingtEnvironment;
            this.Options = optionsAccessor.Value;
            this.FileProvider = new PhysicalFileProvider(this.Options.RootPath);
        }

        private async Task<string> ResolveBasePathAsync(IFileInfo fileInfo)
        {
            if (null == fileInfo) throw new ArgumentNullException(nameof(fileInfo));

            using (var stream = fileInfo.CreateReadStream())
            using (var streamReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                while (await jsonReader.ReadAsync())
                {
                    if (JsonToken.String == jsonReader.TokenType && 0 == string.Compare("base", jsonReader.Path, StringComparison.OrdinalIgnoreCase))
                    {
                        string basePath = Convert.ToString(jsonReader.Value);
                        if (string.IsNullOrWhiteSpace(basePath)) return null;
                        if (0 != string.Compare(Path.GetExtension(basePath), FileSystemReportRepository.Extension, StringComparison.OrdinalIgnoreCase))
                        {
                            basePath += FileSystemReportRepository.Extension;
                        }
                        return basePath;
                    }
                }
            }

            return null;
        }

        private async Task<ReportDefinitionFile> ResolveReportDefinitionAsync(string name)
        {
            if (0 != string.Compare(FileSystemReportRepository.Extension, Path.GetExtension(name), StringComparison.OrdinalIgnoreCase))
            {
                name += FileSystemReportRepository.Extension;
            }

            IFileInfo reportFileInfo = null;
            IFileInfo environmentSpecificFileInfo = this.FileProvider.GetFileInfo(Path.Combine(this.HostingEnvironment.EnvironmentName, name));
            if (!environmentSpecificFileInfo.Exists || environmentSpecificFileInfo.IsDirectory)
            {
                IFileInfo genericFileInfo = this.FileProvider.GetFileInfo(name);
                if (!genericFileInfo.Exists || genericFileInfo.IsDirectory)
                {
                    throw new FileNotFoundException($"Report definition '{name}' could not be found.");
                }
                reportFileInfo = genericFileInfo;
            }
            else
            {
                reportFileInfo = environmentSpecificFileInfo;
            }

            if (null == reportFileInfo)
            {
                throw new FileNotFoundException($"Report definition '{name}' could not be found.");
            }

            string basePath = await this.ResolveBasePathAsync(reportFileInfo);
            return new ReportDefinitionFile()
            {
                Name = reportFileInfo.Name,
                Path = reportFileInfo.GetDirectoryName(this.Options.RootPath),
                Base = basePath
            };
        }

        private async Task<IList<ReportDefinitionFile>> ResolveReportDefinitionChainAsync(ReportDefinitionFile reportDefinitionFile)
        {
            IList<ReportDefinitionFile> output = new List<ReportDefinitionFile>();
            string directoryName = reportDefinitionFile.Path;

            while (!string.IsNullOrWhiteSpace(directoryName))
            {
                if (Path.GetRelativePath(this.Options.RootPath, Path.Combine(this.Options.RootPath, directoryName)).StartsWith("..", StringComparison.OrdinalIgnoreCase))
                {
                    throw new FileNotFoundException($"Failed to locate the base report definition '{reportDefinitionFile.Base}'.");
                }

                string testPath = Path.Combine(directoryName, reportDefinitionFile.Base);
                var baseFile = this.FileProvider.GetFileInfo(testPath);
                if (baseFile.Exists)
                {
                    var baseOfBase = await this.ResolveBasePathAsync(baseFile);
                    var baseDefinition = new ReportDefinitionFile()
                    {
                        Name = baseFile.Name,
                        Base = baseOfBase,
                        Path = baseFile.GetDirectoryName(this.Options.RootPath)
                    };
                    reportDefinitionFile.Base = baseDefinition.Base;
                    output.Add(baseDefinition);

                    if (null == baseOfBase) break;
                }
                else
                {
                    directoryName = Path.Combine(this.Options.RootPath, directoryName, "..");
                    directoryName = Path.GetRelativePath(this.Options.RootPath, directoryName);
                }
            }

            return output;
        }

        private async Task<IEnumerable<ReportDefinitionFile>> ResolveReportDefinitionChainAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var reportDefinitionFile = await this.ResolveReportDefinitionAsync(name);
            var baseChain = await this.ResolveReportDefinitionChainAsync(reportDefinitionFile);

            List<ReportDefinitionFile> output = new List<ReportDefinitionFile>();
            output.Add(reportDefinitionFile);
            output.AddRange(baseChain);
            output.Reverse();
            return output;
        }

        public async Task<ReportDefinition> LoadReportDefinitionAsync(string name)
        {
            var reportDefinitionFiles = await this.ResolveReportDefinitionChainAsync(name);

            if (null == reportDefinitionFiles || !reportDefinitionFiles.Any()) return null;

            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(this.Options.RootPath);

            foreach (var reportDefinitionFile in reportDefinitionFiles)
            {
                configurationBuilder.AddJsonFile(Path.Combine(reportDefinitionFile.Path, reportDefinitionFile.Name), true);
            }

            var reportConfiguration = configurationBuilder.Build();
            return reportConfiguration.Get<ReportDefinition>();
        }
    }
}
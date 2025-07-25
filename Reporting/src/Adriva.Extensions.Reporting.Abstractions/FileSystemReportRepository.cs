using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class FileSystemReportRepository : IReportRepository
    {
        private const string Extension = ".json";

        private readonly FileSystemReportRepositoryOptions Options;
        private readonly IHostEnvironment HostingEnvironment;
        private readonly IFileProvider FileProvider;
        private readonly ILogger Logger;

        public FileSystemReportRepository(IHostEnvironment hostingtEnvironment, IOptions<FileSystemReportRepositoryOptions> optionsAccessor, ILogger<FileSystemReportRepository> logger)
        {
            this.HostingEnvironment = hostingtEnvironment;
            this.Options = optionsAccessor.Value;
            this.FileProvider = new PhysicalFileProvider(this.Options.RootPath);
            this.Logger = logger;
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

        public async Task<RepositoryFile> GetRepositoryFileAsync(string name, bool isReportDefinition)
        {
            string extension = Path.GetExtension(name);

            if (string.IsNullOrWhiteSpace(extension))
            {
                name += FileSystemReportRepository.Extension;
            }

            IFileInfo fileInfo = null;
            IFileInfo environmentSpecificFileInfo = this.FileProvider.GetFileInfo(Path.Combine(this.HostingEnvironment.EnvironmentName, name));

            if (!environmentSpecificFileInfo.Exists || environmentSpecificFileInfo.IsDirectory)
            {
                IFileInfo genericFileInfo = this.FileProvider.GetFileInfo(name);
                if (!genericFileInfo.Exists || genericFileInfo.IsDirectory)
                {
                    return RepositoryFile.NotExists;
                }
                fileInfo = genericFileInfo;
            }
            else
            {
                fileInfo = environmentSpecificFileInfo;
            }

            if (null == fileInfo)
            {
                return RepositoryFile.NotExists;
            }

            string basePath = null;

            if (isReportDefinition)
            {
                basePath = await this.ResolveBasePathAsync(fileInfo);
            }
            else
            {
                basePath = fileInfo.PhysicalPath;
            }

            return new RepositoryFile()
            {
                Name = fileInfo.Name,
                Path = fileInfo.GetDirectoryName(this.Options.RootPath),
                Base = basePath
            };
        }

        private async Task<IList<RepositoryFile>> ResolveReportDefinitionChainAsync(RepositoryFile reportDefinitionFile)
        {
            IList<RepositoryFile> output = [];

            RepositoryFile current = reportDefinitionFile;

            while (!string.IsNullOrWhiteSpace(current.Base))
            {
                var baseFile = await this.GetRepositoryFileAsync(Path.Combine(current.Path, current.Base), true);

                if (RepositoryFile.NotExists.Equals(baseFile))
                {
                    baseFile = await this.GetRepositoryFileAsync(current.Base, true);
                }

                if (!RepositoryFile.NotExists.Equals(baseFile))
                {
                    output.Add(baseFile);
                    current = baseFile;
                }
                else
                {
                    throw new FileNotFoundException($"Failed to locate the base report definition '{current.Base}'.");
                }
            }

            return output;
        }

        private async Task<IEnumerable<RepositoryFile>> ResolveReportDefinitionChainAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var reportDefinitionFile = await this.GetRepositoryFileAsync(name, true);

            if (RepositoryFile.NotExists.Equals(reportDefinitionFile))
            {
                throw new IOException($"Report definition '{name}' could not be found in registered repositories.");
            }

            var baseChain = await this.ResolveReportDefinitionChainAsync(reportDefinitionFile);

            List<RepositoryFile> output = [reportDefinitionFile, .. baseChain];
            output.Reverse();
            return output;
        }

        public Stream OpenReadStream(RepositoryFile repositoryFile)
        {
            if (RepositoryFile.NotExists.Equals(repositoryFile))
            {
                throw new FileNotFoundException($"Repository file not found.");
            }

            string targetPath = Path.Combine(repositoryFile.Path, repositoryFile.Name);
            var fileInfo = this.FileProvider.GetFileInfo(targetPath);

            if (!fileInfo.Exists || fileInfo.IsDirectory)
            {
                throw new FileNotFoundException($"Repository file not found.");
            }

            return fileInfo.CreateReadStream();
        }

        public async Task<ReportDefinition> LoadReportDefinitionAsync(string name)
        {
            var reportDefinitionFiles = await this.ResolveReportDefinitionChainAsync(name);

            if (null == reportDefinitionFiles || !reportDefinitionFiles.Any())
            {
                this.Logger.LogError($"Couldn't resolve the definition chain for report '{name}'.");
                return null;
            }

            JObject definitionObject = [];

            foreach (var reportDefinitionFile in reportDefinitionFiles)
            {
                this.Logger.LogTrace($"Using report definition file '{reportDefinitionFile.Path}' to construct report '{name}'.");

                using (StreamReader streamReader = File.OpenText(Path.Combine(this.Options.RootPath, reportDefinitionFile.Path, reportDefinitionFile.Name)))
                using (JsonTextReader jsonReader = new(streamReader))
                {
                    definitionObject.Merge(await JObject.ReadFromAsync(jsonReader), new JsonMergeSettings()
                    {
                        MergeArrayHandling = MergeArrayHandling.Union,
                        MergeNullValueHandling = MergeNullValueHandling.Ignore,
                        PropertyNameComparison = StringComparison.OrdinalIgnoreCase
                    });
                }
            }

            this.Logger.LogTrace($"Final definition for the report '{name}' is:");
            this.Logger.LogTrace(definitionObject?.ToString() ?? "NULL");

            var reportDefinition = definitionObject.ToObject<ReportDefinition>();
            return reportDefinition;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Extensions.Reporting.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Adriva.DevTools.Cli.Reporting
{

    internal sealed class MigrationHandler : BaseHandler
    {
        private readonly IServiceProvider ServiceProvider;

        private static T GetJsonValue<T>(JToken jtoken, string name)
        {
            if (null == jtoken) return default(T);

            if (JTokenType.Object == jtoken.Type)
            {
                JObject jobject = (JObject)jtoken;
                if (!jobject.ContainsKey(name))
                {
                    return default(T);
                }
                return jobject[name].ToObject<T>();
            }
            else
            {
                return jtoken.Value<T>(name);
            }
        }

        private static void ConvertFilterOptions(JToken filterToken, FilterDefinition filterDefinition, TextMappingsManager textMappingsManager)
        {
            if (null == filterToken || null == filterDefinition) return;

            if (JTokenType.Object == filterToken.Type)
            {
                if (filterToken is JObject filterObject)
                {
                    string viewName = MigrationHandler.GetJsonValue<string>(filterObject["rendererOptions"]?["mvc"], "view");

                    if (!string.IsNullOrWhiteSpace(viewName))
                    {
                        filterDefinition.Options = JToken.FromObject(new
                        {
                            control = textMappingsManager.GetSubstitution(viewName)
                        });
                    }
                }
            }
        }

        public MigrationHandler(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            this.ServiceProvider = serviceProvider;
        }

        [CommandHandler("update-report")]
        [CommandArgument("--path", Aliases = new[] { "-p" }, IsRequired = false, Type = typeof(DirectoryInfo), Description = "Root path of the reports repository.")]
        [CommandArgument("--report", Aliases = new[] { "-n" }, IsRequired = true, Type = typeof(string), Description = "Name of the legacy report.")]
        [CommandArgument("--mappings", Aliases = new[] { "-m" }, IsRequired = false, Type = typeof(FileInfo), Description = "Path of a user provided string mappings file.")]
        [CommandArgument("--output", Aliases = new[] { "-o" }, IsRequired = false, Type = typeof(DirectoryInfo), Description = "Output folder where the new report will be generated. (Default: Current directory).")]
        public async Task InvokeAsync(DirectoryInfo path, DirectoryInfo output, string report, FileInfo mappings)
        {
            if (null == output)
            {
                output = new DirectoryInfo(Directory.GetCurrentDirectory());
            }

            TextMappingsManager textMappingsManager = new TextMappingsManager(mappings);
            await textMappingsManager.InitializeAsync();

            FileInfo legacyReportFile = path.GetFiles($"{report}.json").FirstOrDefault();

            using (var fileStream = legacyReportFile.OpenRead())
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var legacyReportRoot = await JToken.ReadFromAsync(jsonReader);

                ReportDefinition reportDefinition = new ReportDefinition();

                reportDefinition.Base = MigrationHandler.GetJsonValue<string>(legacyReportRoot, "baseReport");
                reportDefinition.ContextProvider = MigrationHandler.GetJsonValue<string>(legacyReportRoot, "contextProvider");
                reportDefinition.Name = MigrationHandler.GetJsonValue<string>(legacyReportRoot, "title");

                var queries = MigrationHandler.GetJsonValue<Dictionary<string, JToken>>(legacyReportRoot, "queries");
                var filters = MigrationHandler.GetJsonValue<List<JToken>>(legacyReportRoot, "filters");

                if (null != queries && 0 < queries.Count)
                {
                    reportDefinition.Commands = new StringKeyDictionary<CommandDefinition>();
                    foreach (var query in queries)
                    {
                        CommandDefinition commandDefinition = new CommandDefinition()
                        {
                            CommandText = MigrationHandler.GetJsonValue<string>(query.Value, "command")
                        };

                        reportDefinition.Commands.Add(query.Key, commandDefinition);
                    }
                }

                if (null != filters && 0 < filters.Count)
                {
                    reportDefinition.Filters = new FilterDefinitionDictionary();
                    foreach (var filter in filters)
                    {
                        string filterName = MigrationHandler.GetJsonValue<string>(filter, "name");
                        var dataType = MigrationHandler.GetJsonValue<TypeCode>(filter, "dataType");
                        var defaultValue = MigrationHandler.GetJsonValue<object>(filter, "defaultValue");

                        if (defaultValue is string stringDefaultValue && string.IsNullOrWhiteSpace(stringDefaultValue))
                        {
                            defaultValue = null;
                        }

                        if (TypeCode.Empty == dataType)
                        {
                            dataType = TypeCode.String;
                        }

                        FilterDefinition filterDefinition = new FilterDefinition()
                        {
                            Fields = null,
                            DefaultValue = defaultValue,
                            DefaultValueFormatter = textMappingsManager.GetSubstitution(MigrationHandler.GetJsonValue<string>(filter, "defaultValueFormatter")),
                            DataType = dataType,
                            DisplayName = MigrationHandler.GetJsonValue<string>(filter, "title"),
                            Command = MigrationHandler.GetJsonValue<string>(filter, "query"),
                            DataSource = MigrationHandler.GetJsonValue<string>(filter, "dataSource"),
                        };

                        MigrationHandler.ConvertFilterOptions(filter, filterDefinition, textMappingsManager);

                        reportDefinition.Filters.Add(filterName, filterDefinition);
                    }
                }

                System.Console.WriteLine(Utilities.SafeSerialize(reportDefinition, new JsonSerializerSettings()
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    Formatting = Formatting.Indented,
                    ContractResolver = new DefaultContractResolver()
                    {
                        NamingStrategy = new CamelCaseNamingStrategy(false, false, false)
                    }
                }));
            }
        }
    }
}
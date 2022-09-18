using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Extensions.Reporting.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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

            if (string.IsNullOrWhiteSpace(name)) return default(T);

            if (JTokenType.Object == jtoken.Type)
            {
                JObject jobject = (JObject)jtoken;

                if (name.Contains('.', StringComparison.Ordinal))
                {
                    var pathToken = jobject.SelectToken(name);
                    if (null != pathToken)
                    {
                        return pathToken.Value<T>();
                    }
                }

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
                    Dictionary<string, object> optionsData = new Dictionary<string, object>();

                    string viewName = MigrationHandler.GetJsonValue<string>(filterObject["rendererOptions"]?["mvc"], "view");
                    string cssClass = MigrationHandler.GetJsonValue<string>(filterObject["rendererOptions"]?["mvc"]?["Properties"], "cssClass");

                    if (!string.IsNullOrWhiteSpace(viewName))
                    {
                        optionsData["editor"] = textMappingsManager.GetSubstitution(viewName, true);
                    }

                    if (!string.IsNullOrWhiteSpace(cssClass))
                    {
                        optionsData["containerClass"] = textMappingsManager.GetSubstitution(cssClass);
                    }

                    if (0 < optionsData.Count)
                    {
                        filterDefinition.Options = JToken.FromObject(optionsData);
                    }
                }
            }
        }

        public MigrationHandler(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            this.ServiceProvider = serviceProvider;
        }

        [CommandHandler("update-report", Description = "Migrates Adriva Reports version 2 json definitions to version 3")]
        [CommandArgument("--path", Aliases = new[] { "-p" }, IsRequired = true, Type = typeof(DirectoryInfo), Description = "Root path of the reports repository.")]
        [CommandArgument("--report", Aliases = new[] { "-n" }, IsRequired = true, Type = typeof(string), Description = "Name of the legacy report. * wildcard is supported.")]
        [CommandArgument("--mappings", Aliases = new[] { "-m" }, IsRequired = false, Type = typeof(FileInfo), Description = "Path of a user provided string mappings file.")]
        [CommandArgument("--output", Aliases = new[] { "-o" }, IsRequired = false, Type = typeof(DirectoryInfo), Description = "Output folder where the new report will be generated.")]
        public async Task InvokeAsync(DirectoryInfo path, DirectoryInfo output, string report, FileInfo mappings)
        {
            if (null == output)
            {
                output = new DirectoryInfo(Directory.GetCurrentDirectory());
            }
            else
            {
                if (!output.Exists)
                {
                    output.Create();
                }
            }
            bool hasWildcard = report.Contains("*", StringComparison.Ordinal);
            TextMappingsManager textMappingsManager = new TextMappingsManager(mappings);
            await textMappingsManager.InitializeAsync();

            FileInfo[] legacyReportFiles = path.GetFiles($"{report}.json", hasWildcard ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            foreach (var legacyReportFile in legacyReportFiles)
            {
                this.Logger.LogInformation($"Processing legacy report {legacyReportFile.FullName}.");

                try
                {
                    using (var fileStream = legacyReportFile.OpenRead())
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true))
                    using (var jsonReader = new JsonTextReader(streamReader))
                    {
                        var legacyReportRoot = await JToken.ReadFromAsync(jsonReader);

                        ReportDefinition reportDefinition = new ReportDefinition();

                        reportDefinition.Base = MigrationHandler.GetJsonValue<string>(legacyReportRoot, "baseReport");
                        reportDefinition.ContextProvider = textMappingsManager.GetSubstitution(MigrationHandler.GetJsonValue<string>(legacyReportRoot, "contextProvider"));
                        reportDefinition.Name = MigrationHandler.GetJsonValue<string>(legacyReportRoot, "title");

                        var dataSources = MigrationHandler.GetJsonValue<Dictionary<string, JToken>>(legacyReportRoot, "dataSources");
                        var queries = MigrationHandler.GetJsonValue<Dictionary<string, JToken>>(legacyReportRoot, "queries");
                        var filters = MigrationHandler.GetJsonValue<List<JToken>>(legacyReportRoot, "filters");
                        var legacyOutput = MigrationHandler.GetJsonValue<JToken>(legacyReportRoot, "output");

                        this.Logger.LogTrace($"Processing {dataSources?.Count ?? 0} data sources for '{legacyReportFile.FullName}'.");

                        if (null != dataSources && 0 < dataSources.Count)
                        {
                            reportDefinition.DataSources = new StringKeyDictionary<DataSourceDefinition>();

                            foreach (var dataSource in dataSources)
                            {
                                if (!(dataSource.Value is JValue))
                                {
                                    base.RunWithStepOver(() =>
                                    {
                                        DataSourceDefinition dataSourceDefinition = new DataSourceDefinition()
                                        {
                                            ConnectionString =
                                                                                    MigrationHandler.GetJsonValue<string>(dataSource.Value["parameters"], "connectionString")
                                                                                    ??
                                                                                    MigrationHandler.GetJsonValue<string>(dataSource.Value["parameters"], "rootUrl"),
                                            Type = textMappingsManager.GetSubstitution(MigrationHandler.GetJsonValue<string>(dataSource.Value, "type"))
                                        };

                                        if (!string.IsNullOrWhiteSpace(dataSourceDefinition.ConnectionString))
                                        {
                                            reportDefinition.DataSources.TryAdd(dataSource.Key, dataSourceDefinition);
                                        }
                                        else
                                        {
                                            this.Logger.LogWarning($"Failed to migrate data source '{dataSource.Key}' from '{legacyReportFile.FullName}' since a connection string could not be found or determined.");
                                        }
                                    }, $"Error processing '{dataSource.Key}' in report '{legacyReportFile.FullName}'.");
                                }
                            }
                        }

                        this.Logger.LogTrace($"Processing {queries?.Count ?? 0} queries for '{legacyReportFile.FullName}'.");

                        if (null != queries && 0 < queries.Count)
                        {
                            reportDefinition.Commands = new StringKeyDictionary<CommandDefinition>();
                            foreach (var query in queries)
                            {
                                CommandDefinition commandDefinition = new CommandDefinition()
                                {
                                    CommandText = MigrationHandler.GetJsonValue<string>(query.Value, "command")
                                };

                                base.RunWithStepOver(() =>
                                {
                                    reportDefinition.Commands.Add(query.Key, commandDefinition);
                                }, $"Error processing '{query.Key}' in report '{legacyReportFile.FullName}'.");
                            }
                        }

                        this.Logger.LogTrace($"Processing {filters?.Count ?? 0} filters for '{legacyReportFile.FullName}'.");

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

                                string filterType = MigrationHandler.GetJsonValue<string>(filter, "type");

                                if ("Context".Equals(filterType, StringComparison.OrdinalIgnoreCase))
                                {
                                    filterDefinition.Properties = FilterProperties.Context;
                                }

                                MigrationHandler.ConvertFilterOptions(filter, filterDefinition, textMappingsManager);

                                if ("CurrentUser".Equals(MigrationHandler.GetJsonValue<string>(filter, "rendererOptions.mvc.view"), StringComparison.OrdinalIgnoreCase))
                                {
                                    filterDefinition.Properties = FilterProperties.Context;
                                    filterDefinition.DefaultValue = "CurrentUserId";

                                    if (filterDefinition.Options is JObject jobjectOptions)
                                    {
                                        var editorProperty = jobjectOptions.Property("editor", StringComparison.OrdinalIgnoreCase);
                                        editorProperty?.Remove();
                                    }
                                }

                                base.RunWithStepOver(() =>
                                {
                                    reportDefinition.Filters.Add(filterName, filterDefinition);
                                }, $"Error processing '{filterName}' in report '{legacyReportFile.FullName}'.");
                            }
                        }

                        this.Logger.LogTrace($"Processing output for '{legacyReportFile.FullName}'.");

                        if (null != legacyOutput && legacyOutput is JObject)
                        {
                            OutputDefinition outputDefinition = new OutputDefinition();

                            outputDefinition.DataSource = MigrationHandler.GetJsonValue<string>(legacyOutput, "dataSource");
                            outputDefinition.Command = MigrationHandler.GetJsonValue<string>(legacyOutput, "query");

                            var outputFields = MigrationHandler.GetJsonValue<List<JToken>>(legacyOutput, "columnDefinitions");

                            if (null != outputFields && 0 < outputFields.Count)
                            {
                                outputDefinition.Fields = new StringKeyDictionary<FieldDefinition>();

                                foreach (var outputField in outputFields)
                                {
                                    FieldDefinition fieldDefinition = new FieldDefinition()
                                    {
                                        Name = MigrationHandler.GetJsonValue<string>(outputField, "field"),
                                        DisplayName = MigrationHandler.GetJsonValue<string>(outputField, "title")
                                    };

                                    if (outputDefinition.Fields.ContainsKey(fieldDefinition.Name))
                                    {
                                        this.Logger.LogWarning($"Output field '{fieldDefinition.Name}' is declared more than once in '{legacyReportFile.FullName}'. This instance will be skipped.");
                                        continue;
                                    }

                                    base.RunWithStepOver(() =>
                                    {
                                        outputDefinition.Fields.Add(fieldDefinition.Name, fieldDefinition);
                                    }, $"Error processing '{fieldDefinition.Name}' in report '{legacyReportFile.FullName}'.");
                                }
                            }

                            reportDefinition.Output = outputDefinition;
                        }

                        string newJson = Utilities.SafeSerialize(reportDefinition, new JsonSerializerSettings()
                        {
                            DefaultValueHandling = DefaultValueHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore,
                            Formatting = Formatting.Indented,
                            ContractResolver = new DefaultContractResolver()
                            {
                                NamingStrategy = new CamelCaseNamingStrategy(false, false, false)
                            },
                            Converters = new JsonConverter[]{
                                new ReportDefinitionConverter(),
                                new StringEnumConverter()
                            },
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });

                        string relativePath = Path.GetRelativePath(path.ToString(), legacyReportFile.ToString());

                        string outputPath = Path.Combine(output.ToString(), relativePath);
                        string outputDirectory = Path.GetDirectoryName(outputPath);

                        if (!Directory.Exists(outputDirectory))
                        {
                            Directory.CreateDirectory(outputDirectory);
                        }

                        await File.WriteAllTextAsync(outputPath, newJson, Encoding.UTF8);
                        this.Logger.LogInformation($"Report written to '{outputPath}'.");
                    }
                }
                catch (Exception error)
                {
                    this.Logger.LogError(error, $"Error processing '{legacyReportFile}'.");
                }
            }
        }
    }

    internal sealed class ReportDefinitionConverter : JsonConverter<ReportDefinition>
    {
        public override bool CanRead => false;

        public override bool CanWrite => true;

        public override ReportDefinition ReadJson(JsonReader reader, Type objectType, ReportDefinition existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, ReportDefinition value, JsonSerializer serializer)
        {
            int index = serializer.Converters.IndexOf(this);

            if (-1 < index)
            {
                serializer.Converters.RemoveAt(index);
            }

            var jobject = JObject.FromObject(value, serializer);
            jobject.AddFirst(new JProperty("$schema", "https://raw.githubusercontent.com/adriva/coreframework/master/Reporting/src/Adriva.Extensions.Reporting.Abstractions/report-schema.json"));

            jobject.WriteTo(writer, serializer.Converters.ToArray());

            if (-1 < index)
            {
                serializer.Converters.Insert(index, this);
            }
        }
    }
}
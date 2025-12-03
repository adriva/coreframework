using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Extensions.Reporting.Abstractions;
using HtmlAgilityPack;
using Microsoft.Extensions.FileProviders;
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

        private static void ConvertOutputFieldOptions(JToken filterToken, FieldDefinition fieldDefinition, TextMappingsManager textMappingsManager)
        {
            if (null == filterToken || null == fieldDefinition) return;

            if (JTokenType.Object == filterToken.Type)
            {
                if (filterToken is JObject filterObject)
                {
                    Dictionary<string, object> optionsData = new Dictionary<string, object>();

                    string rendererMethod = MigrationHandler.GetJsonValue<string>(filterObject["rendererOptions"]?["mvc"], "renderer");

                    if (!string.IsNullOrWhiteSpace(rendererMethod))
                    {
                        optionsData["formatter"] = rendererMethod;
                    }

                    if (0 < optionsData.Count)
                    {
                        fieldDefinition.Options = JToken.FromObject(optionsData);
                    }
                }
            }
        }

        private static IEnumerable<Esprima.Ast.FunctionDeclaration> FindAllJavascriptFunctions(Esprima.Ast.ChildNodes nodes)
        {
            if (!nodes.Any())
            {
                yield break;
            }

            foreach (var node in nodes)
            {
                if (null == node)
                {
                    continue;
                }

                if (Esprima.Ast.Nodes.FunctionDeclaration == node.Type)
                {
                    yield return (Esprima.Ast.FunctionDeclaration)node;
                }

                foreach (var childNode in MigrationHandler.FindAllJavascriptFunctions(node.ChildNodes))
                {
                    yield return childNode;
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
        [CommandArgument("--local-schema", Aliases = new[] { "-ls" }, IsRequired = false, Type = typeof(string), Description = "Overrides default json schema and uses the local schema when generating report definition files. This setting should always be relative to the input path.")]
        [CommandArgument("--output", Aliases = new[] { "-o" }, IsRequired = false, Type = typeof(DirectoryInfo), Description = "Output folder where the new report will be generated.")]
        public async Task InvokeAsync(DirectoryInfo path, DirectoryInfo output, string report, FileInfo mappings, string localSchema)
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

            IFileInfo localSchemaFile = null;

            if (!string.IsNullOrWhiteSpace(localSchema))
            {
                using (PhysicalFileProvider physicalFileProvider = new PhysicalFileProvider(path.FullName))
                {
                    localSchemaFile = physicalFileProvider.GetFileInfo(localSchema);

                    if (!localSchemaFile.Exists || localSchemaFile.IsDirectory)
                    {
                        this.Logger.LogError($"Provided local schema file '{localSchema}' could not be found. Tried '{localSchemaFile.PhysicalPath}'");
                        return;
                    }
                }
            }

            bool hasWildcard = report.Contains("*", StringComparison.Ordinal);
            TextMappingsManager textMappingsManager = new TextMappingsManager(mappings);
            await textMappingsManager.InitializeAsync();

            FileInfo[] legacyReportFiles = path.GetFiles($"{report}.json", hasWildcard ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            foreach (var legacyReportFile in legacyReportFiles)
            {
                if (null != localSchemaFile && localSchemaFile.PhysicalPath.Equals(legacyReportFile.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

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
                                            ConnectionString = MigrationHandler.GetJsonValue<string>(dataSource.Value["parameters"], "connectionString")
                                                                ??
                                                                MigrationHandler.GetJsonValue<string>(dataSource.Value["parameters"], "rootUrl"),
                                            Type = textMappingsManager.GetSubstitution(MigrationHandler.GetJsonValue<string>(dataSource.Value, "type"))
                                        };

                                        if (Uri.TryCreate(dataSourceDefinition.ConnectionString, UriKind.Absolute, out Uri dataSourceUri))
                                        {
                                            if (Utilities.IsValidHttpUri(dataSourceUri))
                                            {
                                                dataSourceDefinition.ConnectionString = dataSourceDefinition.ConnectionString.EndsWith('/') ? dataSourceDefinition.ConnectionString : dataSourceDefinition.ConnectionString + '/';
                                            }
                                        }

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

                                commandDefinition.CommandText = textMappingsManager.GetCommandSubstitution(commandDefinition.CommandText?.TrimStart('/'));

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
                                string filterName = textMappingsManager.GetSubstitution(MigrationHandler.GetJsonValue<string>(filter, "name"), true);
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
                                        DisplayName = MigrationHandler.GetJsonValue<string>(outputField, "title"),
                                        DataType = MigrationHandler.GetJsonValue<TypeCode>(outputField, "dataType"),
                                        Format = MigrationHandler.GetJsonValue<string>(outputField, "format"),
                                    };

                                    string fieldName = MigrationHandler.GetJsonValue<string>(outputField, "field") ?? string.Empty;

                                    if (outputDefinition.Fields.ContainsKey(fieldName))
                                    {
                                        this.Logger.LogWarning($"Output field '{fieldDefinition.Name}' is declared more than once in '{legacyReportFile.FullName}'. This instance will be skipped.");
                                        continue;
                                    }

                                    MigrationHandler.ConvertOutputFieldOptions(outputField, fieldDefinition, textMappingsManager);

                                    string mvcTemplate = outputField["rendererOptions"]?["mvc"]?["template"]?.Value<string>();

                                    if (!string.IsNullOrWhiteSpace(mvcTemplate))
                                    {
                                        if (null == fieldDefinition.Options)
                                        {
                                            fieldDefinition.Options = JToken.Parse("{}");
                                        }

                                        fieldDefinition.Options["formatter"] = mvcTemplate;
                                    }
                                    base.RunWithStepOver(() =>
                                    {
                                        outputDefinition.Fields.Add(fieldName, fieldDefinition);
                                    }, $"Error processing '{fieldDefinition.Name}' in report '{legacyReportFile.FullName}'.");
                                }
                            }

                            reportDefinition.Output = outputDefinition;
                        }

                        string relativePath = Path.GetRelativePath(path.ToString(), legacyReportFile.ToString());
                        string outputPath = Path.Combine(output.ToString(), relativePath);
                        string outputDirectory = Path.GetDirectoryName(outputPath);
                        string jsonSchemaLocation = "https://raw.githubusercontent.com/adriva/coreframework/master/Reporting/src/Adriva.Extensions.Reporting.Abstractions/report-schema.json";

                        if (null != localSchemaFile)
                        {
                            jsonSchemaLocation = Path.GetRelativePath(Path.GetDirectoryName(legacyReportFile.FullName), localSchemaFile.PhysicalPath);
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
                                new ReportDefinitionConverter(jsonSchemaLocation),
                                new StringEnumConverter()
                            },
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });

                        if (!Directory.Exists(outputDirectory))
                        {
                            Directory.CreateDirectory(outputDirectory);
                        }

                        await File.WriteAllTextAsync(outputPath, newJson, Encoding.UTF8);
                        this.Logger.LogInformation($"Report written to '{outputPath}'.");

                        try
                        {
                            await this.ExtractCustomViewScriptsAsync(legacyReportFile, outputDirectory);
                        }
                        catch (Exception scriptExtractionError)
                        {
                            this.Logger.LogError(scriptExtractionError, $"Error extracting scripts from '{legacyReportFile.FullName}'.");
                        }
                    }
                }
                catch (Exception error)
                {
                    this.Logger.LogError(error, $"Error processing '{legacyReportFile}'.");
                }
            }
        }

        private async Task ExtractCustomViewScriptsAsync(FileInfo legacyReportFile, string outputDirectory)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(legacyReportFile.FullName);
            string cshtmlFile = Path.Combine(legacyReportFile.DirectoryName, $"{fileNameWithoutExtension}.cshtml");

            if (!File.Exists(cshtmlFile))
            {
                return;
            }

            HtmlDocument cshtmlDocument = new HtmlDocument();
            cshtmlDocument.Load(cshtmlFile);

            var nodes = cshtmlDocument.DocumentNode.SelectNodes("//script");

            if (null == nodes || 0 == nodes.Count)
            {
                return;
            }

            StringBuilder outputBuffer = new StringBuilder();

            nodes
                .Where(n => 0 < n.InnerLength)
                .ForEach((index, node) =>
                {
                    var parser = new Esprima.JavaScriptParser();
                    var javascript = parser.ParseScript(node.InnerText);

                    var scriptNodes = MigrationHandler.FindAllJavascriptFunctions(javascript.ChildNodes).ToArray();

                    var topLevelNodes = scriptNodes
                                    .Where(x => !scriptNodes.Any(y => x != y && x.Range.Start > y.Range.Start && x.Range.End < y.Range.End))
                                    .Where(x => !x.Id.Name.Equals("gridReady", StringComparison.Ordinal));

                    foreach (var topLevelNode in topLevelNodes)
                    {
                        string code = Esprima.Utils.AstToJavaScript.ToJavaScriptString(topLevelNode, new Esprima.Utils.KnRJavaScriptTextFormatterOptions()
                        {
                            Indent = "\t",
                            KeepEmptyBlockBodyInLine = false
                        });
                        if (!string.IsNullOrWhiteSpace(code))
                        {
                            outputBuffer.Append(code);
                            outputBuffer.Append(Environment.NewLine);
                            outputBuffer.Append(Environment.NewLine);
                        }
                    }
                });

            if (0 < outputBuffer.Length)
            {
                string outputPath = Path.Combine(outputDirectory, $"{fileNameWithoutExtension}.js");
                await File.WriteAllTextAsync(outputPath, outputBuffer.ToString(), Encoding.UTF8);
                this.Logger.LogInformation($"Scripts from '{cshtmlFile}' for report '{legacyReportFile.FullName}' extracted to '{outputPath}'.");
            }
        }
    }
}
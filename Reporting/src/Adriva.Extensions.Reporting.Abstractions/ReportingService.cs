using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Adriva.Extensions.Caching.Abstractions;
using Adriva.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Adriva.Extensions.Reporting.Abstractions
{
    internal class ReportingService : IReportingService
    {
        private readonly IServiceProvider ServiceProvider;
        private readonly ICommandBuilder CommandBuilder;
        private readonly ICache Cache;
        private readonly ReportingServiceOptions Options;
        private readonly IEnumerable<IReportRepository> Repositories;

        public ReportingService(IServiceProvider serviceProvider,
                                                        ICommandBuilder commandBuilder,
                                                        IEnumerable<IReportRepository> repositories,
                                                        IOptions<ReportingServiceOptions> optionsAccessor)
        {
            this.ServiceProvider = serviceProvider;
            this.CommandBuilder = commandBuilder;
            this.Options = optionsAccessor.Value;
            this.Repositories = repositories;

            if (this.Options.UseCache)
            {
                this.Cache = this.ServiceProvider.GetRequiredService<ICache<InMemoryCache>>().Instance;
            }
            else
            {
                this.Cache = new NullCache();
            }
        }

        private void FixFilterDefinitions(ReportDefinition reportDefinition, IDictionary<string, FilterDefinition> filterDefinitions)
        {
            if (null == filterDefinitions) return;

            foreach (var entry in filterDefinitions)
            {
                if (null == entry.Value)
                {
                    throw new ArgumentNullException($"FilterDefinition for filter '{entry.Key}' is not set to an instance of an object.");
                }
                entry.Value.Name = entry.Key;

                foreach (var fieldEntry in entry.Value.Fields)
                {
                    fieldEntry.Value.Name = fieldEntry.Key;
                }

                this.FixFilterDefinitions(reportDefinition, entry.Value.Children);
            }
        }

        private async Task FixFilterDefaultValuesAsync(ReportContext reportContext, bool isExecuting = false)
        {
            var filters = reportContext.ReportDefinition.EnumerateFilterDefinitions();

            foreach (var filter in filters)
            {
                if (!isExecuting)
                {
                    if (null != filter.DefaultValue && !string.IsNullOrWhiteSpace(reportContext.ReportDefinition.ContextProvider))
                    {
                        filter.DefaultValue = await Helpers.GetFilterValueFromContextAsync(reportContext, filter, this.Cache);
                    }
                }

                if (!string.IsNullOrWhiteSpace(filter.DefaultValueFormatter))
                {
                    filter.DefaultValue = Helpers.ApplyMethodFormatter(reportContext.ReportDefinition, filter);
                }
            }
        }

        private void FixFieldDefinitions(OutputDefinition outputDefinition)
        {
            if (null == outputDefinition?.Fields) return;

            foreach (var fieldEntry in outputDefinition.Fields)
            {
                fieldEntry.Value.Name = fieldEntry.Key;
            }
        }

        public Task<ReportDefinition> LoadReportDefinitionAsync(string name) => this.LoadReportDefinitionAsync(name, false);

        private async Task<ReportDefinition> LoadReportDefinitionAsync(string name, bool isExecuting = false)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            ReportDefinition reportDefinition = await this.Cache.GetOrCreateAsync($"ReportingService:LoadReportDefinitionAsync:{name}", async (entry) =>
            {
                entry.AbsoluteExpirationRelativeToNow = this.Options.DefinitionTimeToLive;

                foreach (var repository in this.Repositories)
                {
                    reportDefinition = await repository.LoadReportDefinitionAsync(name);
                    if (null != reportDefinition)
                    {
                        using (var scope = this.ServiceProvider.CreateScope())
                        using (ReportContext reportContext = ReportContext.Create(scope.ServiceProvider, reportDefinition))
                        {
                            this.FixFilterDefinitions(reportDefinition, reportDefinition.Filters);
                            await this.FixFilterDefaultValuesAsync(reportContext, isExecuting);
                            this.FixFieldDefinitions(reportDefinition.Output);
                            return reportDefinition;
                        }
                    }
                }

                return null;
            });

            /*
            need to return the clone of the definition since multiple clients might be
            accessing the cached instance and modifying its state
            by cloning we make sure each client gets its own copy of the definition
            */
            return reportDefinition?.Clone() ?? throw new InvalidOperationException();
        }

        private async Task PopulateFilterValuesAsync(ReportDefinition reportDefinition, FilterDefinition filterDefinition, FilterValuesDictionary values)
        {
            if (null == filterDefinition || string.IsNullOrWhiteSpace(filterDefinition.DataSource))
            {
                return;
            }

            var output = await this.GetDataAsync(reportDefinition, filterDefinition, filterDefinition.EnumerateFieldDefinitions(), filterDefinition.Command, values);
            filterDefinition.Data = output.DataSet;
        }

        private async Task<ReportOutput> GetDataAsync(ReportDefinition reportDefinition, IDataDrivenObject dataSourceScope, IEnumerable<FieldDefinition> fieldDefinitions, string commandName, FilterValuesDictionary values)
        {
            using (IServiceScope serviceScope = this.ServiceProvider.CreateScope())
            using (ReportCommandContext context = ReportCommandContext.Create(serviceScope.ServiceProvider, reportDefinition, commandName))
            {
                var reportCommand = await this.CommandBuilder.BuildCommandAsync(context, values, false);

                if (!reportDefinition.TryFindDataSourceDefinition(dataSourceScope, out DataSourceDefinition dataSourceDefinition))
                {
                    throw new InvalidOperationException($"Could not find data source definition '{dataSourceScope.DataSource}' in the report '{reportDefinition.Name}'.");
                }

                var dataSourceRegistrationOptionsSnapshot = serviceScope.ServiceProvider.GetRequiredService<IOptionsSnapshot<DataSourceRegistrationOptions>>();
                var dataSourceRegistrationOptions = dataSourceRegistrationOptionsSnapshot.Get(dataSourceDefinition.Type);

                if (null == dataSourceRegistrationOptions.TypeHandle || IntPtr.Zero == dataSourceRegistrationOptions.TypeHandle.Value)
                {
                    throw new InvalidOperationException($"No data source service is registered for data source type '{dataSourceDefinition.Type}'.");
                }

                Type dataSourceType = Type.GetTypeFromHandle(dataSourceRegistrationOptions.TypeHandle);
                IDataSource dataSource = (IDataSource)serviceScope.ServiceProvider.GetRequiredService(dataSourceType);
                await dataSource.OpenAsync(dataSourceDefinition);

                try
                {
                    var dataset = await dataSource.GetDataAsync(reportCommand, fieldDefinitions.ToArray(), reportDefinition.Output.Options);

                    if (null != context.PostProcessor)
                    {
                        await context.PostProcessor.PostProcessAsync(dataset, reportCommand);
                    }

                    if (this.Options.AllowSensitiveData)
                    {
                        return new ReportOutput(reportCommand, dataset);
                    }
                    else
                    {
                        return new ReportOutput(new ReportCommand(commandName, new CommandDefinition() { CommandText = commandName }), dataset);
                    }
                }
                finally
                {
                    await dataSource.CloseAsync();
                }
            }
        }

        private async Task<ReportOutput> ExecuteReportOutputAsync(ReportDefinition reportDefinition, FilterValuesDictionary values)
        {
            return await this.GetDataAsync(reportDefinition, reportDefinition.Output, reportDefinition.EnumerateFieldDefinitions(), reportDefinition.Output.Command, values);
        }

        public async Task<ReportOutput> ExecuteReportOutputAsync(string name, FilterValuesDictionary values)
        {
            var reportDefinition = await this.LoadReportDefinitionAsync(name, true);
            return await this.ExecuteReportOutputAsync(reportDefinition, values);
        }

        public async Task<DataSet> GetFilterDataAsync(ReportDefinition reportDefinition, string filterName, FilterValuesDictionary values)
        {
            if (!reportDefinition.TryFindFilterDefinition(filterName, out FilterDefinition filterDefinition))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(filterDefinition.DataSource))
            {
                return null;
            }

            if (null == values)
            {
                values = [];
            }

            foreach (var filterDefinitionEntry in reportDefinition.Filters)
            {
                Queue<FilterDefinition> filterQueue = new();
                filterQueue.Enqueue(filterDefinitionEntry.Value);

                while (0 < filterQueue.Count)
                {
                    if (filterQueue.TryDequeue(out FilterDefinition filter))
                    {
                        if (!values.ContainsKey(filter.Name) && null != filter.DefaultValue)
                        {
                            values[filter.Name] = Convert.ToString(filter.DefaultValue);
                        }

                        if (null != filter.Children)
                        {
                            foreach (var childFilter in filter.Children)
                            {
                                filterQueue.Enqueue(childFilter.Value);
                            }
                        }
                    }
                }
            }

            var output = await this.GetDataAsync(reportDefinition, filterDefinition, filterDefinition.EnumerateFieldDefinitions(), filterDefinition.Command, values);
            return output.DataSet;
        }

        public async Task PopulateFilterValuesAsync(ReportDefinition reportDefinition, FilterValuesDictionary values)
        {
            if (null == reportDefinition)
            {
                throw new ArgumentNullException(nameof(reportDefinition), "Report definition is not set to an instance of an object");
            }

            if (null == reportDefinition.Filters || 0 == reportDefinition.Filters.Count)
            {
                return;
            }

            if (null == values)
            {
                values = [];
            }

            foreach (var filterDefinition in reportDefinition.Filters)
            {
                Queue<FilterDefinition> filterQueue = new();
                filterQueue.Enqueue(filterDefinition.Value);

                while (0 < filterQueue.Count)
                {
                    if (filterQueue.TryDequeue(out FilterDefinition filter))
                    {
                        await this.PopulateFilterValuesAsync(reportDefinition, filter, values);

                        if (!values.ContainsKey(filter.Name) && null != filter.DefaultValue)
                        {
                            values[filter.Name] = Convert.ToString(filter.DefaultValue);
                        }

                        if (null != filter.Children)
                        {
                            foreach (var childFilter in filter.Children)
                            {
                                filterQueue.Enqueue(childFilter.Value);
                            }
                        }
                    }
                }
            }
        }

        public async ValueTask RenderAsync<TRenderer>(string name, FilterValuesDictionary values, Stream stream, Func<TRenderer, ReportDefinition, ReportOutput, Task> interceptor = null) where TRenderer : ReportRenderer
        {
            var definition = await this.LoadReportDefinitionAsync(name, true);
            var output = await this.ExecuteReportOutputAsync(definition, values);

            using (var serviceScope = this.ServiceProvider.CreateScope())
            {
                var renderer = serviceScope.ServiceProvider
                                            .GetServices<ReportRenderer>()
                                            .OfType<TRenderer>()
                                            .FirstOrDefault();

                if (null == renderer)
                {
                    throw new ArgumentException($"System cannot find a report renderer of type '{typeof(TRenderer).FullName}'. Did you forget to call reportingBuilder.AddRenderer<T>() ?");
                }

                if (null != interceptor)
                {
                    await interceptor(renderer, definition, output);
                }

                await renderer.RenderAsync(string.IsNullOrWhiteSpace(definition.Name) ? name : definition.Name, definition.Output, output, stream);
            }
        }

        public async Task<ReportOutput> ExecuteCommandAsync(string name, string commandName, FilterValuesDictionary values, string dataSourceName = null)
        {
            var reportDefinition = await this.LoadReportDefinitionAsync(name, false);
            var dataSourceScope = reportDefinition.Output;
            dataSourceName = string.IsNullOrWhiteSpace(dataSourceName) ? dataSourceScope.DataSource : dataSourceName;

            using (IServiceScope serviceScope = this.ServiceProvider.CreateScope())
            using (ReportCommandContext context = ReportCommandContext.Create(serviceScope.ServiceProvider, reportDefinition, commandName))
            {
                var reportCommand = await this.CommandBuilder.BuildCommandAsync(context, values, true);

                if (!reportDefinition.TryFindDataSourceDefinition(dataSourceName, out DataSourceDefinition dataSourceDefinition))
                {
                    throw new InvalidOperationException($"Could not find data source definition '{dataSourceName}' in the report '{reportDefinition.Name}'.");
                }

                var dataSourceRegistrationOptionsSnapshot = serviceScope.ServiceProvider.GetRequiredService<IOptionsSnapshot<DataSourceRegistrationOptions>>();
                var dataSourceRegistrationOptions = dataSourceRegistrationOptionsSnapshot.Get(dataSourceDefinition.Type);

                if (null == dataSourceRegistrationOptions.TypeHandle || IntPtr.Zero == dataSourceRegistrationOptions.TypeHandle.Value)
                {
                    throw new InvalidOperationException($"No data source service is registered for data source type '{dataSourceDefinition.Type}'.");
                }

                Type dataSourceType = Type.GetTypeFromHandle(dataSourceRegistrationOptions.TypeHandle);
                IDataSource dataSource = (IDataSource)serviceScope.ServiceProvider.GetRequiredService(dataSourceType);

                if (!(dataSource is IExecutableDataSource excutableDataSource))
                {
                    throw new NotSupportedException($"Data source '{dataSourceName}' does not support exexcuting ad-hoc commands. Ad-hoc command execution a data source of type {nameof(IExecutableDataSource)}.");
                }

                await excutableDataSource.OpenAsync(dataSourceDefinition);

                try
                {
                    var output = await excutableDataSource.ExecuteAsync(reportCommand);
                    var dataset = DataSet.FromColumnNames("Output");

                    var row = dataset.CreateRow();
                    row.AddData(output);

                    if (this.Options.AllowSensitiveData)
                    {
                        return new ReportOutput(reportCommand, dataset);
                    }
                    else
                    {
                        return new ReportOutput(new ReportCommand(commandName, new CommandDefinition() { CommandText = commandName }), dataset);
                    }
                }
                finally
                {
                    await excutableDataSource.CloseAsync();
                }
            }
        }
    }
}
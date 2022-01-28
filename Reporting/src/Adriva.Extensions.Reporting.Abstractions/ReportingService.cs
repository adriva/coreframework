using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Adriva.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Adriva.Extensions.Caching.Memory;

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

        private void FixFilterDefinitions(IDictionary<string, FilterDefinition> filterDefinitions)
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

                this.FixFilterDefinitions(entry.Value.Children);
            }
        }

        private void FixFieldDefinitions(OutputDefinition outputDefinition)
        {
            if (null == outputDefinition) return;

            foreach (var fieldEntry in outputDefinition.Fields)
            {
                fieldEntry.Value.Name = fieldEntry.Key;
            }
        }

        public async Task<ReportDefinition> LoadReportDefinitionAsync(string name)
        {
            ReportDefinition reportDefinition = await this.Cache.GetOrCreateAsync($"ReportingService:LoadReportDefinitionAsync:{name}", async (entry) =>
            {
                entry.AbsoluteExpirationRelativeToNow = this.Options.DefinitionTimeToLive;
                foreach (var repository in this.Repositories)
                {
                    reportDefinition = await repository.LoadReportDefinitionAsync(name);
                    if (null != reportDefinition)
                    {
                        this.FixFilterDefinitions(reportDefinition.Filters);
                        this.FixFieldDefinitions(reportDefinition.Output);
                        return reportDefinition;
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

        private async Task<ReportOutput> GetDataAsync(ReportDefinition reportDefinition, IDataDrivenObject dataSourceScope, IEnumerable<FieldDefinition> fieldDefinitions, string commandName, IDictionary<string, string> values)
        {
            ReportCommandContext context = new ReportCommandContext(reportDefinition, commandName);

            if (!string.IsNullOrWhiteSpace(reportDefinition.ContextProvider))
            {
                Type contextProviderType = Type.GetType(reportDefinition.ContextProvider, true, true);
                context.ContextProvider = ActivatorUtilities.CreateInstance(this.ServiceProvider, contextProviderType);
            }

            var reportCommand = await this.CommandBuilder.BuildCommandAsync(context, values);

            using (IServiceScope serviceScope = this.ServiceProvider.CreateScope())
            {
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
                    var dataset = await dataSource.GetDataAsync(reportCommand, fieldDefinitions.ToArray());
                    return new ReportOutput(reportCommand, dataset);
                }
                finally
                {
                    await dataSource.CloseAsync();
                }
            }
        }

        public async Task<ReportOutput> ExecuteReportOutputAsync(ReportDefinition reportDefinition, IDictionary<string, string> values)
        {
            return await this.GetDataAsync(reportDefinition, reportDefinition.Output, reportDefinition.EnumerateFieldDefinitions(), reportDefinition.Output.Command, values);
        }

        private async Task PopulateFilterValuesAsync(ReportDefinition reportDefinition, FilterDefinition filterDefinition, IDictionary<string, string> values)
        {
            if (null == filterDefinition || string.IsNullOrWhiteSpace(filterDefinition.DataSource))
            {
                return;
            }

            var output = await this.GetDataAsync(reportDefinition, filterDefinition, filterDefinition.EnumerateFieldDefinitions(), filterDefinition.Command, values);
        }

        public async Task PopulateFilterValuesAsync(ReportDefinition reportDefinition, IDictionary<string, string> values)
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
                values = new Dictionary<string, string>();
            }

            foreach (var filterDefinition in reportDefinition.Filters)
            {
                await this.PopulateFilterValuesAsync(reportDefinition, filterDefinition.Value, values);
            }
        }
    }
}
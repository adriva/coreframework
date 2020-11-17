using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Adriva.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

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
                this.Cache = this.ServiceProvider.GetService<ICache>();
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
            accessing the cached instance and modifying it state
            by cloning we make sure each client gets its own copy of the definition
            */
            return reportDefinition?.Clone() ?? throw new InvalidOperationException();
        }

        public async Task ExecuteReportOutputAsync(ReportDefinition reportDefinition, IDictionary<string, string> values)
        {
            ReportCommandContext context = new ReportCommandContext(reportDefinition, reportDefinition.Output.Command);

            if (!string.IsNullOrWhiteSpace(reportDefinition.ContextProvider))
            {
                Type contextProviderType = Type.GetType(reportDefinition.ContextProvider, true, true);
                context.ContextProvider = ActivatorUtilities.CreateInstance(this.ServiceProvider, contextProviderType);
            }

            var reportCommand = await this.CommandBuilder.BuildCommandAsync(context, values);

            using (IServiceScope serviceScope = this.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                if (!reportDefinition.TryFindDataSourceDefinition(reportDefinition.Output, out DataSourceDefinition dataSourceDefinition))
                {
                    throw new InvalidOperationException($"Could not find data source definition '{reportDefinition.Output.DataSource}' in the report '{reportDefinition.Name}'.");
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
                    await dataSource.GetDataAsync(reportCommand, reportDefinition.EnumerateFieldDefinitions().ToArray());
                }
                finally
                {
                    await dataSource.CloseAsync();
                }
            }
        }
    }
}
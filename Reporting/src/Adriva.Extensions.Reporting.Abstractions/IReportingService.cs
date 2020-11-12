using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Adriva.Extensions.Caching.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public interface IReportingService
    {
        Task<ReportDefinition> LoadReportDefinitionAsync(string name);
    }

    internal class ReportingService : IReportingService
    {
        private readonly IServiceProvider ServiceProvider;
        private readonly ICache Cache;
        private readonly ReportingServiceOptions Options;
        private readonly IEnumerable<IReportRepository> Repositories;

        public ReportingService(IServiceProvider serviceProvider, IEnumerable<IReportRepository> repositories, IOptions<ReportingServiceOptions> optionsAccessor)
        {
            this.ServiceProvider = serviceProvider;
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
                        return reportDefinition;
                    }
                }

                return null;
            });

            return reportDefinition ?? throw new InvalidOperationException();
        }
    }
}
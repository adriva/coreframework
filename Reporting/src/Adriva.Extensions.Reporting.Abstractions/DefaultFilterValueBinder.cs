using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Extensions.Caching.Abstractions;
using Adriva.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class DefaultFilterValueBinder : IFilterValueBinder
    {
        private readonly ICache Cache;
        private readonly ILogger Logger;

        public DefaultFilterValueBinder(IServiceProvider serviceProvider, ILogger<DefaultFilterValueBinder> logger)
        {
            var cacheWrapper = serviceProvider.GetService<ICache<InMemoryCache>>();

            if (null == cacheWrapper?.Instance) this.Cache = new NullCache();
            else this.Cache = cacheWrapper.Instance;

            this.Logger = logger;
        }

        public async Task<FilterValue> GetFilterValueAsync(ReportContext context, FilterDefinition filterDefinition, string rawValue)
        {
            object value;

            if (FilterProperties.Constant == filterDefinition.Properties)
            {
                value = filterDefinition.DefaultValue;
            }
            else if (FilterProperties.Required == filterDefinition.Properties && null == rawValue)
            {
                if (null != filterDefinition.DefaultValue)
                {
                    value = filterDefinition.DefaultValue;
                }
                else
                {
                    throw new ArgumentNullException(filterDefinition.Name, $"Filter parameter {filterDefinition.Name} is required.");
                }
            }
            else if (FilterProperties.Context == filterDefinition.Properties)
            {
                value = await Helpers.GetFilterValueFromContextAsync(context, filterDefinition, this.Cache);
            }
            else
            {
                value = rawValue ?? filterDefinition.DefaultValue;
            }

            if (TypeCode.Empty == filterDefinition.DataType)
            {
                value = null;
            }
            else
            {
                try
                {
                    value = Convert.ChangeType(value, filterDefinition.DataType, CultureInfo.CurrentCulture);
                }
                catch (Exception conversionError)
                {
                    this.Logger.LogError(conversionError, $"Error binding filter '{filterDefinition.Name}' using raw value '{rawValue ?? "NULL"}' and assumed value '{value ?? "NULL"}'");
                    throw;
                }
            }

            return new FilterValue(rawValue, value);
        }
    }
}
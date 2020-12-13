using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Extensions.Caching.Abstractions;
using Adriva.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class DefaultFilterValueBinder : IFilterValueBinder
    {
        private readonly ICache Cache;

        public DefaultFilterValueBinder(IServiceProvider serviceProvider)
        {
            var cacheWrapper = serviceProvider.GetService<ICache<InMemoryCache>>();
            if (null == cacheWrapper?.Instance) this.Cache = new NullCache();
            else this.Cache = cacheWrapper.Instance;
        }

        public async Task<FilterValue> GetFilterValueAsync(ReportContext context, FilterDefinition filterDefinition, string rawValue)
        {
            object value;

            if (filterDefinition.Properties.HasFlag(FilterProperties.Constant)) value = filterDefinition.DefaultValue;
            else if (filterDefinition.Properties.HasFlag(FilterProperties.Required) && null == rawValue)
            {
                throw new ArgumentNullException(filterDefinition.Name, $"Filter parameter {filterDefinition.Name} is required.");
            }
            else if (filterDefinition.Properties.HasFlag(FilterProperties.Context))
            {
                string cacheKey = $"{context.ContextProvider.GetType().AssemblyQualifiedName}:{filterDefinition.DefaultValue}:Instance:Public";

                var methodInfo = await this.Cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(10);
                    return await Task.Run<MethodInfo>(() => ReflectionHelpers.FindMethod(context.ContextProvider.GetType(), Convert.ToString(filterDefinition.DefaultValue), ClrMemberFlags.Instance | ClrMemberFlags.Public));
                });

                if (null == methodInfo)
                {
                    cacheKey = $"{context.ContextProvider.GetType().AssemblyQualifiedName}:{filterDefinition.DefaultValue}:Static:Public";
                    methodInfo = await this.Cache.GetOrCreateAsync(cacheKey, async entry =>
                    {
                        entry.SlidingExpiration = TimeSpan.FromMinutes(10);
                        return await Task.Run<MethodInfo>(() => ReflectionHelpers.FindMethod(context.ContextProvider.GetType(), Convert.ToString(filterDefinition.DefaultValue), ClrMemberFlags.Static | ClrMemberFlags.Public));
                    });
                }

                if (null == methodInfo) throw new InvalidOperationException($"Filter value provider method '{filterDefinition.DefaultValue}' could not be found on context provider type '{context.ReportDefinition.ContextProvider}'.");

                value = methodInfo.Invoke(context.ContextProvider, null);
            }
            else
            {
                value = rawValue ?? filterDefinition.DefaultValue;
            }

            value = Convert.ChangeType(value, filterDefinition.DataType, CultureInfo.CurrentCulture);
            return new FilterValue(rawValue, value);
        }
    }
}
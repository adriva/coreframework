using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public static class Helpers
    {
        private static readonly MemoryCache HelperCache;

        static Helpers()
        {
            HelperCache = new MemoryCache(new MemoryCacheOptions()
            {
                SizeLimit = 1000
            });
        }

        private static bool TryGetFormatterMethod(string cacheKey, string formatter, out MethodInfo formatterMethod)
        {
            formatterMethod = HelperCache.GetOrCreate(cacheKey, (entry) =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20);
                entry.SetSize(1L);

                MethodInfo targetMethod = null;
                var match = Regex.Match(formatter, @"^(?<typeName>(\w|\.)+)\:(?<methodName>\w+)\s*\,\s*(?<assemblyName>(\w|\.)+)$");

                if (
                    match.Groups["typeName"].Success && match.Groups["assemblyName"].Success && match.Groups["methodName"].Success &&
                    !string.IsNullOrWhiteSpace(match.Groups["typeName"].Value) && !string.IsNullOrWhiteSpace(match.Groups["assemblyName"].Value) && !string.IsNullOrWhiteSpace(match.Groups["methodName"].Value)
                )
                {
                    string typeName = $"{match.Groups["typeName"].Value}, {match.Groups["assemblyName"].Value}";
                    string methodName = match.Groups["methodName"].Value;

                    Type targetType = Type.GetType(typeName, false);
                    if (null == targetType) return null;

                    targetMethod = targetType.GetMethod(methodName, BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Public);
                    if (null == targetMethod || 1 != targetMethod.GetParameters().Length)
                    {
                        targetMethod = null;
                    }
                }

                return targetMethod;
            });

            return null != formatterMethod;
        }

        public static object ApplyMethodFormatter(ReportDefinition reportDefinition, FilterDefinition filterDefinition)
        {
            if (string.IsNullOrWhiteSpace(filterDefinition?.DefaultValueFormatter)) return filterDefinition.DefaultValue;

            if (!Helpers.TryGetFormatterMethod($"DefaultValueFormatter:{reportDefinition.Name}:{filterDefinition.Name}", filterDefinition.DefaultValueFormatter, out MethodInfo formatterMethod))
            {
                return filterDefinition.DefaultValue;
            }

            return formatterMethod.Invoke(null, new object[] { filterDefinition.DefaultValue });
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Extensions.Caching.Abstractions;
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

        public static async Task<object> GetFilterValueFromContextAsync(ReportContext context, FilterDefinition filterDefinition, ICache cache)
        {
            if (null == context)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (null == filterDefinition)
            {
                throw new ArgumentNullException(nameof(filterDefinition));
            }

            if (FilterProperties.Context != filterDefinition.Properties)
            {
                return filterDefinition.DefaultValue;
            }

            string contextMemberName = Convert.ToString(filterDefinition?.DefaultValue);

            if (string.IsNullOrWhiteSpace(contextMemberName))
            {
                return filterDefinition.DefaultValue;
            }

            Type contextProviderType = context.ContextProvider.GetType();

            var contextMember = await cache.GetOrCreateAsync<MemberInfo>($"Helpers:GetFilterValueFromContextAsync:{contextProviderType.FullName}:{contextMemberName}", (entry) =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                MemberInfo memberInfo = contextProviderType.FindProperties(p => !p.IsSpecialName && p.Name.Equals(contextMemberName, StringComparison.OrdinalIgnoreCase), ClrMemberFlags.Instance | ClrMemberFlags.Public).FirstOrDefault();

                if (null == memberInfo)
                {
                    memberInfo = contextProviderType.FindProperties(p => !p.IsSpecialName && p.Name.Equals(contextMemberName, StringComparison.OrdinalIgnoreCase), ClrMemberFlags.Static | ClrMemberFlags.Public).FirstOrDefault();
                }

                if (null == memberInfo)
                {
                    memberInfo = contextProviderType.FindMethod(contextMemberName, ClrMemberFlags.Instance | ClrMemberFlags.Public);
                }

                if (null == memberInfo)
                {
                    memberInfo = contextProviderType.FindMethod(contextMemberName, ClrMemberFlags.Static | ClrMemberFlags.Public);
                }

                return Task.FromResult(memberInfo);
            });

            if (null == contextMember)
            {
                throw new ArgumentException($"Couldn't find context member '{filterDefinition.DefaultValue}' in context of type '{contextProviderType.FullName}'. Supported member types are static property, instance property, parameterless instance or static method.");
            }

            if (contextMember is PropertyInfo propertyMember)
            {
                return propertyMember.GetValue(context.ContextProvider);
            }
            else if (contextMember is MethodInfo methodMember)
            {
                return methodMember.Invoke(context.ContextProvider, null);
            }
            else
            {
                throw new InvalidOperationException($"Context member '{filterDefinition.DefaultValue}' type '{contextMember.MemberType}' is not supported. Supported member types are static property, instance property, parameterless instance or static method.");
            }
        }
    }
}
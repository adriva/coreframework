using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Runtime;

internal sealed class DefaultDynamicLinqTypeProvider : IDynamicLinqCustomTypeProvider
{
    private readonly IServiceProvider ServiceProvider;
    private readonly DefaultDynamicLinqCustomTypeProvider BaseProvider;

    public DefaultDynamicLinqTypeProvider(IServiceProvider serviceProvider, ParsingConfig parsingConfig, IRuntimeContext runtimeContext = null)
    {
        List<Type> additionalTypes = [];

        additionalTypes.Add(typeof(object));

        if (runtimeContext is not null)
        {
            foreach (var type in runtimeContext.KnownTypes)
            {
                additionalTypes.Add(type);
            }
        }

        this.ServiceProvider = serviceProvider;
        this.BaseProvider = new(parsingConfig, additionalTypes);
    }

    public HashSet<Type> GetCustomTypes() => this.BaseProvider.GetCustomTypes();

    public Dictionary<Type, List<MethodInfo>> GetExtensionMethods() => this.BaseProvider.GetExtensionMethods();

    public Type ResolveType(string typeName)
    {
        Type baseType = this.BaseProvider.ResolveType(typeName);
        baseType ??= this.ServiceProvider.GetService<IRuntimeTypeResolver>()?.Resolve(typeName);
        return baseType;
    }

    public Type ResolveTypeBySimpleName(string simpleTypeName)
    {
        Type baseType = this.BaseProvider.ResolveTypeBySimpleName(simpleTypeName);
        baseType ??= this.ServiceProvider.GetService<IRuntimeTypeResolver>()?.Resolve(simpleTypeName);
        return baseType;
    }
}

using System;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Adriva.Extensions.Runtime;

public class DefaultRuntimeTypeResolver(IRuntimeContext runtimeContext) : IRuntimeTypeResolver
{
    protected IRuntimeContext RuntimeContext { get; } = runtimeContext;

    public Type Resolve(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            throw new ArgumentNullException(nameof(typeName), "Typename is not provided or empty.");
        }

        var targetType = this.RuntimeContext.KnownTypes.FirstOrDefault(type => type.AssemblyQualifiedName.Equals(typeName, StringComparison.Ordinal));
        targetType ??= this.RuntimeContext.KnownTypes.FirstOrDefault(type => type.FullName.Equals(typeName, StringComparison.Ordinal));
        targetType ??= this.RuntimeContext.KnownTypes.FirstOrDefault(type => type.Name.Equals(typeName, StringComparison.Ordinal));

        return targetType;
    }
}

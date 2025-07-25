using System;
using System.Collections.Generic;
using System.Linq;

namespace Adriva.Extensions.Runtime;

internal sealed class RuntimeContext : IRuntimeContext
{
    private readonly HashSet<Type> WellKnownTypes = [];

    public IEnumerable<Type> KnownTypes => this.WellKnownTypes.AsEnumerable();

    public IRuntimeContext AddKnownType(Type type)
    {
        this.WellKnownTypes.Add(type);
        return this;
    }
}

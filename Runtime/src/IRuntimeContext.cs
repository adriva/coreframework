using System;
using System.Collections.Generic;

namespace Adriva.Extensions.Runtime;

public interface IRuntimeContext
{
    IEnumerable<Type> KnownTypes { get; }

    IRuntimeContext AddKnownType(Type type);
}

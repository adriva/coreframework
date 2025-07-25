using System;

namespace Adriva.Extensions.Runtime;

public interface IRuntimeTypeResolver
{
    Type Resolve(string typeName);
}

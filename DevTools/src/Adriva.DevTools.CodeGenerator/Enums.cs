using System;

namespace Adriva.DevTools.CodeGenerator
{
    [Flags]
    public enum AccessModifier : long
    {
        None = 0,
        Internal = 1 << 0,
        Public = 1 << 1,
        Protected = 1 << 2,
        Private = 1 << 3,
        Sealed = 1 << 4,
        Abstract = 1 << 5,
        Partial = 1 << 6
    }
}

using System;

namespace Adriva.Common.Core
{
    [Flags]
    public enum ClrMemberFlags : long
    {
        None = 0,
        Class = 1,
        Primitive = 1 << 1,
        Concrete = 1 << 2,
        SpecialName = 1 << 3,
        NotSpecialName = 1 << 4,
        Static = 1 << 5,
        Instance = 1 << 6,
        Public = 1 << 10,
        NonPublic = 1 << 11,
        Constant = 1 << 12
    }
}

using System;

namespace Adriva.Extensions.Reporting.Abstractions
{
    [Flags]
    public enum FilterProperties : long
    {
        Default = 0,
        Constant = 1,
        Context = 1 << 1,
        Required = 1 << 10
    }
}
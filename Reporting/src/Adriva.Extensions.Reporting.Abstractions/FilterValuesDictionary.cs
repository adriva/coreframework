using System;
using System.Collections.Generic;

namespace Adriva.Extensions.Reporting.Abstractions
{
    [Serializable]
    public sealed class FilterValuesDictionary : Dictionary<string, string>
    {
        public FilterValuesDictionary() : base(StringComparer.OrdinalIgnoreCase)
        {

        }
    }
}
using System;
using System.Collections.Generic;

namespace Adriva.Common.Core
{
    public class StringKeyDictionary<T> : Dictionary<string, T>
    {
        public StringKeyDictionary() : base(StringComparer.OrdinalIgnoreCase)
        {

        }

        public StringKeyDictionary(StringComparer comparer) : base(comparer)
        {

        }
    }
}

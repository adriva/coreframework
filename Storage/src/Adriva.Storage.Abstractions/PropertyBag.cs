using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Adriva.Common.Core;

namespace Adriva.Storage.Abstractions
{
    public class PropertyBag : Dictionary<string, object>
    {
        private static readonly GenericEqualityComparer<string> KeyComparer = new GenericEqualityComparer<string>(
            (first, second) => 0 == string.Compare(first, second, StringComparison.OrdinalIgnoreCase),
            (input) => null == input ? 0 : input.GetHashCode()
        );

        public PropertyBag() : base(PropertyBag.KeyComparer)
        {

        }
    }
}
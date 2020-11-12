using System;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public sealed class FilterNameComparer : StringComparer
    {
        public override int Compare(string x, string y)
        {
            if (string.IsNullOrWhiteSpace(x) && string.IsNullOrWhiteSpace(y)) return 0;
            if (string.IsNullOrWhiteSpace(x)) return -1;
            if (string.IsNullOrWhiteSpace(y)) return 1;

            if (x.StartsWith("@", StringComparison.Ordinal)) x = x.Substring(1);
            if (y.StartsWith("@", StringComparison.Ordinal)) y = y.Substring(1);

            return string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(string x, string y)
        {
            return 0 == this.Compare(x, y);
        }

        public override int GetHashCode(string obj)
        {
            if (string.IsNullOrWhiteSpace(obj)) return 0;
            if (obj.StartsWith("@", StringComparison.Ordinal)) obj = obj.Substring(1);
            return obj.GetHashCode();
        }
    }
}
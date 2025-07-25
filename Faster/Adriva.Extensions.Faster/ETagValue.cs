using System;
using Adriva.Common.Core;

namespace Adriva.Extensions.Faster
{
    internal readonly ref struct ETagValue
    {
        private readonly string Value;

        public static bool IsAny(ETagValue etag)
        {
            return "*".Equals(etag, StringComparison.Ordinal);
        }

        public static ETagValue Create()
        {
            return new ETagValue(Utilities.GetBaseString(Guid.NewGuid().ToByteArray(), Utilities.Base63Alphabet));
        }

        private ETagValue(string value)
        {
            this.Value = value;
        }

        public static implicit operator string(ETagValue eTag)
        {
            return eTag.Value;
        }

        public static implicit operator ETagValue(string value)
        {
            return new ETagValue(value);
        }
    }
}
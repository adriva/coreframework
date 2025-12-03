using System.Collections.Generic;

namespace Adriva.Extensions.TextSearch
{
    public class RawDataResult
    {
        public IEnumerable<object> Items;

        public object LastRowId;

        public bool HasMore;
    }
}

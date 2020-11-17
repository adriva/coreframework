using Adriva.Common.Core;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public sealed class FilterDefinitionDictionary : StringKeyDictionary<FilterDefinition>
    {
        public FilterDefinitionDictionary() : base(new FilterNameComparer())
        {

        }

    }
}